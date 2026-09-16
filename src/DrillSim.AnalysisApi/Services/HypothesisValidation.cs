using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Services;

internal static class HypothesisValidation
{
    public const int MaximumSnapshotBytes = 16 * 1024 * 1024;

    public static HypothesisScope Scope(HypothesisScope? scope)
    {
        if (scope is null || scope.FieldId == Guid.Empty || scope.ScenarioId == Guid.Empty)
            throw Invalid("An explicit field/reservoir/scenario/as-of scope is required.");
        string reservoir = Text(scope.ReservoirName, "reservoirName", 200).Trim();
        if (scope.ScenarioId.HasValue != scope.AsOfUtc.HasValue ||
            scope.AsOfUtc is { } instant && (instant == default || instant == DateTimeOffset.MaxValue))
            throw Invalid("Scenario scopes require asOfUtc; live scopes require scenarioId and asOfUtc both null.");
        return scope with { ReservoirName = reservoir, AsOfUtc = scope.AsOfUtc?.ToUniversalTime() };
    }

    public static string ScopeKey(HypothesisScope scope) =>
        PredictionJson.ComputeSha256(Scope(scope) with { ReservoirName = scope.ReservoirName.Trim().ToUpperInvariant() });

    public static void Input(HypothesisRevisionInput? input)
    {
        if (input is null) throw Invalid("input is required.");
        AnalysisConfiguration.Validate(input.Configuration);
        Hash(input.PackageSha256);
        Hash(input.AnalysisSha256);
        Text(input.SelectedCandidateId, "selectedCandidateId", 200);
        Text(input.Rationale, "rationale", 10_000);
        if (input.CorrelationNotes is not null) Text(input.CorrelationNotes, "correlationNotes", 4_000);
        if (input.ControlNotes is not null)
        {
            if (input.ControlNotes.Count > 32) throw Invalid("At most 32 control notes are supported.");
            foreach (HypothesisControlNote? note in input.ControlNotes)
            {
                if (note is null) throw Invalid("Control notes cannot contain null.");
                Text(note.EvidenceId, "evidenceId", 200);
                Text(note.Note, "control note", 2_000);
            }
        }
    }

    public static string Text(string? text, string field, int maximum)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > maximum || text.Any(c => char.IsControl(c) && c is not '\n' and not '\r' and not '\t'))
            throw Invalid($"{field} must contain between 1 and {maximum} characters without control characters.");
        return text;
    }

    public static void Hash(string? hash)
    {
        if (hash is null || hash.Length != 64 || hash.Any(c => c is not (>= '0' and <= '9') and not (>= 'a' and <= 'f')))
            throw Invalid("Fingerprints must be 64 lowercase hexadecimal characters.");
    }

    public static void Reference(HypothesisReference? reference)
    {
        if (reference is null || reference.HypothesisId == Guid.Empty || reference.Revision is < 1 or > 100_000)
            throw Invalid("An exact nonempty hypothesis ID and positive revision (at most 100000) are required.");
    }

    public static void Page(int limit, int offset)
    {
        if (limit is < 1 or > 50 || offset is < 0 or > 100_000)
            throw Invalid("limit must be 1..50 and offset 0..100000.");
    }

    public static void Idempotency(string? key)
    {
        if (key is null || key.Length is < 1 or > 128 || key.Any(c => c < 33 || c > 126))
            throw Invalid("A bounded stable Idempotency-Key is required.");
    }

    public static int Version(int? version)
    {
        if (version is null)
            throw new ScenarioApiException(428, "Hypothesis precondition required", "Supply the exact quoted revision/version in If-Match.");
        if (version is < 1 or >= 100_000)
            throw Invalid("The revision/version precondition must be 1..99999.");
        return version.Value;
    }

    public static HashSet<string> Evidence(AnalysisPackage package) =>
        EvidenceCatalog.Enumerate(package).Select(e => e.EvidenceId).ToHashSet(StringComparer.Ordinal);

    public static void Citations(IReadOnlyList<string>? citations, HashSet<string> evidence)
    {
        if (citations is null || citations.Count is < 1 or > 32 ||
            citations.Distinct(StringComparer.Ordinal).Count() != citations.Count ||
            citations.Any(id => id is null || !evidence.Contains(id)))
            throw Invalid("Each objection requires 1..32 unique evidence IDs from the exact saved visible package.");
    }

    public static ScenarioApiException Invalid(string detail) => new(400, "Invalid hypothesis request", detail);
    public static ScenarioApiException Conflict(string detail) => new(409, "Hypothesis conflict", detail);
    public static ScenarioApiException NotFound() => new(404, "Hypothesis artifact not found", "No artifact exists in the explicit requested scope.");
}
