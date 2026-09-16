using System.Text.Json;
using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Services;

public sealed class HypothesisService(
    SqliteHypothesisStore store,
    ScenarioService scenarios,
    IPetrophysicsAnalysisService analysis,
    TimeProvider time)
{
    public async Task<HypothesisRevision> CreateAsync(HypothesisCreateRequest request, string key, CancellationToken ct = default)
    {
        HypothesisScope scope = HypothesisValidation.Scope(request.Scope);
        string name = HypothesisValidation.Text(request.Name, "name", 120).Trim();
        HypothesisValidation.Input(request.Input);
        string operation = $"create:{HypothesisValidation.ScopeKey(scope)}";
        string hash = PredictionJson.ComputeSha256(request with { Name = name, Scope = scope });
        HypothesisRevision? replay = await store.FindRevisionMutationAsync(scope, operation, key, hash, ct);
        if (replay is not null) return replay;
        scope = await ResolveScopeAsync(scope, ct);
        AnalysisPackage package = await LoadPackageAsync(scope, ct);
        HypothesisRevision value = Prepare(Guid.NewGuid(), 1, name, scope, request.Input, null, package);
        return await store.SaveRevisionAsync(value, null, operation, key, hash, ct);
    }

    public async Task<HypothesisRevision> ReviseAsync(
        Guid id, HypothesisReviseRequest request, int? expectedVersion, string key, CancellationToken ct = default)
    {
        int expected = HypothesisValidation.Version(expectedVersion);
        HypothesisScope scope = HypothesisValidation.Scope(request.Scope);
        HypothesisValidation.Input(request.Input);
        string operation = $"revise:{id:D}:{HypothesisValidation.ScopeKey(scope)}";
        string hash = PredictionJson.ComputeSha256(new { Request = request with { Scope = scope }, Expected = expected });
        HypothesisRevision? replay = await store.FindRevisionMutationAsync(scope, operation, key, hash, ct);
        if (replay is not null) return replay;
        HypothesisRevision previous = await GetAsync(scope, new(id, expected), ct);
        _ = await ResolveScopeAsync(previous.Scope, ct);
        AnalysisPackage package = await LoadPackageAsync(previous.Scope, ct);
        HypothesisRevision value = Prepare(id, expected + 1, previous.Name, previous.Scope, request.Input, previous.BranchedFrom, package);
        return await store.SaveRevisionAsync(value, expected, operation, key, hash, ct);
    }

    public async Task<HypothesisRevision> BranchAsync(HypothesisBranchRequest request, string key, CancellationToken ct = default)
    {
        HypothesisScope scope = HypothesisValidation.Scope(request.Scope);
        HypothesisValidation.Reference(request.Source);
        HypothesisValidation.Input(request.Input);
        string name = HypothesisValidation.Text(request.Name, "name", 120).Trim();
        string operation = $"branch:{HypothesisValidation.ScopeKey(scope)}";
        string hash = PredictionJson.ComputeSha256(request with { Scope = scope, Name = name });
        HypothesisRevision? replay = await store.FindRevisionMutationAsync(scope, operation, key, hash, ct);
        if (replay is not null) return replay;
        HypothesisRevision source = await GetAsync(scope, request.Source, ct);
        HypothesisRevision value = Prepare(Guid.NewGuid(), 1, name, source.Scope, request.Input, request.Source, source.Package);
        return await store.SaveRevisionAsync(value, null, operation, key, hash, ct);
    }

    public async Task<AnalysisResult> AnalyzeAsync(
        HypothesisReference reference, HypothesisAnalysisRequest request, CancellationToken ct = default)
    {
        AnalysisConfiguration.Validate(request.Configuration);
        HypothesisRevision source = await GetAsync(request.Scope, reference, ct);
        return analysis.Analyze(source.Package, source.Scope.ReservoirName, request.Configuration);
    }

    public async Task<HypothesisRevision> GetAsync(HypothesisScope scope, HypothesisReference reference, CancellationToken ct = default)
    {
        HypothesisValidation.Reference(reference);
        return await store.FindRevisionAsync(HypothesisValidation.Scope(scope), reference.HypothesisId, reference.Revision, ct)
            ?? throw HypothesisValidation.NotFound();
    }

    public async Task<HypothesisRevision> GetLatestAsync(HypothesisScope scope, Guid id, CancellationToken ct = default)
    {
        HypothesisValidation.Reference(new(id, 1));
        return await store.FindRevisionAsync(HypothesisValidation.Scope(scope), id, null, ct)
            ?? throw HypothesisValidation.NotFound();
    }

    public async Task<IReadOnlyList<HypothesisSummary>> ListAsync(
        HypothesisScope scope, Guid? id, int limit = 20, int offset = 0, CancellationToken ct = default)
    {
        scope = HypothesisValidation.Scope(scope);
        if (id.HasValue) _ = await GetLatestAsync(scope, id.Value, ct);
        return await store.ListAsync(scope, id, limit, offset, ct);
    }

    public async Task<HypothesisChallenge> CreateChallengeAsync(
        HypothesisReference reference, HypothesisChallengeRequest request, string key, CancellationToken ct = default)
    {
        HypothesisValidation.Idempotency(key);
        HypothesisValidation.Hash(request.AnalysisSha256);
        HypothesisValidation.Text(request.Summary, "summary", 4_000);
        string actor = HypothesisValidation.Text(request.Actor, "actor", 200).Trim();
        if (request.Objections is null || request.Objections.Count is < 1 or > 32)
            throw HypothesisValidation.Invalid("A challenge requires 1..32 objections.");
        HypothesisRevision source = await GetAsync(request.Scope, reference, ct);
        if (request.AnalysisSha256 != source.Analysis.AnalysisSha256)
            throw HypothesisValidation.Conflict("The challenge analysis fingerprint does not match the saved revision.");
        HashSet<string> evidence = HypothesisValidation.Evidence(source.Package);
        var objections = new List<HypothesisObjection>();
        foreach (HypothesisObjectionInput? objection in request.Objections)
        {
            if (objection is null) throw HypothesisValidation.Invalid("Objections cannot contain null.");
            HypothesisValidation.Text(objection.Text, "objection text", 4_000);
            HypothesisValidation.Citations(objection.CitedEvidenceIds, evidence);
            objections.Add(new(Guid.NewGuid(), objection.Text, objection.CitedEvidenceIds.ToArray(), "open", null, null));
        }
        DateTimeOffset now = time.GetUtcNow().ToUniversalTime();
        HypothesisChallenge challenge = HypothesisIntegrity.Finalize(new HypothesisChallenge(
            "hypothesis-challenge-v1", Guid.NewGuid(), 1, reference, source.SnapshotSha256, source.Analysis.AnalysisSha256,
            request.Summary, actor, actor, now, now, objections, string.Empty));
        return await store.SaveChallengeAsync(challenge, null, $"challenge:{reference.HypothesisId:D}:{reference.Revision}",
            key, PredictionJson.ComputeSha256(request with { Scope = source.Scope, Actor = actor }), ct);
    }

    public async Task<HypothesisChallenge> DispositionAsync(
        HypothesisReference reference, Guid challengeId, HypothesisDispositionRequest request,
        int? expectedVersion, string key, CancellationToken ct = default)
    {
        int expected = HypothesisValidation.Version(expectedVersion);
        HypothesisValidation.Idempotency(key);
        string actor = HypothesisValidation.Text(request.Actor, "actor", 200).Trim();
        if (request.Dispositions is null || request.Dispositions.Count is < 1 or > 32 ||
            request.Dispositions.Any(item => item is null) ||
            request.Dispositions.Select(item => item.ObjectionId).Distinct().Count() != request.Dispositions.Count)
            throw HypothesisValidation.Invalid("Supply 1..32 unique objection dispositions.");
        HypothesisChallenge previous = await GetChallengeAsync(request.Scope, reference, challengeId, expected, ct);
        var edits = new Dictionary<Guid, HypothesisDisposition>();
        foreach (HypothesisDisposition edit in request.Dispositions)
        {
            if (edit.Disposition is not ("open" or "accepted" or "rejected" or "deferred") ||
                !previous.Objections.Any(item => item.ObjectionId == edit.ObjectionId))
                throw HypothesisValidation.Invalid("Each disposition must identify an existing objection and a supported status.");
            HypothesisValidation.Text(edit.Reason, "disposition reason", 2_000);
            edits.Add(edit.ObjectionId, edit);
        }
        HypothesisChallenge updated = HypothesisIntegrity.Finalize(previous with
        {
            Version = expected + 1,
            ModifiedUtc = time.GetUtcNow().ToUniversalTime(),
            LastModifiedBy = actor,
            Objections = previous.Objections.Select(item => edits.TryGetValue(item.ObjectionId, out HypothesisDisposition? edit)
                ? item with { Disposition = edit.Disposition, DispositionReason = edit.Reason, DispositionActor = actor }
                : item).ToArray()
        });
        return await store.SaveChallengeAsync(updated, expected, $"disposition:{challengeId:D}", key,
            PredictionJson.ComputeSha256(new { Request = request with { Scope = HypothesisValidation.Scope(request.Scope), Actor = actor }, Expected = expected }), ct);
    }

    public async Task<HypothesisChallenge> GetChallengeAsync(
        HypothesisScope scope, HypothesisReference reference, Guid id, int? version = null, CancellationToken ct = default)
    {
        if (id == Guid.Empty || version is < 1 or > 100_000) throw HypothesisValidation.Invalid("Invalid challenge ID/version.");
        HypothesisRevision source = await GetAsync(scope, reference, ct);
        HypothesisChallenge value = await store.FindChallengeAsync(reference, id, version, ct) ?? throw HypothesisValidation.NotFound();
        ValidateChallengeSource(value, source);
        return value;
    }

    public async Task<IReadOnlyList<HypothesisChallenge>> ListChallengesAsync(
        HypothesisScope scope, HypothesisReference reference, int limit = 20, int offset = 0, CancellationToken ct = default)
    {
        HypothesisRevision source = await GetAsync(scope, reference, ct);
        IReadOnlyList<HypothesisChallenge> values = await store.ListChallengesAsync(reference, limit, offset, ct);
        foreach (HypothesisChallenge value in values) ValidateChallengeSource(value, source);
        return values;
    }

    public async Task<HypothesisComparison> CompareAsync(HypothesisCompareRequest request, CancellationToken ct = default)
    {
        if (request.Revisions is null || request.Revisions.Count is < 2 or > 8 ||
            request.Revisions.Any(item => item is null) ||
            request.Revisions.Select(item => item.Reference).Distinct().Count() != request.Revisions.Count)
            throw HypothesisValidation.Invalid("Compare 2..8 unique explicit saved revisions.");
        foreach (HypothesisComparisonSelection selection in request.Revisions)
            HypothesisValidation.Reference(selection.Reference);
        HypothesisComparisonSelection[] selections = request.Revisions
            .OrderBy(item => item.Reference.HypothesisId).ThenBy(item => item.Reference.Revision).ToArray();
        HypothesisRevision baseline = await GetAsync(selections[0].Scope, selections[0].Reference, ct);
        JsonObject baselineFields = ComparisonFields(baseline);
        var entries = new List<HypothesisComparisonEntry>();
        foreach (HypothesisComparisonSelection selection in selections)
        {
            HypothesisRevision value = selection.Reference == selections[0].Reference ? baseline :
                await GetAsync(selection.Scope, selection.Reference, ct);
            string[] scopeDifferences = ScopeDifferences(baseline, value);
            JsonObject fields = ComparisonFields(value);
            HypothesisDifference[] differences = baselineFields
                .Where(field => !JsonNode.DeepEquals(field.Value, fields[field.Key]))
                .Select(field => new HypothesisDifference(field.Key, field.Value?.DeepClone(), fields[field.Key]?.DeepClone()))
                .ToArray();
            entries.Add(new HypothesisComparisonEntry(HypothesisIntegrity.Summary(value), value.Input.Configuration, Selected(value), value.Input.Rationale,
                value.Input.CorrelationNotes, value.Input.ControlNotes, scopeDifferences.Length == 0, scopeDifferences, differences));
        }
        var result = new HypothesisComparison("hypothesis-comparison-v1", new(baseline.HypothesisId, baseline.Revision), entries, string.Empty);
        return result with { ComparisonSha256 = PredictionJson.ComputeSha256(result) };
    }

    private HypothesisRevision Prepare(Guid id, int revision, string name, HypothesisScope scope,
        HypothesisRevisionInput input, HypothesisReference? branchedFrom, AnalysisPackage source)
    {
        AnalysisPackage package = PredictionJson.Deserialize<AnalysisPackage>(HypothesisIntegrity.Serialize(source), "visible hypothesis package");
        if (package.Sha256 != input.PackageSha256)
            throw HypothesisValidation.Conflict("The submitted package hash differs from the package read for this save. Refresh/review the evidence.");
        AnalysisResult result = analysis.Analyze(package, scope.ReservoirName, input.Configuration);
        if (result.AnalysisSha256 != input.AnalysisSha256 ||
            !result.CandidateGrid.Any(point => point.CandidateId == input.SelectedCandidateId && point.Status == "eligible" && point.Prediction is not null))
            throw HypothesisValidation.Conflict("The submitted analysis fingerprint or selected eligible candidate does not match this package/configuration.");
        HashSet<string> evidence = HypothesisValidation.Evidence(package);
        if (input.ControlNotes?.Any(note => !evidence.Contains(note.EvidenceId)) == true)
            throw HypothesisValidation.Invalid("Control notes must cite evidence from the saved visible package.");
        return HypothesisIntegrity.Finalize(new HypothesisRevision("hypothesis-revision-v1", id, revision, name, scope, input,
            branchedFrom, time.GetUtcNow().ToUniversalTime(), package, result, string.Empty));
    }

    private async Task<HypothesisScope> ResolveScopeAsync(HypothesisScope scope, CancellationToken ct)
    {
        if (scope.ScenarioId is not Guid id) return scope;
        Scenario scenario = await scenarios.GetAsync(id, ct);
        if (!string.Equals(scope.ReservoirName, scenario.ReservoirName, StringComparison.OrdinalIgnoreCase))
            throw HypothesisValidation.Conflict("The reservoir does not match the selected scenario.");
        return scope with { ReservoirName = scenario.ReservoirName };
    }

    private async Task<AnalysisPackage> LoadPackageAsync(HypothesisScope scope, CancellationToken ct)
    {
        AnalysisPackage package = await scenarios.GetPackageAsync(scope.FieldId, scope.ScenarioId, scope.AsOfUtc, ct);
        if (package.FieldId != scope.FieldId) throw HypothesisValidation.Conflict("The visible package belongs to a different field.");
        return package;
    }

    private static void ValidateChallengeSource(HypothesisChallenge value, HypothesisRevision source)
    {
        if (value.HypothesisSnapshotSha256 != source.SnapshotSha256 || value.AnalysisSha256 != source.Analysis.AnalysisSha256 ||
            value.Hypothesis.HypothesisId != source.HypothesisId || value.Hypothesis.Revision != source.Revision)
            throw new InvalidDataException("The challenge is not bound to the exact saved hypothesis.");
        try
        {
            HashSet<string> evidence = HypothesisValidation.Evidence(source.Package);
            if (value.Objections.Count is < 1 or > 32 || value.Objections.Select(item => item.ObjectionId).Distinct().Count() != value.Objections.Count)
                throw new InvalidDataException("Saved challenge objections are invalid.");
            foreach (HypothesisObjection item in value.Objections)
            {
                HypothesisValidation.Citations(item.CitedEvidenceIds, evidence);
                if (item.Disposition is not ("open" or "accepted" or "rejected" or "deferred"))
                    throw new InvalidDataException("Saved challenge disposition is invalid.");
            }
        }
        catch (ScenarioApiException exception) { throw new InvalidDataException("Saved challenge citations are invalid.", exception); }
    }

    private static RankedCandidate Selected(HypothesisRevision value) =>
        value.Analysis.CandidateGrid.Single(point => point.CandidateId == value.Input.SelectedCandidateId).Prediction!;

    private static JsonObject ComparisonFields(HypothesisRevision value)
    {
        var fields = new JsonObject();
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        foreach (var property in JsonSerializer.SerializeToNode(value.Input.Configuration, options)!.AsObject())
            fields.Add($"configuration.{property.Key}", property.Value?.DeepClone());
        foreach (var property in JsonSerializer.SerializeToNode(Selected(value), options)!.AsObject())
            fields.Add($"selectedCandidate.{property.Key}", property.Value?.DeepClone());
        fields.Add("rationale", value.Input.Rationale);
        fields.Add("correlationNotes", value.Input.CorrelationNotes);
        fields.Add("controlNotes", JsonSerializer.SerializeToNode(value.Input.ControlNotes, options));
        return fields;
    }

    private static string[] ScopeDifferences(HypothesisRevision baseline, HypothesisRevision value)
    {
        var reasons = new List<string>();
        if (baseline.Scope.FieldId != value.Scope.FieldId) reasons.Add("different-field");
        if (!string.Equals(baseline.Scope.ReservoirName, value.Scope.ReservoirName, StringComparison.OrdinalIgnoreCase)) reasons.Add("different-reservoir");
        if (baseline.Scope.ScenarioId != value.Scope.ScenarioId) reasons.Add("different-scenario");
        if (baseline.Scope.AsOfUtc != value.Scope.AsOfUtc) reasons.Add("different-as-of");
        if (baseline.Package.Sha256 != value.Package.Sha256) reasons.Add("different-visible-package");
        return reasons.ToArray();
    }
}
