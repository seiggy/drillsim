using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Primitives;

namespace DrillingOperations;

public enum RunStageKind { S0BindWorld, S1MaterializePlan, S2ExecuteDrilling, S3GenerateSurvey, S4SampleGeology, S5GenerateLogs, S6DesignCompletion, S7RunProduction, S8PublishReveal, S9Score }
public enum RunStatus { Queued, Running, Blocked, AwaitingDependency, AwaitingApproval, ReadyToReveal, PublishFailed, Revealed, Scored, Cancelled, Failed }
public enum StageStatus { Pending, Running, Completed, Blocked, AwaitingDependency, AwaitingApproval, Failed, Cancelled }
public static class RunStateMachine
{
    public static bool IsTerminal(RunStatus status) => status is RunStatus.Cancelled or RunStatus.Failed or RunStatus.Scored;
    public static bool CanExecute(RunStageKind stage, IReadOnlyDictionary<RunStageKind, StageStatus> stages)
    {
        if (stage is RunStageKind.S8PublishReveal) return PriorCompleted(RunStageKind.S7RunProduction, stages) && stages[stage] is not StageStatus.Completed;
        if (stage is RunStageKind.S9Score) return stages[RunStageKind.S8PublishReveal] is StageStatus.Completed && stages[stage] is not StageStatus.Completed;
        if (stages[stage] is StageStatus.Completed) return false;
        return stage == RunStageKind.S0BindWorld || PriorCompleted((RunStageKind)((int)stage - 1), stages);
    }
    private static bool PriorCompleted(RunStageKind prior, IReadOnlyDictionary<RunStageKind, StageStatus> stages) => stages.TryGetValue(prior, out StageStatus status) && status is StageStatus.Completed;
}

public sealed record BindWorldRequest(string ScenarioId, string ApprovedSealedPredictionHash, string SourcePackageSha256, string WorldId, string WorldModelVersion, string CalibrationArtifactId, string CalibrationArtifactSha256);
public sealed record TruthBindingResponse(string BindingId, string ScenarioId, string ApprovedSealedPredictionHash, string SourcePackageSha256, string WorldId, string WorldModelVersion, string CalibrationArtifactId, string CalibrationArtifactSha256, DateTimeOffset CreatedUtc);
public sealed record CreateRunRequest(string ScenarioId, string ApprovedSealedPredictionHash, string PlanArtifactId, string PlanArtifactSha256);
public sealed record RunResponse(string RunId, string ScenarioId, RunStatus Status, RunStageKind? CurrentStage, DateTimeOffset CreatedUtc, DateTimeOffset UpdatedUtc, DateTimeOffset? EndedUtc,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? DiagnosticCode = null);
public sealed record StageResponse(RunStageKind Stage, string Name, StageStatus Status, int AttemptCount, DateTimeOffset? StartedUtc, DateTimeOffset? EndedUtc, string? DiagnosticCode);
public sealed record AuditResponse(string AuditId, string ScenarioId, long Sequence, string Action, string SubjectId, string DataHash, string PreviousHash, string EntryHash, DateTimeOffset CreatedUtc);
public sealed record ServiceStatus(string Service, string Status, string Scope);
public sealed record ApiOutcome(int StatusCode, string Body, string? Location = null, bool Replayed = false);
public sealed class PersistenceIntegrityException(string message) : Exception(message);

public interface IDependencyCapabilityProbe { bool IsAvailable(RunStageKind stage); }
public sealed class DeferredDependencyCapabilityProbe : IDependencyCapabilityProbe { public bool IsAvailable(RunStageKind stage) => false; }

public sealed class InternalKeyValidator
{
    public const string HeaderName = "X-DrillSim-Internal-Key";
    private readonly byte[] _expectedDigest;
    public InternalKeyValidator(string key)
    {
        if (string.IsNullOrEmpty(key) || key.Length > 1024) throw new ArgumentException("Internal key must contain between 1 and 1024 characters.", nameof(key));
        _expectedDigest = SHA256.HashData(Encoding.UTF8.GetBytes(key));
    }
    public bool IsAuthorized(IHeaderDictionary headers)
    {
        if (!headers.TryGetValue(HeaderName, out StringValues values) || values.Count != 1 || values[0] is null) return false;
        byte[] presented = SHA256.HashData(Encoding.UTF8.GetBytes(values[0]!));
        return CryptographicOperations.FixedTimeEquals(_expectedDigest, presented);
    }
}

public static class DeterministicIdentity
{
    public static string Create(params string[] parts)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n", parts)));
        Span<byte> id = stackalloc byte[16]; hash.AsSpan(0, 16).CopyTo(id);
        id[6] = (byte)((id[6] & 0x0f) | 0x50); id[8] = (byte)((id[8] & 0x3f) | 0x80);
        return new Guid(id, bigEndian: true).ToString("D", CultureInfo.InvariantCulture);
    }
    public static string Sha256(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}

public static class CanonicalJson
{
    public static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();
    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { Encoder = JavaScriptEncoder.Default, WriteIndented = false };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
    public static string Serialize<T>(T value) { using JsonDocument document = JsonSerializer.SerializeToDocument(value, SerializerOptions); return Canonicalize(document.RootElement); }
    public static string Canonicalize(JsonElement element)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Encoder = JavaScriptEncoder.Default })) Write(element, writer);
        return Encoding.UTF8.GetString(stream.ToArray());
    }
    private static void Write(JsonElement element, Utf8JsonWriter writer)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (JsonProperty property in element.EnumerateObject().OrderBy(static p => p.Name, StringComparer.Ordinal)) { writer.WritePropertyName(property.Name); Write(property.Value, writer); }
                writer.WriteEndObject(); break;
            case JsonValueKind.Array: writer.WriteStartArray(); foreach (JsonElement item in element.EnumerateArray()) Write(item, writer); writer.WriteEndArray(); break;
            case JsonValueKind.String: writer.WriteStringValue(element.GetString()); break;
            case JsonValueKind.Number: writer.WriteRawValue(element.GetRawText(), false); break;
            case JsonValueKind.True: writer.WriteBooleanValue(true); break;
            case JsonValueKind.False: writer.WriteBooleanValue(false); break;
            case JsonValueKind.Null: writer.WriteNullValue(); break;
            default: throw new InvalidOperationException("Unsupported JSON value.");
        }
    }
}

public static class RequestValidation
{
    public static IReadOnlyDictionary<string, string[]> Validate(BindWorldRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        CanonicalGuid(request.ScenarioId, nameof(request.ScenarioId), errors); Hash(request.ApprovedSealedPredictionHash, nameof(request.ApprovedSealedPredictionHash), errors);
        Hash(request.SourcePackageSha256, nameof(request.SourcePackageSha256), errors); Token(request.WorldId, nameof(request.WorldId), 128, errors);
        Token(request.WorldModelVersion, nameof(request.WorldModelVersion), 100, errors); Token(request.CalibrationArtifactId, nameof(request.CalibrationArtifactId), 128, errors);
        Hash(request.CalibrationArtifactSha256, nameof(request.CalibrationArtifactSha256), errors); return errors;
    }
    public static IReadOnlyDictionary<string, string[]> Validate(CreateRunRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        CanonicalGuid(request.ScenarioId, nameof(request.ScenarioId), errors); Hash(request.ApprovedSealedPredictionHash, nameof(request.ApprovedSealedPredictionHash), errors);
        Token(request.PlanArtifactId, nameof(request.PlanArtifactId), 200, errors); Hash(request.PlanArtifactSha256, nameof(request.PlanArtifactSha256), errors); return errors;
    }
    public static bool IsCanonicalGuid(string? value, out Guid guid) => Guid.TryParseExact(value, "D", out guid) && guid != Guid.Empty && value == guid.ToString("D", CultureInfo.InvariantCulture);
    private static void CanonicalGuid(string? value, string name, Dictionary<string, string[]> errors) { if (!IsCanonicalGuid(value, out _)) errors[name] = ["Value must be a nonempty canonical lowercase GUID."]; }
    private static void Hash(string? value, string name, Dictionary<string, string[]> errors)
    {
        if (value is null || value.Length != 64 || value.Any(static c => !((c >= 48 && c <= 57) || (c >= 97 && c <= 102)))) errors[name] = ["Value must be a lowercase 64-character SHA-256 hexadecimal digest."];
    }
    private static void Token(string? value, string name, int maximum, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximum || value.Any(static c => !(char.IsAsciiLetterOrDigit(c) || "-_.:".Contains(c)))) errors[name] = [$"Value must be a nonempty identifier of at most {maximum} safe characters."];
    }
}
