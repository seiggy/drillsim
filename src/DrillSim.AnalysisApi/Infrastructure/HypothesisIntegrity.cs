using System.Text;
using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;

namespace DrillSim.AnalysisApi.Infrastructure;

internal static class HypothesisIntegrity
{
    public static HypothesisRevision Finalize(HypothesisRevision value) =>
        value with { SnapshotSha256 = PredictionJson.ComputeSha256(value with { SnapshotSha256 = string.Empty }) };

    public static HypothesisChallenge Finalize(HypothesisChallenge value) =>
        value with { Sha256 = PredictionJson.ComputeSha256(value with { Sha256 = string.Empty }) };

    public static string Serialize<T>(T value)
    {
        string json = PredictionJson.Canonicalize(value);
        if (Encoding.UTF8.GetByteCount(json) > HypothesisValidation.MaximumSnapshotBytes)
            throw new ScenarioApiException(413, "Hypothesis snapshot too large", "A saved snapshot is limited to 16 MiB; no evidence was truncated or saved.");
        return json;
    }

    public static HypothesisRevision ReadRevision(string json, string hash)
    {
        if (Encoding.UTF8.GetByteCount(json) > HypothesisValidation.MaximumSnapshotBytes)
            throw new InvalidDataException("Saved hypothesis exceeds the snapshot size limit.");
        HypothesisRevision value = PredictionJson.Deserialize<HypothesisRevision>(json, "hypothesis revision");
        if (value.SchemaVersion != "hypothesis-revision-v1" || PredictionJson.Canonicalize(value) != json ||
            value.SnapshotSha256 != hash || Finalize(value).SnapshotSha256 != hash ||
            value.Package is null || value.Analysis is null ||
            value.Package.Field is null || value.Package.Clusters is null || value.Package.Wells is null ||
            value.Package.WellBores is null || value.Package.WellBoreArchitectures is null ||
            value.Package.Trajectories is null || value.Package.GeologicalProperties is null ||
            value.Package.SourceCounts is null || value.Package.DataGaps is null ||
            value.Analysis.CandidateGrid is null)
            throw new InvalidDataException("Saved hypothesis snapshot integrity failed.");
        try
        {
            HypothesisValidation.Scope(value.Scope);
            HypothesisValidation.Input(value.Input);
            HypothesisValidation.Reference(new(value.HypothesisId, value.Revision));
            HypothesisValidation.Text(value.Name, "name", 120);
            string packageHash = new CanonicalJsonHasher().Compute(value.Package.FieldId, value.Package.Field,
                value.Package.Clusters, value.Package.Wells, value.Package.WellBores, value.Package.WellBoreArchitectures,
                value.Package.Trajectories, value.Package.GeologicalProperties, value.Package.SourceCounts, value.Package.DataGaps);
            JsonObject analysisIdentity = JsonNode.Parse(PredictionJson.Canonicalize(value.Analysis))!.AsObject();
            analysisIdentity.Remove("generatedAt");
            analysisIdentity.Remove("configuration");
            analysisIdentity.Remove("analysisSha256");
            if (value.Package.FieldId != value.Scope.FieldId || value.Package.Sha256 != packageHash ||
                value.Input.PackageSha256 != packageHash || value.Analysis.PackageSha256 != packageHash ||
                value.Analysis.FieldId != value.Scope.FieldId ||
                value.Analysis.ReservoirName != value.Scope.ReservoirName ||
                value.Analysis.Configuration != value.Input.Configuration ||
                value.Analysis.ConfigurationSha256 != value.Input.Configuration.ComputeSha256() ||
                value.Analysis.AnalysisSha256 != value.Input.AnalysisSha256 ||
                CanonicalJsonHasher.ComputeCanonicalSha256(analysisIdentity) != value.Input.AnalysisSha256 ||
                !value.Analysis.CandidateGrid.Any(p => p.Status == "eligible" && p.CandidateId == value.Input.SelectedCandidateId && p.Prediction is not null))
                throw new InvalidDataException("Saved hypothesis snapshot integrity failed.");
        }
        catch (ScenarioApiException exception)
        {
            throw new InvalidDataException("Saved hypothesis snapshot validation failed.", exception);
        }
        return value;
    }

    public static HypothesisChallenge ReadChallenge(string json, string hash)
    {
        if (Encoding.UTF8.GetByteCount(json) > HypothesisValidation.MaximumSnapshotBytes)
            throw new InvalidDataException("Saved challenge exceeds the snapshot size limit.");
        HypothesisChallenge value = PredictionJson.Deserialize<HypothesisChallenge>(json, "hypothesis challenge");
        if (value.SchemaVersion != "hypothesis-challenge-v1" || value.Version is < 1 or > 100_000 ||
            value.ChallengeId == Guid.Empty || PredictionJson.Canonicalize(value) != json ||
            value.Sha256 != hash || Finalize(value).Sha256 != hash ||
            value.Hypothesis is null || value.Objections is null || value.Objections.Count is < 1 or > 32 ||
            value.Objections.Any(item => item is null))
            throw new InvalidDataException("Saved hypothesis challenge integrity failed.");
        return value;
    }

    public static HypothesisSummary Summary(HypothesisRevision value) =>
        new(value.HypothesisId, value.Revision, value.Name, value.Scope, value.Package.Sha256,
            value.Analysis.ConfigurationSha256, value.Analysis.AnalysisSha256, value.Input.SelectedCandidateId, value.SnapshotSha256, value.CreatedUtc);
}
