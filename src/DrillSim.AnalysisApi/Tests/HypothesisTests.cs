using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;
using Microsoft.Data.Sqlite;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class HypothesisTests
{
    [Test]
    public async Task Create_RestartScopedListingUniqueNamesAndIdempotentReplayPinTheOriginalPackage()
    {
        using var f = new HypothesisFixture();
        HypothesisCreateRequest request = await f.RequestAsync();
        HypothesisRevision first = await f.Service.CreateAsync(request, "create");
        f.ChangeLivePackage();
        HypothesisRevision replay = await f.Service.CreateAsync(request, "create");
        HypothesisRevision restart = await f.Restart().GetAsync(f.LiveScope, new(first.HypothesisId, 1));
        Assert.That(PredictionJson.Canonicalize(replay), Is.EqualTo(PredictionJson.Canonicalize(first)));
        Assert.That(PredictionJson.Canonicalize(restart), Is.EqualTo(PredictionJson.Canonicalize(first)));
        Assert.That((await f.Service.ListAsync(f.LiveScope, null)).Single().HypothesisId, Is.EqualTo(first.HypothesisId));
        Assert.That(restart.Package.Field["newEvidence"], Is.Null);
        Assert.That(Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.CreateAsync(
            request with { Input = request.Input with { Rationale = "Different" } }, "create"))!.StatusCode, Is.EqualTo(409));
        HypothesisCreateRequest refreshed = await f.RequestAsync(name: "  PRIMARY  ");
        Assert.That(Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.CreateAsync(refreshed, "duplicate"))!.StatusCode, Is.EqualTo(409));
        HypothesisScope scenarioScope = await f.ScenarioScopeAsync();
        await f.Service.CreateAsync(await f.RequestAsync(scope: scenarioScope), "same-name-different-scope");
        Assert.That(await f.Service.ListAsync(scenarioScope, null), Has.Count.EqualTo(1));
        Assert.That(await f.Service.ListAsync(f.LiveScope, null), Has.Count.EqualTo(1));
        Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.GetAsync(scenarioScope, new(first.HypothesisId, 1)));
    }

    [Test]
    public async Task Revise_RequiresExactPreconditionPreservesHistoryAndAllowsOnlyOneConcurrentWinner()
    {
        using var f = new HypothesisFixture();
        HypothesisCreateRequest request = await f.RequestAsync();
        HypothesisRevision first = await f.Service.CreateAsync(request, "create");
        var edit = new HypothesisReviseRequest(f.LiveScope, request.Input with { Rationale = "Second interpretation." });
        Assert.That(Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.ReviseAsync(first.HypothesisId, edit, null, "missing"))!.StatusCode, Is.EqualTo(428));
        HypothesisRevision second = await f.Service.ReviseAsync(first.HypothesisId, edit, 1, "revise");
        Assert.That(second.Revision, Is.EqualTo(2));
        Assert.That((await f.Service.GetAsync(f.LiveScope, new(first.HypothesisId, 1))).SnapshotSha256, Is.EqualTo(first.SnapshotSha256));
        Assert.That((await f.Service.ReviseAsync(first.HypothesisId, edit, 1, "revise")).SnapshotSha256, Is.EqualTo(second.SnapshotSha256));
        Assert.That(Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.ReviseAsync(first.HypothesisId, edit, 1, "stale"))!.StatusCode, Is.EqualTo(409));
        async Task<int> Attempt(string key)
        {
            try { await f.Service.ReviseAsync(first.HypothesisId, edit, 2, key); return 201; }
            catch (ScenarioApiException exception) { return exception.StatusCode; }
        }
        int[] results = await Task.WhenAll(Task.Run(() => Attempt("race-a")), Task.Run(() => Attempt("race-b")));
        Assert.That(results, Is.EquivalentTo(new[] { 201, 409 }));
        Assert.That((await f.Service.ListAsync(f.LiveScope, first.HypothesisId)).Select(item => item.Revision),
            Is.EqualTo(new[] { 3, 2, 1 }));
        Assert.ThrowsAsync<SqliteException>(() => f.ExecuteAsync("UPDATE hypothesis_revisions SET content_json='{}';"));
        Assert.ThrowsAsync<SqliteException>(() => f.ExecuteAsync("DELETE FROM hypothesis_revisions;"));
    }

    [Test]
    public async Task Branch_RecalculatesOnlyTheStoredVisibleEvidenceAndPreservesSource()
    {
        using var f = new HypothesisFixture();
        HypothesisRevision first = await f.Service.CreateAsync(await f.RequestAsync(), "create");
        f.ChangeLivePackage();
        var reference = new HypothesisReference(first.HypothesisId, 1);
        var configuration = AnalysisConfiguration.Default with { PorosityCutoff = .25, GridPointsPerAxis = 9 };
        AnalysisResult result = await f.Service.AnalyzeAsync(reference, new(f.LiveScope, configuration));
        var input = new HypothesisRevisionInput(configuration, result.PackageSha256, result.AnalysisSha256,
            result.Ranking[0].CandidateId, "Conservative branch.", "Compare the same observed column tops.",
            [new($"field:{f.Source.Package.FieldId:D}", "Annotations only; does not change controls.")]);
        HypothesisRevision branch = await f.Service.BranchAsync(new("Conservative", f.LiveScope, reference, input), "branch");
        Assert.Multiple(() =>
        {
            Assert.That(branch.BranchedFrom, Is.EqualTo(reference));
            Assert.That(branch.Revision, Is.EqualTo(1));
            Assert.That(branch.HypothesisId, Is.Not.EqualTo(first.HypothesisId));
            Assert.That(branch.Package.Sha256, Is.EqualTo(first.Package.Sha256));
            Assert.That(branch.Package.Field["newEvidence"], Is.Null);
            Assert.That(branch.Analysis.AnalysisSha256, Is.EqualTo(result.AnalysisSha256));
            Assert.That(branch.Analysis.WellSummaries.Sum(well => well.NetPayThicknessM),
                Is.LessThan(first.Analysis.WellSummaries.Sum(well => well.NetPayThicknessM)));
        });
        Assert.That((await f.Service.GetAsync(f.LiveScope, reference)).SnapshotSha256, Is.EqualTo(first.SnapshotSha256));
    }

    [Test]
    public async Task Save_RejectsChangedEvidenceHashesCandidateAndUnsupportedControlNotes()
    {
        using var f = new HypothesisFixture();
        HypothesisCreateRequest request = await f.RequestAsync();
        foreach (HypothesisRevisionInput bad in new[]
        {
            request.Input with { PackageSha256 = new string('a', 64) },
            request.Input with { AnalysisSha256 = new string('a', 64) },
            request.Input with { SelectedCandidateId = "candidate:00:00" },
            request.Input with { Configuration = request.Input.Configuration with { PorosityCutoff = double.NaN } },
            request.Input with { ControlNotes = [new($"well:{Guid.NewGuid():D}", "Not visible")] },
            request.Input with { Rationale = new string('x', 10_001) }
        })
            Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.CreateAsync(request with { Input = bad }, Guid.NewGuid().ToString()));
        Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.CreateAsync(request with { Name = " " }, "empty-name"));
        Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.CreateAsync(request with { Name = new string('x', 121) }, "long-name"));
        f.ChangeLivePackage();
        Assert.That(Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.CreateAsync(request, "changed-live"))!.StatusCode, Is.EqualTo(409));
        Assert.That(await f.Service.ListAsync(f.LiveScope, null), Is.Empty);
    }

    [Test]
    public async Task ScenarioScope_RejectsCrossFieldReservoirTimeAndPinsTheImmutableScenarioPackage()
    {
        using var f = new HypothesisFixture();
        HypothesisScope scope = await f.ScenarioScopeAsync();
        HypothesisCreateRequest request = await f.RequestAsync(scope: scope);
        foreach (HypothesisScope bad in new[]
        {
            scope with { FieldId = Guid.NewGuid() },
            scope with { ReservoirName = "Other" },
            scope with { AsOfUtc = scope.AsOfUtc!.Value.AddDays(1) },
            scope with { AsOfUtc = null },
            f.LiveScope with { AsOfUtc = HypothesisFixture.Instant },
            f.LiveScope with { FieldId = Guid.Empty }
        })
            Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.CreateAsync(request with { Scope = bad }, Guid.NewGuid().ToString()));
        f.ChangeLivePackage();
        HypothesisRevision saved = await f.Service.CreateAsync(request, "scenario");
        Assert.That(saved.Package.Sha256, Is.EqualTo(request.Input.PackageSha256));
        Assert.That(saved.Package.Field["newEvidence"], Is.Null);
        Assert.That(Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.GetAsync(f.LiveScope, new(saved.HypothesisId, 1)))!.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task Challenges_ValidateCitationsPinRevisionAndPreserveDispositionHistoryAcrossRestart()
    {
        using var f = new HypothesisFixture();
        HypothesisCreateRequest create = await f.RequestAsync();
        HypothesisRevision first = await f.Service.CreateAsync(create, "create");
        var reference = new HypothesisReference(first.HypothesisId, 1);
        var request = new HypothesisChallengeRequest(f.LiveScope, first.Analysis.AnalysisSha256, "<script>plain data</script>",
            "Reviewer", [new("Test sparse evidence support.", [$"field:{first.Scope.FieldId:D}"])]);
        HypothesisChallenge challenge = await f.Service.CreateChallengeAsync(reference, request, "challenge");
        Assert.That(challenge.Objections[0].Disposition, Is.EqualTo("open"));
        Assert.That(PredictionJson.Canonicalize(challenge), Does.Not.Contain("<script>"));
        foreach (HypothesisChallengeRequest invalid in new[]
        {
            request with { AnalysisSha256 = new string('a', 64) },
            request with { Objections = [new("Hidden reference", [$"well:{Guid.NewGuid():D}"])] },
            request with { Objections = [new("Duplicate", [$"field:{first.Scope.FieldId:D}", $"field:{first.Scope.FieldId:D}"])] },
            request with { Objections = [] }
        })
            Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.CreateChallengeAsync(reference, invalid, Guid.NewGuid().ToString()));
        var disposition = new HypothesisDispositionRequest(f.LiveScope, "Human reviewer",
            [new(challenge.Objections[0].ObjectionId, "accepted", "Account for sparse spatial support in the decision.")]);
        Assert.That(Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.DispositionAsync(reference, challenge.ChallengeId,
            disposition, null, "missing-version"))!.StatusCode, Is.EqualTo(428));
        HypothesisChallenge accepted = await f.Service.DispositionAsync(reference, challenge.ChallengeId, disposition, 1, "disposition");
        Assert.That(accepted.Version, Is.EqualTo(2));
        Assert.That(accepted.Objections[0].DispositionActor, Is.EqualTo("Human reviewer"));
        Assert.That((await f.Service.DispositionAsync(reference, challenge.ChallengeId, disposition, 1, "disposition")).Sha256, Is.EqualTo(accepted.Sha256));
        Assert.That(Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.DispositionAsync(reference, challenge.ChallengeId,
            disposition, 1, "stale"))!.StatusCode, Is.EqualTo(409));
        Assert.That((await f.Service.GetChallengeAsync(f.LiveScope, reference, challenge.ChallengeId, 1)).Objections[0].Disposition, Is.EqualTo("open"));
        Assert.That((await f.Restart().GetChallengeAsync(f.LiveScope, reference, challenge.ChallengeId)).Sha256, Is.EqualTo(accepted.Sha256));
        Assert.That((await f.Service.CreateChallengeAsync(reference, request, "challenge")).Sha256, Is.EqualTo(challenge.Sha256));
        await f.Service.ReviseAsync(first.HypothesisId, new(f.LiveScope, create.Input with { Rationale = "Next revision" }), 1, "revision");
        Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.GetChallengeAsync(f.LiveScope, new(first.HypothesisId, 2), challenge.ChallengeId));
        Assert.That(await f.Service.ListChallengesAsync(f.LiveScope, new(first.HypothesisId, 2)), Is.Empty);
        Assert.ThrowsAsync<SqliteException>(() => f.ExecuteAsync("UPDATE hypothesis_challenge_versions SET content_json='{}';"));
    }

    [TestCase("unknown", "reason")]
    [TestCase("accepted", "")]
    public async Task Dispositions_RejectInvalidStatusOrMissingHumanReason(string status, string reason)
    {
        using var f = new HypothesisFixture();
        HypothesisRevision saved = await f.Service.CreateAsync(await f.RequestAsync(), "create");
        var reference = new HypothesisReference(saved.HypothesisId, 1);
        HypothesisChallenge challenge = await f.Service.CreateChallengeAsync(reference,
            new(f.LiveScope, saved.Input.AnalysisSha256, "Review", "Reviewer", [new("Concern", [$"field:{saved.Scope.FieldId:D}"])]), "challenge");
        Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.DispositionAsync(reference, challenge.ChallengeId,
            new(f.LiveScope, "Reviewer", [new(challenge.Objections[0].ObjectionId, status, reason)]), 1, "invalid"));
    }

    [Test]
    public async Task Comparisons_AreDeterministicCompactAndFlagNonLikeForLikeEvidence()
    {
        using var f = new HypothesisFixture();
        HypothesisRevision first = await f.Service.CreateAsync(await f.RequestAsync(), "first");
        var reference = new HypothesisReference(first.HypothesisId, 1);
        AnalysisResult changed = await f.Service.AnalyzeAsync(reference,
            new(f.LiveScope, AnalysisConfiguration.Default with { PorosityCutoff = .25 }));
        HypothesisRevision second = await f.Service.BranchAsync(new("Alternate", f.LiveScope, reference,
            new(changed.Configuration, changed.PackageSha256, changed.AnalysisSha256, changed.Ranking[0].CandidateId, "Alternative")), "second");
        var selections = new[]
        {
            new HypothesisComparisonSelection(f.LiveScope, reference),
            new HypothesisComparisonSelection(f.LiveScope, new(second.HypothesisId, 1))
        };
        HypothesisComparison comparison = await f.Service.CompareAsync(new(selections));
        Assert.That((await f.Service.CompareAsync(new(selections.Reverse().ToArray()))).ComparisonSha256, Is.EqualTo(comparison.ComparisonSha256));
        Assert.That(comparison.Entries.All(item => item.LikeForLikeEvidence), Is.True);
        Assert.That(comparison.Entries.SelectMany(item => item.Differences).Any(item => item.Field == "configuration.porosityCutoff"), Is.True);
        Assert.That(comparison.Entries.All(item => item.SelectedCandidate.UncertaintyComponents!.Calibrated == false), Is.True);
        Assert.That(PredictionJson.Canonicalize(comparison), Does.Not.Contain("geologicalPropertyTable"));
        f.ChangeLivePackage();
        HypothesisRevision fresh = await f.Service.CreateAsync(await f.RequestAsync(name: "New evidence"), "fresh");
        HypothesisComparison different = await f.Service.CompareAsync(new(
            [selections[0], new(f.LiveScope, new(fresh.HypothesisId, 1))]));
        Assert.That(different.Entries.Count(item => !item.LikeForLikeEvidence), Is.EqualTo(1));
        Assert.That(different.Entries.SelectMany(item => item.ScopeDifferences), Does.Contain("different-visible-package"));
        Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.CompareAsync(new([selections[0], selections[0]])));
    }

    [TestCase("revision")]
    [TestCase("challenge")]
    [TestCase("head")]
    [TestCase("missing-head")]
    [TestCase("challenge-head")]
    public async Task Tampering_RejectsAlteredCanonicalSnapshotsChallengeVersionsAndHeadPointers(string target)
    {
        using var f = new HypothesisFixture();
        HypothesisCreateRequest request = await f.RequestAsync();
        HypothesisRevision first = await f.Service.CreateAsync(request, "create");
        var reference = new HypothesisReference(first.HypothesisId, 1);
        if (target == "revision")
        {
            await f.ExecuteAsync("DROP TRIGGER hypothesis_revisions_no_update; UPDATE hypothesis_revisions SET content_json=content_json || ' ';");
            Assert.ThrowsAsync<InvalidDataException>(() => f.Service.GetAsync(f.LiveScope, reference));
        }
        else if (target is "head" or "missing-head")
        {
            await f.Service.ReviseAsync(first.HypothesisId, new(f.LiveScope, request.Input), 1, "revise");
            await f.ExecuteAsync(target == "head" ? "UPDATE hypotheses SET head_revision=1;" : "UPDATE hypotheses SET head_revision=100;");
            Assert.ThrowsAsync<InvalidDataException>(() => f.Service.GetLatestAsync(f.LiveScope, first.HypothesisId));
            Assert.ThrowsAsync<InvalidDataException>(() => f.Service.ListAsync(f.LiveScope, null));
        }
        else
        {
            HypothesisChallenge challenge = await f.Service.CreateChallengeAsync(reference,
                new(f.LiveScope, first.Input.AnalysisSha256, "Summary", "Reviewer", [new("Concern", [$"field:{first.Scope.FieldId:D}"])]), "challenge");
            await f.ExecuteAsync(target == "challenge-head" ? "UPDATE hypothesis_challenges SET head_version=100;" :
                "DROP TRIGGER hypothesis_challenges_no_update; UPDATE hypothesis_challenge_versions SET content_json=content_json || ' ';");
            Assert.ThrowsAsync<InvalidDataException>(() => f.Service.GetChallengeAsync(f.LiveScope, reference, challenge.ChallengeId));
        }
    }

    [Test]
    public async Task TamperedPackageOrResult_CannotBeHiddenByRehashingOnlyTheSnapshotEnvelope()
    {
        using var f = new HypothesisFixture();
        HypothesisRevision first = await f.Service.CreateAsync(await f.RequestAsync(), "create");
        var alteredPackage = PredictionJson.Deserialize<AnalysisPackage>(PredictionJson.Canonicalize(first.Package), "test copy");
        alteredPackage.Field["tampered"] = true;
        HypothesisRevision packageTamper = HypothesisIntegrity.Finalize(first with { Package = alteredPackage });
        Assert.Throws<InvalidDataException>(() => HypothesisIntegrity.ReadRevision(PredictionJson.Canonicalize(packageTamper), packageTamper.SnapshotSha256));
        HypothesisRevision resultTamper = HypothesisIntegrity.Finalize(first with
        {
            Analysis = first.Analysis with { DataGaps = ["Forged"] }
        });
        Assert.Throws<InvalidDataException>(() => HypothesisIntegrity.ReadRevision(PredictionJson.Canonicalize(resultTamper), resultTamper.SnapshotSha256));
    }

    [Test]
    public async Task ConcurrentDispositions_CommitOnlyOneVersionAndPreserveTheOtherReview()
    {
        using var f = new HypothesisFixture();
        HypothesisRevision saved = await f.Service.CreateAsync(await f.RequestAsync(), "create");
        var reference = new HypothesisReference(saved.HypothesisId, 1);
        HypothesisChallenge challenge = await f.Service.CreateChallengeAsync(reference,
            new(f.LiveScope, saved.Input.AnalysisSha256, "Review", "Reviewer",
                [new("Concern", [$"field:{saved.Scope.FieldId:D}"])]), "challenge");
        async Task<int> Attempt(string status)
        {
            try
            {
                await f.Service.DispositionAsync(reference, challenge.ChallengeId,
                    new(f.LiveScope, "Reviewer", [new(challenge.Objections[0].ObjectionId, status, "Explicit reason")]), 1, status);
                return 200;
            }
            catch (ScenarioApiException exception) { return exception.StatusCode; }
        }
        int[] outcomes = await Task.WhenAll(Task.Run(() => Attempt("accepted")), Task.Run(() => Attempt("deferred")));
        Assert.That(outcomes, Is.EquivalentTo(new[] { 200, 409 }));
        Assert.That((await f.Service.GetChallengeAsync(f.LiveScope, reference, challenge.ChallengeId)).Version, Is.EqualTo(2));
        Assert.That((await f.Service.GetChallengeAsync(f.LiveScope, reference, challenge.ChallengeId, 1)).Sha256, Is.EqualTo(challenge.Sha256));
    }

    [Test]
    public async Task ComparisonFlagsDifferentScenarioScopesEvenWhenTheyHaveTheSameVisiblePackage()
    {
        using var f = new HypothesisFixture();
        HypothesisScope firstScope = await f.ScenarioScopeAsync();
        Scenario other = await f.Scenarios.CreateAsync(new(f.LiveScope.FieldId, "Target", HypothesisFixture.Instant,
            "other-scenario", new string('a', 64)));
        HypothesisScope secondScope = firstScope with { ScenarioId = other.ScenarioId };
        HypothesisRevision first = await f.Service.CreateAsync(await f.RequestAsync(scope: firstScope), "first");
        HypothesisRevision second = await f.Service.CreateAsync(await f.RequestAsync(scope: secondScope), "second");
        Assert.That(second.Package.Sha256, Is.EqualTo(first.Package.Sha256));
        HypothesisComparison comparison = await f.Service.CompareAsync(new(
            [new(firstScope, new(first.HypothesisId, 1)), new(secondScope, new(second.HypothesisId, 1))]));
        Assert.That(comparison.Entries.SelectMany(item => item.ScopeDifferences), Does.Contain("different-scenario"));
        Assert.That(comparison.Entries.Count(item => !item.LikeForLikeEvidence), Is.EqualTo(1));
    }

    [Test]
    public async Task CollectionAndSnapshotLimits_FailExplicitlyWithoutTruncation()
    {
        using var f = new HypothesisFixture();
        Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.ListAsync(f.LiveScope, null, limit: 51));
        Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.ListAsync(f.LiveScope, null, offset: -1));
        HypothesisCreateRequest request = await f.RequestAsync();
        Assert.ThrowsAsync<ScenarioApiException>(() => f.Service.CreateAsync(request with
        {
            Input = request.Input with
            {
                ControlNotes = Enumerable.Range(0, 33).Select(_ => new HypothesisControlNote($"field:{f.LiveScope.FieldId:D}", "Note")).ToArray()
            }
        }, "too-many-notes"));
        Assert.That(Assert.Throws<ScenarioApiException>(() =>
            HypothesisIntegrity.Serialize(new { value = new string('x', HypothesisValidation.MaximumSnapshotBytes) }))!.StatusCode, Is.EqualTo(413));
        Assert.That(await f.Service.ListAsync(f.LiveScope, null), Is.Empty);
    }
}

internal sealed class HypothesisFixture : IDisposable
{
    public static readonly DateTimeOffset Instant = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
    public string DatabasePath { get; } = Path.Combine(AppContext.BaseDirectory, $"hypotheses-{Guid.NewGuid():N}.db");
    public string ConnectionString => new SqliteConnectionStringBuilder { DataSource = DatabasePath }.ToString();
    public MutableHypothesisPackages Source { get; } = new();
    public ScenarioService Scenarios { get; }
    public HypothesisService Service { get; }
    public PetrophysicsAnalysisService Analysis { get; } = new(TimeProvider.System);
    public HypothesisScope LiveScope => new(Source.Package.FieldId, "Target", null, null);

    public HypothesisFixture()
    {
        Scenarios = new(new SqliteScenarioStore(ConnectionString), Source, new CanonicalJsonHasher(), TimeProvider.System);
        Service = Restart();
    }

    public HypothesisService Restart() => new(new SqliteHypothesisStore(ConnectionString), Scenarios, Analysis, TimeProvider.System);

    public async Task<HypothesisScope> ScenarioScopeAsync()
    {
        Scenario scenario = await Scenarios.CreateAsync(new(Source.Package.FieldId, "Target", Instant, "hypothesis-tests", new string('a', 64)));
        return new(Source.Package.FieldId, scenario.ReservoirName, scenario.ScenarioId, scenario.AsOfUtc);
    }

    public async Task<HypothesisCreateRequest> RequestAsync(string name = "Primary", HypothesisScope? scope = null)
    {
        scope ??= LiveScope;
        AnalysisPackage package = await Scenarios.GetPackageAsync(scope.FieldId, scope.ScenarioId, scope.AsOfUtc);
        AnalysisResult result = Analysis.Analyze(package, scope.ReservoirName);
        return new(name, scope, new(result.Configuration, package.Sha256, result.AnalysisSha256, result.Ranking[0].CandidateId, "Evidence-based interpretation."));
    }

    public void ChangeLivePackage()
    {
        Source.Package.Field["newEvidence"] = true;
        AnalysisPackage p = Source.Package;
        Source.Package = p with
        {
            Sha256 = new CanonicalJsonHasher().Compute(p.FieldId, p.Field, p.Clusters, p.Wells, p.WellBores,
                p.WellBoreArchitectures, p.Trajectories, p.GeologicalProperties, p.SourceCounts, p.DataGaps)
        };
    }

    public async Task ExecuteAsync(string sql)
    {
        await using var connection = new SqliteConnection(ConnectionString);
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(DatabasePath)) File.Delete(DatabasePath);
    }
}

internal sealed class MutableHypothesisPackages : IFieldPackageService
{
    public AnalysisPackage Package { get; set; } = AnalysisConfigurationTests.CreatePackage(withTrajectories: true);
    public Task<JsonNode> GetFieldsAsync(CancellationToken cancellationToken) => Task.FromResult<JsonNode>(new JsonArray());
    public Task<AnalysisPackage> BuildPackageAsync(Guid fieldId, CancellationToken cancellationToken) => Task.FromResult(Package);
}
