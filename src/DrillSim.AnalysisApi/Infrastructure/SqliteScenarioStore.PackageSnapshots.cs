using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Models;
using Microsoft.Data.Sqlite;

namespace DrillSim.AnalysisApi.Infrastructure;

internal enum ScenarioPackageSnapshotOrigin
{
    ScenarioCreation,
    LegacyBackfill
}

internal sealed record ScenarioPackageSnapshot(
    Guid ScenarioId,
    Guid FieldId,
    DateTimeOffset AsOfUtc,
    AnalysisPackage Package,
    string CanonicalPackageJson,
    string CanonicalPackageSha256,
    ScenarioPackageSnapshotOrigin Origin,
    DateTimeOffset CreatedUtc);

internal sealed record ScenarioClonePackageSnapshot(
    Guid ScenarioId,
    Guid RevealId,
    Guid ClonedFieldId,
    string ManifestSha256,
    DateTimeOffset ValidTimeUtc,
    AnalysisPackage Package,
    string CanonicalPackageJson,
    string CanonicalPackageSha256,
    DateTimeOffset CreatedUtc);

public sealed partial class SqliteScenarioStore
{
    internal async Task<AnalysisPackage?> FindClonePackageSnapshotAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: true);
        ScenarioClonePackageSnapshot? snapshot = await FindClonePackageSnapshotAsync(
            connection,
            transaction,
            scenario.ScenarioId,
            cancellationToken);
        transaction.Commit();
        if (snapshot is null)
            return null;
        if (snapshot.ClonedFieldId != scenario.ClonedFieldId ||
            snapshot.ValidTimeUtc != scenario.AsOfUtc)
            throw new InvalidDataException($"Scenario {scenario.ScenarioId:D} clone package snapshot does not match its scenario.");
        return snapshot.Package;
    }

    private static PreparedClonePackageSnapshot PrepareClonePackageSnapshot(
        ValidatedReveal reveal,
        AnalysisPackage package,
        DateTimeOffset createdUtc)
    {
        ValidatePackageIntegrity(package, reveal.ClonedFieldId);
        string canonical = PredictionJson.Canonicalize(package);
        return new(
            reveal.ScenarioId,
            reveal.RevealId,
            reveal.ClonedFieldId,
            reveal.ManifestSha256,
            reveal.ValidTimeUtc,
            PredictionJson.Deserialize<AnalysisPackage>(canonical, "clone package snapshot"),
            canonical,
            PredictionJson.ComputeSha256(package),
            createdUtc.ToUniversalTime());
    }

    private static async Task InsertClonePackageSnapshotAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        PreparedClonePackageSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO scenario_clone_package_snapshots (
                scenario_id, reveal_id, cloned_field_id, manifest_sha256,
                valid_time_utc, package_json, canonical_package_sha256, created_utc)
            VALUES (
                $scenario_id, $reveal_id, $cloned_field_id, $manifest_sha256,
                $valid_time_utc, $package_json, $canonical_package_sha256, $created_utc);
            """;
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(snapshot.ScenarioId));
        command.Parameters.AddWithValue("$reveal_id", FormatGuid(snapshot.RevealId));
        command.Parameters.AddWithValue("$cloned_field_id", FormatGuid(snapshot.ClonedFieldId));
        command.Parameters.AddWithValue("$manifest_sha256", snapshot.ManifestSha256);
        command.Parameters.AddWithValue("$valid_time_utc", FormatInstant(snapshot.ValidTimeUtc));
        command.Parameters.AddWithValue("$package_json", snapshot.CanonicalPackageJson);
        command.Parameters.AddWithValue("$canonical_package_sha256", snapshot.CanonicalPackageSha256);
        command.Parameters.AddWithValue("$created_utc", FormatInstant(snapshot.CreatedUtc));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<ScenarioClonePackageSnapshot?> FindClonePackageSnapshotAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid scenarioId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT p.scenario_id, p.reveal_id, p.cloned_field_id, p.manifest_sha256,
                   p.valid_time_utc, p.package_json, p.canonical_package_sha256, p.created_utc,
                   r.canonical_request_json, r.canonical_request_sha256
            FROM scenario_clone_package_snapshots p
            JOIN reveal_manifests r
              ON r.scenario_id = p.scenario_id AND r.reveal_id = p.reveal_id
            WHERE p.scenario_id = $scenario_id;
            """;
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        ScenarioClonePackageSnapshot snapshot = ReadClonePackageSnapshot(reader);
        ValidateClonePackageAgainstReveal(snapshot, reader.GetString(8), reader.GetString(9));
        return snapshot;
    }

    private static ScenarioClonePackageSnapshot ReadClonePackageSnapshot(SqliteDataReader reader)
    {
        Guid scenarioId = ParseGuid(reader.GetString(0));
        Guid revealId = ParseGuid(reader.GetString(1));
        Guid fieldId = ParseGuid(reader.GetString(2));
        string manifest = reader.GetString(3);
        DateTimeOffset validTime = ParseInstant(reader.GetString(4));
        string packageJson = reader.GetString(5);
        string canonicalHash = reader.GetString(6);
        DateTimeOffset createdUtc = ParseInstant(reader.GetString(7));
        AnalysisPackage package = PredictionJson.Deserialize<AnalysisPackage>(packageJson, "clone package snapshot");
        if (scenarioId == Guid.Empty || revealId == Guid.Empty || fieldId == Guid.Empty ||
            validTime == default || createdUtc == default ||
            !string.Equals(PredictionJson.Canonicalize(package), packageJson, StringComparison.Ordinal) ||
            !string.Equals(PredictionJson.ComputeSha256(package), canonicalHash, StringComparison.Ordinal))
            throw new InvalidDataException($"Scenario {scenarioId:D} clone package snapshot canonical content is invalid.");
        ValidatePackageIntegrity(package, fieldId);
        if (package.FieldId != fieldId || package.GeneratedAt != validTime)
            throw new InvalidDataException($"Scenario {scenarioId:D} clone package snapshot identity is inconsistent.");
        return new(scenarioId, revealId, fieldId, manifest, validTime, package, packageJson, canonicalHash, createdUtc);
    }

    private static void EnsureEquivalentCloneSnapshot(
        ScenarioClonePackageSnapshot existing,
        PreparedClonePackageSnapshot incoming)
    {
        if (existing.ScenarioId != incoming.ScenarioId ||
            existing.RevealId != incoming.RevealId ||
            existing.ClonedFieldId != incoming.ClonedFieldId ||
            existing.ValidTimeUtc != incoming.ValidTimeUtc ||
            !string.Equals(existing.ManifestSha256, incoming.ManifestSha256, StringComparison.Ordinal) ||
            !string.Equals(existing.CanonicalPackageJson, incoming.CanonicalPackageJson, StringComparison.Ordinal) ||
            !string.Equals(existing.CanonicalPackageSha256, incoming.CanonicalPackageSha256, StringComparison.Ordinal))
            throw Conflict("Clone package snapshot conflict", "A different immutable clone package snapshot already exists.");
    }

    private static void ValidateClonePackageAgainstReveal(
        ScenarioClonePackageSnapshot snapshot,
        string canonicalRevealJson,
        string canonicalRevealSha256)
    {
        ValidatedReveal reveal = PredictionJson.Deserialize<ValidatedReveal>(
            canonicalRevealJson,
            "prepared reveal manifest");
        if (!string.Equals(PredictionJson.Canonicalize(reveal), canonicalRevealJson, StringComparison.Ordinal) ||
            !string.Equals(PredictionJson.ComputeSha256(reveal), canonicalRevealSha256, StringComparison.Ordinal) ||
            reveal.ScenarioId != snapshot.ScenarioId ||
            reveal.RevealId != snapshot.RevealId ||
            reveal.ClonedFieldId != snapshot.ClonedFieldId ||
            reveal.ValidTimeUtc != snapshot.ValidTimeUtc ||
            !string.Equals(reveal.ManifestSha256, snapshot.ManifestSha256, StringComparison.Ordinal))
            throw new InvalidDataException($"Scenario {snapshot.ScenarioId:D} clone package snapshot reveal binding is invalid.");
        try
        {
            _ = RevealPackageValidator.Validate(reveal, snapshot.Package);
        }
        catch (ScenarioApiException exception)
        {
            throw new InvalidDataException(
                $"Scenario {snapshot.ScenarioId:D} clone package snapshot failed reveal validation.",
                exception);
        }
    }

    internal async Task<ScenarioPackageSnapshot?> FindScenarioPackageSnapshotAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: true);
        ScenarioPackageSnapshot? snapshot = await FindScenarioPackageSnapshotAsync(
            connection,
            transaction,
            scenario,
            cancellationToken);
        transaction.Commit();
        return snapshot;
    }

    internal async Task<ScenarioPackageSnapshot> BackfillScenarioPackageSnapshotAsync(
        Scenario scenario,
        AnalysisPackage sourcePackage,
        DateTimeOffset createdUtc,
        CancellationToken cancellationToken = default)
    {
        PreparedScenarioPackageSnapshot prepared = PrepareScenarioPackageSnapshot(
            scenario,
            sourcePackage,
            ScenarioPackageSnapshotOrigin.LegacyBackfill,
            createdUtc);
        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: false);
        await EnsureScenarioSnapshotIdentityAsync(connection, transaction, scenario, cancellationToken);
        ScenarioPackageSnapshot? existing = await FindScenarioPackageSnapshotAsync(
            connection,
            transaction,
            scenario,
            cancellationToken);
        if (existing is not null)
        {
            EnsureEquivalentSnapshot(existing, prepared);
            transaction.Commit();
            return existing;
        }

        await InsertScenarioPackageSnapshotAsync(connection, transaction, prepared, cancellationToken);
        transaction.Commit();
        return prepared.ToSnapshot();
    }

    private static PreparedScenarioPackageSnapshot PrepareScenarioPackageSnapshot(
        Scenario scenario,
        AnalysisPackage sourcePackage,
        ScenarioPackageSnapshotOrigin origin,
        DateTimeOffset createdUtc)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(sourcePackage);
        if (createdUtc == default ||
            createdUtc == DateTimeOffset.MinValue ||
            createdUtc == DateTimeOffset.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(createdUtc));

        AnalysisPackage normalized = sourcePackage with { GeneratedAt = scenario.InitialAsOfUtc };
        ValidatePackageIntegrity(normalized, scenario.SourceFieldId);
        string canonicalPackageJson = PredictionJson.Canonicalize(normalized);
        string canonicalPackageSha256 = PredictionJson.ComputeSha256(normalized);
        AnalysisPackage immutablePackage = PredictionJson.Deserialize<AnalysisPackage>(
            canonicalPackageJson,
            "scenario package snapshot");
        return new PreparedScenarioPackageSnapshot(
            scenario.ScenarioId,
            scenario.SourceFieldId,
            scenario.InitialAsOfUtc,
            immutablePackage,
            canonicalPackageJson,
            canonicalPackageSha256,
            origin,
            createdUtc.ToUniversalTime());
    }

    private static async Task InsertScenarioPackageSnapshotAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        PreparedScenarioPackageSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO scenario_package_snapshots (
                scenario_id, field_id, as_of_utc, package_json,
                canonical_package_sha256, origin, created_utc)
            VALUES (
                $scenario_id, $field_id, $as_of_utc, $package_json,
                $canonical_package_sha256, $origin, $created_utc);
            """;
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(snapshot.ScenarioId));
        command.Parameters.AddWithValue("$field_id", FormatGuid(snapshot.FieldId));
        command.Parameters.AddWithValue("$as_of_utc", FormatInstant(snapshot.AsOfUtc));
        command.Parameters.AddWithValue("$package_json", snapshot.CanonicalPackageJson);
        command.Parameters.AddWithValue("$canonical_package_sha256", snapshot.CanonicalPackageSha256);
        command.Parameters.AddWithValue("$origin", snapshot.Origin.ToString());
        command.Parameters.AddWithValue("$created_utc", FormatInstant(snapshot.CreatedUtc));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<ScenarioPackageSnapshot?> FindScenarioPackageSnapshotAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Scenario scenario,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT scenario_id, field_id, as_of_utc, package_json,
                   canonical_package_sha256, origin, created_utc
            FROM scenario_package_snapshots
            WHERE scenario_id = $scenario_id;
            """;
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenario.ScenarioId));
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        ScenarioPackageSnapshot snapshot = ReadScenarioPackageSnapshot(reader);
        EnsureSnapshotMatchesScenario(snapshot, scenario);
        return snapshot;
    }

    private static ScenarioPackageSnapshot ReadScenarioPackageSnapshot(SqliteDataReader reader)
    {
        Guid scenarioId = ParseGuid(reader.GetString(0));
        Guid fieldId = ParseGuid(reader.GetString(1));
        DateTimeOffset asOfUtc = ParseInstant(reader.GetString(2));
        string packageJson = reader.GetString(3);
        string canonicalPackageSha256 = reader.GetString(4);
        string originText = reader.GetString(5);
        DateTimeOffset createdUtc = ParseInstant(reader.GetString(6));
        if (scenarioId == Guid.Empty ||
            fieldId == Guid.Empty ||
            asOfUtc == default ||
            asOfUtc == DateTimeOffset.MinValue ||
            asOfUtc == DateTimeOffset.MaxValue ||
            createdUtc == default ||
            createdUtc == DateTimeOffset.MinValue ||
            createdUtc == DateTimeOffset.MaxValue)
        {
            throw new InvalidDataException("Scenario package snapshot identity or timestamps are invalid.");
        }
        if (!Enum.TryParse(originText, ignoreCase: false, out ScenarioPackageSnapshotOrigin origin))
            throw new InvalidDataException($"Scenario {scenarioId:D} has unsupported package snapshot origin '{originText}'.");

        AnalysisPackage package = PredictionJson.Deserialize<AnalysisPackage>(
            packageJson,
            "scenario package snapshot");
        if (!string.Equals(PredictionJson.Canonicalize(package), packageJson, StringComparison.Ordinal) ||
            !string.Equals(
                PredictionJson.ComputeSha256(package),
                canonicalPackageSha256,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Scenario {scenarioId:D} package snapshot canonical content is invalid.");
        }
        ValidatePackageIntegrity(package, fieldId);
        if (package.GeneratedAt != asOfUtc)
            throw new InvalidDataException($"Scenario {scenarioId:D} package snapshot time is inconsistent.");

        return new ScenarioPackageSnapshot(
            scenarioId,
            fieldId,
            asOfUtc,
            package,
            packageJson,
            canonicalPackageSha256,
            origin,
            createdUtc);
    }

    internal static void ValidatePackageIntegrity(AnalysisPackage package, Guid expectedFieldId)
    {
        if (package.FieldId != expectedFieldId ||
            JsonAccess.MetaId(package.Field) != expectedFieldId ||
            package.Clusters is null ||
            package.Wells is null ||
            package.WellBores is null ||
            package.WellBoreArchitectures is null ||
            package.Trajectories is null ||
            package.GeologicalProperties is null ||
            package.SourceCounts is null ||
            package.DataGaps is null ||
            package.SourceCounts != new SourceCounts(
                1,
                package.Clusters.Count,
                package.Wells.Count,
                package.WellBores.Count,
                package.WellBoreArchitectures.Count,
                package.Trajectories.Count,
                package.GeologicalProperties.Count))
        {
            throw new InvalidDataException(
                $"Field {expectedFieldId:D} package snapshot has inconsistent identity or source counts.");
        }

        string expectedHash = new CanonicalJsonHasher().Compute(
            package.FieldId,
            package.Field,
            package.Clusters,
            package.Wells,
            package.WellBores,
            package.WellBoreArchitectures,
            package.Trajectories,
            package.GeologicalProperties,
            package.SourceCounts,
            package.DataGaps);
        if (!string.Equals(expectedHash, package.Sha256, StringComparison.Ordinal))
            throw new InvalidDataException($"Field {expectedFieldId:D} package snapshot content hash is invalid.");
    }

    private static void EnsureSnapshotMatchesScenario(
        ScenarioPackageSnapshot snapshot,
        Scenario scenario)
    {
        if (snapshot.ScenarioId != scenario.ScenarioId ||
            snapshot.FieldId != scenario.SourceFieldId ||
            snapshot.AsOfUtc != scenario.InitialAsOfUtc)
        {
            throw new InvalidDataException(
                $"Scenario {scenario.ScenarioId:D} package snapshot identity is inconsistent.");
        }
    }

    private static void EnsureEquivalentSnapshot(
        ScenarioPackageSnapshot existing,
        PreparedScenarioPackageSnapshot incoming)
    {
        if (existing.ScenarioId != incoming.ScenarioId ||
            existing.FieldId != incoming.FieldId ||
            existing.AsOfUtc != incoming.AsOfUtc ||
            !string.Equals(
                existing.CanonicalPackageSha256,
                incoming.CanonicalPackageSha256,
                StringComparison.Ordinal) ||
            !string.Equals(
                existing.CanonicalPackageJson,
                incoming.CanonicalPackageJson,
                StringComparison.Ordinal))
        {
            throw Conflict(
                "Scenario package snapshot conflict",
                $"Scenario {incoming.ScenarioId:D} already has a different immutable source package snapshot.");
        }
    }

    private static async Task EnsureScenarioSnapshotIdentityAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Scenario scenario,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT source_field_id, initial_as_of_utc
            FROM scenarios
            WHERE scenario_id = $scenario_id;
            """;
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenario.ScenarioId));
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException($"Scenario {scenario.ScenarioId:D} disappeared before snapshot persistence.");
        if (ParseGuid(reader.GetString(0)) != scenario.SourceFieldId ||
            ParseInstant(reader.GetString(1)) != scenario.InitialAsOfUtc)
        {
            throw Conflict(
                "Scenario package snapshot conflict",
                $"Scenario {scenario.ScenarioId:D} changed before its source package snapshot could be persisted.");
        }
    }

    private static async Task ValidateScenarioPackageSnapshotsAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT p.scenario_id, p.field_id, p.as_of_utc, p.package_json,
                   p.canonical_package_sha256, p.origin, p.created_utc,
                   s.source_field_id, s.initial_as_of_utc
            FROM scenario_package_snapshots p
            JOIN scenarios s ON s.scenario_id = p.scenario_id
            ORDER BY p.scenario_id;
            """;
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            ScenarioPackageSnapshot snapshot = ReadScenarioPackageSnapshot(reader);
            if (snapshot.FieldId != ParseGuid(reader.GetString(7)) ||
                snapshot.AsOfUtc != ParseInstant(reader.GetString(8)))
            {
                throw new InvalidDataException(
                    $"Scenario {snapshot.ScenarioId:D} package snapshot does not match its scenario.");
            }
        }
    }

    private static async Task ValidateClonePackageSnapshotsAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
                SELECT p.scenario_id, p.reveal_id, p.cloned_field_id, p.manifest_sha256,
                       p.valid_time_utc, p.package_json, p.canonical_package_sha256, p.created_utc,
                       r.cloned_field_id, r.valid_time_utc, r.manifest_sha256,
                       r.canonical_request_json, r.canonical_request_sha256
                FROM scenario_clone_package_snapshots p
                JOIN reveal_manifests r
                  ON r.scenario_id = p.scenario_id AND r.reveal_id = p.reveal_id
                ORDER BY p.scenario_id;
                """;
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            ScenarioClonePackageSnapshot snapshot = ReadClonePackageSnapshot(reader);
            if (snapshot.ClonedFieldId != ParseGuid(reader.GetString(8)) ||
                snapshot.ValidTimeUtc != ParseInstant(reader.GetString(9)) ||
                !string.Equals(snapshot.ManifestSha256, reader.GetString(10), StringComparison.Ordinal))
                throw new InvalidDataException($"Scenario {snapshot.ScenarioId:D} clone package snapshot does not match its reveal.");
            ValidateClonePackageAgainstReveal(snapshot, reader.GetString(11), reader.GetString(12));
        }
    }

    private sealed record PreparedScenarioPackageSnapshot(
        Guid ScenarioId,
        Guid FieldId,
        DateTimeOffset AsOfUtc,
        AnalysisPackage Package,
        string CanonicalPackageJson,
        string CanonicalPackageSha256,
        ScenarioPackageSnapshotOrigin Origin,
        DateTimeOffset CreatedUtc)
    {
        internal ScenarioPackageSnapshot ToSnapshot() =>
            new(
                ScenarioId,
                FieldId,
                AsOfUtc,
                Package,
                CanonicalPackageJson,
                CanonicalPackageSha256,
                Origin,
                CreatedUtc);
    }

    private sealed record PreparedClonePackageSnapshot(
        Guid ScenarioId,
        Guid RevealId,
        Guid ClonedFieldId,
        string ManifestSha256,
        DateTimeOffset ValidTimeUtc,
        AnalysisPackage Package,
        string CanonicalPackageJson,
        string CanonicalPackageSha256,
        DateTimeOffset CreatedUtc);
}
