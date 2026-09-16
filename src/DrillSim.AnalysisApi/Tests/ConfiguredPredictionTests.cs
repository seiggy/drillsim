using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;
using Microsoft.Data.Sqlite;
using StageB = DrillingOperations;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class ConfiguredPredictionTests
{
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(4)]
    [TestCase(5)]
    public async Task Seal_UsesConfiguredCutoffsGridExclusionsAndExactlyFourNeighborBaseline(int neighbors)
    {
        using var f = await ConfiguredPredictionFixture.CreateAsync(AnalysisConfiguration.Default with
        {
            PorosityCutoff = .23, PermeabilityCutoffM2 = 4e-15, GridPointsPerAxis = 9,
            WellExclusionRadiusM = 700, IdwNeighborCount = neighbors
        });
        PredictionRecord saved = await f.Ledger.PutDraftAsync(f.Scenario.ScenarioId, f.Body);
        PredictionRecord seal = await f.Ledger.SealAsync(f.Scenario.ScenarioId, expectedRevision: saved.Revision);
        AnalysisResult four = f.Data.Analysis.Analyze(f.Package, "Target", f.Result.Configuration with { IdwNeighborCount = 4 });
        RankedCandidate fourCandidate = four.CandidateGrid.Single(point =>
            point.EastingM == f.Candidate.EastingM && point.NorthingM == f.Candidate.NorthingM).Prediction!;
        BaselineSnapshot idw = seal.Baselines.Single(item => item.Kind == BaselineKind.FourNeighborIdw);
        BaselineSnapshot mean = seal.Baselines.Single(item => item.Kind == BaselineKind.FieldMean);
        BaselineSnapshot nearest = seal.Baselines.Single(item => item.Kind == BaselineKind.NearestWell);
        BaselineSnapshot rankOne = seal.Baselines.Single(item => item.Kind == BaselineKind.UncertaintyAwareRank1);
        WellPaySummary nearestWell = f.Result.WellSummaries.OrderBy(well =>
            Math.Pow(well.EastingM - f.Candidate.EastingM, 2) + Math.Pow(well.NorthingM - f.Candidate.NorthingM, 2))
            .ThenBy(well => well.WellEvidenceId, StringComparer.Ordinal).First();
        Assert.Multiple(() =>
        {
            Assert.That(f.Result.CandidateGrid, Has.Count.EqualTo(81));
            Assert.That(f.Candidate.NearestWellDistanceM, Is.GreaterThanOrEqualTo(700));
            Assert.That(f.Candidate.NeighborEvidenceIds, Has.Count.EqualTo(neighbors));
            Assert.That(idw.ContributingEvidenceIds, Has.Count.EqualTo(4));
            Assert.That(idw.ContributingEvidenceIds, Is.EqualTo(fourCandidate.NeighborEvidenceIds));
            Assert.That(idw.ExpectedPaydirtM, Is.EqualTo(new QuantileValues(fourCandidate.P90NetPayM, fourCandidate.P50NetPayM, fourCandidate.P10NetPayM)));
            Assert.That(idw.AnalysisBinding!.Configuration.IdwNeighborCount, Is.EqualTo(4));
            Assert.That(idw.AnalysisBinding.AnalysisSha256, Is.EqualTo(four.AnalysisSha256));
            Assert.That(idw.AnalysisBinding.PredictionAnalysisSha256, Is.EqualTo(f.Result.AnalysisSha256));
            Assert.That(mean.ExpectedPaydirtM!.P50, Is.EqualTo(f.Result.WellSummaries.Average(well => well.NetPayThicknessM)));
            Assert.That(mean.ContributingEvidenceIds, Has.Count.EqualTo(5));
            Assert.That(nearest.ExpectedPaydirtM!.P50, Is.EqualTo(nearestWell.NetPayThicknessM));
            Assert.That(nearest.ContributingEvidenceIds, Is.EqualTo(new[] { nearestWell.WellEvidenceId }));
            Assert.That(rankOne.CandidateId, Is.EqualTo(f.Result.Ranking[0].CandidateId));
            Assert.That(rankOne.ExpectedPaydirtM!.P50, Is.EqualTo(f.Result.Ranking[0].P50NetPayM));
            Assert.That(rankOne.ContributingEvidenceIds, Has.Count.EqualTo(neighbors));
            Assert.That(seal.Baselines.All(item => item.ModelVersion.EndsWith("-v3", StringComparison.Ordinal)), Is.True);
            Assert.That(seal.Baselines.All(item => item.FluidClasses is null && item.ProductionForecasts is null), Is.True);
        });
        AnalysisResult legacy = f.Data.Analysis.Analyze(f.Package, "Target");
        Assert.That(mean.ExpectedPaydirtM!.P50, Is.LessThan(legacy.WellSummaries.Average(well => well.NetPayThicknessM)));
        Assert.That(PredictionJson.ComputeSha256(seal.Body), Is.EqualTo(seal.Seal!.Sha256));
        StageB.AnalysisPredictionDto dto = ParseStageB(await f.Ledger.ApproveAsync(f.Scenario.ScenarioId, "Reviewer"));
        Assert.DoesNotThrow(() => StageB.ScoreArtifactIntegrity.ValidatePrediction(f.Scenario.ScenarioId, dto, seal.Seal.Sha256, f.Package.Sha256));
    }

    [Test]
    public async Task ExplicitDefaultBinding_IsValidAndLegacyCanonicalBaselinesRemainExactlyUnchanged()
    {
        using var f = await ConfiguredPredictionFixture.CreateAsync(AnalysisConfiguration.Default);
        AnalysisResult four = f.Data.Analysis.Analyze(f.Package, "Target", AnalysisConfiguration.Default);
        PredictionBody legacy = f.Body with { AnalysisBinding = null };
        IReadOnlyList<BaselineSnapshot> original = BaselineFactory.Create(f.Scenario.ScenarioId, legacy, four);
        foreach (BaselineSnapshot baseline in original)
        {
            Assert.That(baseline.ModelVersion, Does.EndWith("-v2"));
            string bytes = PredictionJson.Canonicalize(baseline);
            Assert.That(bytes, Does.Not.Contain("analysisBinding"));
            StageB.AnalysisBaselineDto dto = JsonSerializer.Deserialize<StageB.AnalysisBaselineDto>(bytes, StageB.CanonicalJson.SerializerOptions)!;
            Assert.That(StageB.CanonicalJson.Serialize(dto), Is.EqualTo(bytes));
            Assert.That(StageB.DeterministicIdentity.Sha256(StageB.ConfiguredPredictionIntegrity.CanonicalBaselineContent(dto)),
                Is.EqualTo(baseline.ContentSha256));
        }
        await f.Ledger.PutDraftAsync(f.Scenario.ScenarioId, f.Body);
        PredictionRecord configured = await f.Ledger.SealAsync(f.Scenario.ScenarioId);
        Assert.That(configured.Baselines.Select(item => item.ExpectedPaydirtM), Is.EqualTo(original.Select(item => item.ExpectedPaydirtM)));
        Assert.That(configured.Baselines.Select(item => item.ContentSha256), Is.Not.EqualTo(original.Select(item => item.ContentSha256)));
        Assert.That(configured.Body.AnalysisBinding!.Version, Is.EqualTo(ConfiguredPredictionBinding.Version));
    }

    [Test]
    public async Task Seal_ReloadReplayAndApprovalUseFrozenSourceEvenAfterLiveMutation()
    {
        using var f = await ConfiguredPredictionFixture.CreateAsync(AnalysisConfiguration.Default with { IdwNeighborCount = 2 });
        await f.Ledger.PutDraftAsync(f.Scenario.ScenarioId, f.Body);
        f.Data.ChangeLivePackage();
        PredictionRecord sealedRecord = await f.Ledger.SealAsync(f.Scenario.ScenarioId);
        PredictionRecord replay = await f.Ledger.SealAsync(f.Scenario.ScenarioId);
        Assert.That(PredictionJson.Canonicalize(replay), Is.EqualTo(PredictionJson.Canonicalize(sealedRecord)));
        PredictionRecord approved = await f.Ledger.ApproveAsync(f.Scenario.ScenarioId, "Reviewer");
        var restarted = new PredictionLedgerService(new SqliteScenarioStore(f.Data.ConnectionString), f.Data.Scenarios, f.Data.Analysis, TimeProvider.System);
        PredictionRecord loaded = await restarted.GetAsync(f.Scenario.ScenarioId);
        Assert.That(PredictionJson.Canonicalize(loaded), Is.EqualTo(PredictionJson.Canonicalize(approved)));
        Assert.ThrowsAsync<ScenarioApiException>(() => restarted.PutDraftAsync(f.Scenario.ScenarioId,
            f.Body with { AnalysisBinding = f.Body.AnalysisBinding! with { Configuration = AnalysisConfiguration.Default } }, 1));
    }

    [TestCase("version")]
    [TestCase("model")]
    [TestCase("scope")]
    [TestCase("field")]
    [TestCase("reservoir")]
    [TestCase("clock")]
    [TestCase("package")]
    [TestCase("configuration")]
    [TestCase("result")]
    [TestCase("candidate")]
    [TestCase("geometry")]
    [TestCase("quantiles")]
    [TestCase("citation")]
    public async Task SaveAndSeal_RejectStaleForeignFabricatedOrModifiedBinding(string error)
    {
        using var f = await ConfiguredPredictionFixture.CreateAsync(AnalysisConfiguration.Default with { GridPointsPerAxis = 9 });
        PredictionAnalysisBinding binding = f.Body.AnalysisBinding!;
        PredictionBody invalid = error switch
        {
            "version" => f.Body with { AnalysisBinding = binding with { Version = "unsupported" } },
            "model" => f.Body with { AnalysisBinding = binding with { ModelVersion = "unsupported" } },
            "scope" => f.Body with { AnalysisBinding = binding with { ScenarioId = Guid.NewGuid() } },
            "field" => f.Body with { AnalysisBinding = binding with { FieldId = Guid.NewGuid() } },
            "reservoir" => f.Body with { AnalysisBinding = binding with { ReservoirName = "Other" } },
            "clock" => f.Body with { AnalysisBinding = binding with { AsOfUtc = binding.AsOfUtc!.Value.AddDays(1) } },
            "package" => f.Body with { FieldPackageSha256 = new string('a', 64) },
            "configuration" => f.Body with { AnalysisBinding = binding with { Configuration = binding.Configuration with { PorosityCutoff = .8 } } },
            "result" => f.Body with { AnalysisBinding = binding with { AnalysisSha256 = new string('a', 64) } },
            "candidate" => f.Body with { CandidateId = "configured:not-real" },
            "geometry" => f.Body with { ProposedWellPath = [f.Body.ProposedWellPath[0], f.Body.ProposedWellPath[1] with { EastingM = f.Candidate.EastingM + 1 }] },
            "quantiles" => f.Body with { ExpectedPaydirtM = new(0, 0, 0) },
            _ => f.Body with { CitedEvidenceIds = [$"well:{Guid.NewGuid():D}"] }
        };
        Assert.ThrowsAsync<ScenarioApiException>(() => f.Ledger.PutDraftAsync(f.Scenario.ScenarioId, invalid));
        await f.Ledger.PutDraftAsync(f.Scenario.ScenarioId, f.Body);
        await f.SetDraftAsync(invalid);
        Assert.ThrowsAsync<ScenarioApiException>(() => f.Ledger.SealAsync(f.Scenario.ScenarioId));
    }

    [Test]
    public async Task ForeignScenarioWithSamePackage_AndForgedTargetPlusMatchingPath_AreRejected()
    {
        using var f = await ConfiguredPredictionFixture.CreateAsync(AnalysisConfiguration.Default);
        Scenario foreign = await f.Data.Scenarios.CreateAsync(new(f.Scenario.SourceFieldId, "Target", f.Scenario.InitialAsOfUtc,
            "different-scenario", new string('a', 64)));
        Assert.That((await f.Data.Scenarios.GetPackageAsync(foreign.SourceFieldId, foreign.ScenarioId, foreign.InitialAsOfUtc)).Sha256,
            Is.EqualTo(f.Package.Sha256));
        Assert.ThrowsAsync<ScenarioApiException>(() => f.Ledger.PutDraftAsync(foreign.ScenarioId, f.Body));
        PredictionBody forged = f.Body with
        {
            ProposedWellPath = f.Body.ProposedWellPath.Select(station => station with { EastingM = station.EastingM + 3 }).ToArray(),
            AnalysisBinding = f.Body.AnalysisBinding! with { Target = f.Body.AnalysisBinding!.Target! with { EastingM = f.Candidate.EastingM + 3 } }
        };
        Assert.ThrowsAsync<ScenarioApiException>(() => f.Ledger.PutDraftAsync(f.Scenario.ScenarioId, forged));
    }

    [Test]
    public async Task DirectStoreSeal_CannotSubstituteDefaultOrIncorrectFourNeighborBaselines()
    {
        using var f = await ConfiguredPredictionFixture.CreateAsync(AnalysisConfiguration.Default with { IdwNeighborCount = 2, PorosityCutoff = .24 });
        await f.Ledger.PutDraftAsync(f.Scenario.ScenarioId, f.Body);
        AnalysisResult four = f.Data.Analysis.Analyze(f.Package, "Target", f.Result.Configuration with { IdwNeighborCount = 4 });
        IReadOnlyList<BaselineSnapshot> valid = BaselineFactory.Create(f.Scenario.ScenarioId, f.Body, f.Result, four);
        BaselineSnapshot wrong = BaselineIntegrity.Finalize(valid[2] with { ExpectedPaydirtM = new(100, 200, 300) });
        var store = new SqliteScenarioStore(f.Data.ConnectionString);
        Assert.ThrowsAsync<ScenarioApiException>(() => store.SealPredictionAsync(f.Scenario.ScenarioId, PredictionJson.Canonicalize(f.Body),
            PredictionJson.ComputeSha256(f.Body), [valid[0], valid[1], wrong, valid[3]], DateTimeOffset.UtcNow));
        Assert.Throws<ScenarioApiException>(() => BaselineFactory.Create(f.Scenario.ScenarioId, f.Body, f.Result, f.Result));
        Assert.That((await f.Ledger.GetAsync(f.Scenario.ScenarioId)).Seal, Is.Null);
        Assert.That((await f.Ledger.SealAsync(f.Scenario.ScenarioId)).Seal, Is.Not.Null);
    }

    [Test]
    public async Task InsufficientFourNeighborControls_RejectsSealInsteadOfRenamingTwoNeighborBaseline()
    {
        using var f = await ConfiguredPredictionFixture.CreateAsync(AnalysisConfiguration.Default with { IdwNeighborCount = 1 }, controls: 3);
        await f.Ledger.PutDraftAsync(f.Scenario.ScenarioId, f.Body);
        var error = Assert.ThrowsAsync<ScenarioApiException>(() => f.Ledger.SealAsync(f.Scenario.ScenarioId))!;
        Assert.That(error.Title, Is.EqualTo("Prediction baseline unavailable"));
        Assert.That((await f.Ledger.GetAsync(f.Scenario.ScenarioId)).Seal, Is.Null);
    }

    [Test]
    public async Task FullGridEligibleCandidateOutsideShortlist_CanBeBoundWithoutRerankingItsIdentity()
    {
        using var f = await ConfiguredPredictionFixture.CreateAsync(AnalysisConfiguration.Default);
        RankedCandidate candidate = f.Result.CandidateGrid.First(point => point.Status == "eligible" && point.Prediction!.Rank == 0).Prediction!;
        PredictionBody body = ConfiguredPredictionFixture.BodyFor(f.Scenario, f.Package, f.Result, candidate);
        await f.Ledger.PutDraftAsync(f.Scenario.ScenarioId, body);
        Assert.That((await f.Ledger.SealAsync(f.Scenario.ScenarioId)).Body.CandidateId, Is.EqualTo(candidate.CandidateId));
    }

    [Test]
    public async Task ConfiguredSealedBodyAndBaselines_RoundTripCrossServiceAndReachStageBExecution()
    {
        using var f = await ConfiguredPredictionFixture.CreateAsync(AnalysisConfiguration.Default with { IdwNeighborCount = 2 });
        await f.Ledger.PutDraftAsync(f.Scenario.ScenarioId, f.Body);
        await f.Ledger.SealAsync(f.Scenario.ScenarioId);
        PredictionRecord approved = await f.Ledger.ApproveAsync(f.Scenario.ScenarioId, "Reviewer");
        StageB.AnalysisPredictionDto prediction = ParseStageB(approved);
        StageB.AnalysisScenarioDto scenario = f.StageBScenario();
        Assert.That(StageB.CanonicalJson.Serialize(prediction.Body), Is.EqualTo(PredictionJson.Canonicalize(approved.Body)));
        Assert.That(StageB.CanonicalJson.Serialize(prediction.Baselines), Is.EqualTo(PredictionJson.Canonicalize(approved.Baselines)));
        using var handler = new HandoffHandler(scenario, prediction);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1") };
        var client = new StageB.AnalysisVerificationClient(http);
        var verifier = new StageB.BindingVerificationService(client, new StageB.ReservoirVerificationClient(http));
        var binding = new StageB.BindWorldRequest(scenario.ScenarioId.ToString("D"), approved.Seal!.Sha256, f.Package.Sha256,
            "test-world", scenario.WorldModelVersion, "test-calibration", new string('c', 64));
        await verifier.VerifyAsync(binding, CancellationToken.None);
        string database = Path.Combine(AppContext.BaseDirectory, $"configured-stage-b-{Guid.NewGuid():N}.db");
        try
        {
            var store = new StageB.DrillingOperationsStore($"Data Source={database}", TimeProvider.System);
            await store.InitializeAsync();
            Assert.That((await store.BindWorldAsync("/bind", "binding", binding.ScenarioId, binding)).StatusCode, Is.EqualTo(201));
            var runRequest = new StageB.CreateRunRequest(binding.ScenarioId, binding.ApprovedSealedPredictionHash, "configured-plan", new string('d', 64));
            StageB.ApiOutcome created = await store.CreateRunAsync("/runs", "run", runRequest);
            StageB.RunResponse run = JsonSerializer.Deserialize<StageB.RunResponse>(created.Body, StageB.CanonicalJson.SerializerOptions)!;
            await store.CompleteS0Async(run.RunId);
            Assert.That(await store.MaterializePlanAsync(run.RunId, scenario, prediction), Is.True);
            Assert.That(await store.ExecuteDeterministicDrillingAsync(run.RunId, new()), Is.True);
            Assert.That((await store.GetMaterializedPlanAsync(run.RunId))!.CandidateId, Is.EqualTo(f.Candidate.CandidateId));
            Assert.That((await store.GetDrillingExecutionAsync(run.RunId))!.Stations, Is.Not.Empty);
            Assert.That(await store.MaterializePlanAsync(run.RunId, scenario, prediction), Is.False);
            StageB.AnalysisPredictionDto altered = prediction with
            {
                Body = prediction.Body! with { AnalysisBinding = prediction.Body!.AnalysisBinding! with { Version = "unsupported" } }
            };
            Assert.ThrowsAsync<StageB.RunStageFailureException>(() => store.MaterializePlanAsync(run.RunId, scenario, altered));
            Assert.DoesNotThrow(() => StageB.ScoreArtifactIntegrity.ValidatePrediction(scenario.ScenarioId, prediction,
                approved.Seal.Sha256, f.Package.Sha256));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(database)) File.Delete(database);
        }
    }

    [Test]
    public async Task CrossService_StrictDtosAndIntegrityRejectChangedConfigurationGeometryAndBaselineVersions()
    {
        using var f = await ConfiguredPredictionFixture.CreateAsync(AnalysisConfiguration.Default with { IdwNeighborCount = 2 });
        await f.Ledger.PutDraftAsync(f.Scenario.ScenarioId, f.Body);
        await f.Ledger.SealAsync(f.Scenario.ScenarioId);
        PredictionRecord approved = await f.Ledger.ApproveAsync(f.Scenario.ScenarioId, "Reviewer");
        StageB.AnalysisPredictionDto dto = ParseStageB(approved);
        StageB.AnalysisPredictionBodyDto body = dto.Body!;
        StageB.AnalysisPredictionBindingDto binding = body.AnalysisBinding!;
        foreach (StageB.AnalysisPredictionDto wrong in new[]
        {
            dto with { Body = body with { AnalysisBinding = binding with { Configuration = binding.Configuration with { IdwNeighborCount = 3 } } } },
            dto with { Body = body with { AnalysisBinding = binding with { Version = "unsupported" } } },
            dto with { Body = body with { AnalysisBinding = null } },
            dto with { Body = body with { ProposedWellPath = [body.ProposedWellPath![0], body.ProposedWellPath[1] with { EastingM = binding.Target.EastingM + 1 }] } },
            dto with { Baselines = dto.Baselines!.Select(item => item with { ModelVersion = "baseline-four-neighbor-idw-v2" }).ToArray() },
            dto with { Baselines = dto.Baselines!.Select(item => item with { AnalysisBinding = null }).ToArray() },
            dto with { Body = body with { Rationale = "Unsealed change" } }
        })
            Assert.Throws<StageB.ScoringException>(() => StageB.ConfiguredPredictionIntegrity.ValidateApproved(wrong, f.StageBScenario()));
        Assert.Throws<StageB.ScoringException>(() => StageB.ConfiguredPredictionIntegrity.ValidateApproved(dto,
            f.StageBScenario() with { SourceFieldId = Guid.NewGuid() }));
        JsonNode json = JsonNode.Parse(PredictionJson.Canonicalize(approved))!;
        json["body"]!["analysisBinding"]!["configuration"]!["unwiredSetting"] = 1;
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StageB.AnalysisPredictionDto>(json.ToJsonString(), StageB.CanonicalJson.SerializerOptions));
        json["body"]!["analysisBinding"]!["configuration"]!.AsObject().Remove("unwiredSetting");
        json["body"]!["analysisBinding"]!.AsObject().Remove("version");
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StageB.AnalysisPredictionDto>(json.ToJsonString(), StageB.CanonicalJson.SerializerOptions));
    }

    private static StageB.AnalysisPredictionDto ParseStageB(PredictionRecord record) =>
        JsonSerializer.Deserialize<StageB.AnalysisPredictionDto>(PredictionJson.Canonicalize(record), StageB.CanonicalJson.SerializerOptions)!;

    [Test]
    public async Task CorruptedConfiguredSeal_CannotBeApprovedBeforeVerification()
    {
        using var f = await ConfiguredPredictionFixture.CreateAsync(AnalysisConfiguration.Default with { IdwNeighborCount = 2 });
        await f.Ledger.PutDraftAsync(f.Scenario.ScenarioId, f.Body);
        await f.Ledger.SealAsync(f.Scenario.ScenarioId);
        await f.SetDraftAsync(f.Body with { AnalysisBinding = f.Body.AnalysisBinding! with { Version = "changed" } });
        Assert.ThrowsAsync<InvalidDataException>(() => f.Ledger.ApproveAsync(f.Scenario.ScenarioId, "Reviewer"));
        await using var connection = new SqliteConnection(f.Data.ConnectionString);
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM prediction_approvals;";
        Assert.That(Convert.ToInt32(await command.ExecuteScalarAsync()), Is.Zero);
    }

    [Test]
    public void PublicCapabilities_DescribeSupportedVersionAndActualPointScreeningLimits()
    {
        JsonNode capabilities = JsonNode.Parse(PredictionJson.Canonicalize(ConfiguredPredictionBinding.Capabilities()))!;
        Assert.Multiple(() =>
        {
            Assert.That(capabilities["configuredSaveSupported"]!.GetValue<bool>(), Is.True);
            Assert.That(capabilities["configuredSealSupported"]!.GetValue<bool>(), Is.True);
            Assert.That(capabilities["bindingVersion"]!.GetValue<string>(), Is.EqualTo("prediction-analysis-binding-v1"));
            Assert.That(capabilities["minimumLocatedControls"]!.GetValue<int>(), Is.EqualTo(4));
            Assert.That(capabilities["baselineModelVersions"]!.AsArray(), Has.Count.EqualTo(4));
            Assert.That(capabilities["targetRule"]!.GetValue<string>(), Does.Contain("Every planned station"));
        });
    }

    private sealed class HandoffHandler(StageB.AnalysisScenarioDto scenario, StageB.AnalysisPredictionDto prediction) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            object result = request.RequestUri!.AbsolutePath.EndsWith("/prediction", StringComparison.Ordinal) ? prediction :
                request.RequestUri.AbsolutePath.StartsWith("/api/scenarios", StringComparison.Ordinal) ? scenario :
                new StageB.ReservoirWorldDto("test-world", scenario.SourceFieldId, scenario.ReservoirName, scenario.WorldModelVersion,
                    new("test-calibration", new string('c', 64)));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(StageB.CanonicalJson.Serialize(result), Encoding.UTF8, "application/json")
            });
        }
    }
}

internal sealed class ConfiguredPredictionFixture : IDisposable
{
    public HypothesisFixture Data { get; } = new();
    public Scenario Scenario { get; private set; } = null!;
    public AnalysisPackage Package { get; private set; } = null!;
    public AnalysisResult Result { get; private set; } = null!;
    public RankedCandidate Candidate => Result.Ranking[0];
    public PredictionBody Body { get; private set; } = null!;
    public PredictionLedgerService Ledger { get; private set; } = null!;

    public static async Task<ConfiguredPredictionFixture> CreateAsync(AnalysisConfiguration configuration, int controls = 5)
    {
        var f = new ConfiguredPredictionFixture();
        if (controls < 5)
        {
            AnalysisPackage source = f.Data.Source.Package;
            var counts = source.SourceCounts with { GeologicalProperties = controls };
            JsonNode[] geology = source.GeologicalProperties.Take(controls).ToArray();
            f.Data.Source.Package = source with
            {
                GeologicalProperties = geology, SourceCounts = counts,
                Sha256 = new CanonicalJsonHasher().Compute(source.FieldId, source.Field, source.Clusters, source.Wells,
                    source.WellBores, source.WellBoreArchitectures, source.Trajectories, geology, counts, source.DataGaps)
            };
        }
        HypothesisScope scope = await f.Data.ScenarioScopeAsync();
        f.Scenario = await f.Data.Scenarios.GetAsync(scope.ScenarioId!.Value);
        f.Package = await f.Data.Scenarios.GetPackageAsync(scope.FieldId, scope.ScenarioId, scope.AsOfUtc);
        f.Result = f.Data.Analysis.Analyze(f.Package, "Target", configuration);
        f.Body = BodyFor(f.Scenario, f.Package, f.Result, f.Candidate);
        f.Ledger = new(new SqliteScenarioStore(f.Data.ConnectionString), f.Data.Scenarios, f.Data.Analysis, TimeProvider.System);
        return f;
    }

    public static PredictionBody BodyFor(Scenario scenario, AnalysisPackage package, AnalysisResult result, RankedCandidate candidate)
    {
        var quantiles = new QuantileValues(candidate.P90NetPayM, candidate.P50NetPayM, candidate.P10NetPayM);
        return new(candidate.CandidateId, [new(0, 0, candidate.EastingM, candidate.NorthingM), new(1500, 1500, candidate.EastingM, candidate.NorthingM)],
            [new("Target", new(100, 150, 200), new(300, 350, 400))], quantiles, [PredictedFluidClass.Oil], [],
            [new(1, 100, 0, 10), new(3, 250, 0, 30), new(5, 350, 0, 50)],
            ["Point-screening proxy; human formation and fluid/production predictions are separate assumptions."],
            candidate.NeighborEvidenceIds, package.Sha256, "Explicit human prediction from visible evidence.",
            new(result.Configuration, result.ConfigurationSha256, result.AnalysisSha256)
            {
                Version = ConfiguredPredictionBinding.Version, ModelVersion = result.ModelVersion,
                ScenarioId = scenario.ScenarioId, FieldId = scenario.SourceFieldId, ReservoirName = scenario.ReservoirName,
                AsOfUtc = scenario.InitialAsOfUtc,
                Target = new(candidate.CandidateId, candidate.EastingM, candidate.NorthingM, quantiles)
            });
    }

    public async Task SetDraftAsync(PredictionBody body)
    {
        await using var connection = new SqliteConnection(Data.ConnectionString);
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "UPDATE prediction_drafts SET body_json=$body WHERE scenario_id=$id;";
        command.Parameters.AddWithValue("$body", PredictionJson.Canonicalize(body));
        command.Parameters.AddWithValue("$id", Scenario.ScenarioId.ToString("D"));
        await command.ExecuteNonQueryAsync();
    }

    public StageB.AnalysisScenarioDto StageBScenario() => new(Scenario.ScenarioId, Scenario.SourceFieldId, Scenario.ReservoirName,
        Scenario.WorldModelVersion, "HumanApproved", Scenario.InitialAsOfUtc, Scenario.ObservationModelVersion);
    public void Dispose() => Data.Dispose();
}
