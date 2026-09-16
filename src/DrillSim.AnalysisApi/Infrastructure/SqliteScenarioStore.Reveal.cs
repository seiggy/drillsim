using System.Security.Cryptography;
using System.Text;
using DrillSim.AnalysisApi.Models;
using Microsoft.Data.Sqlite;

namespace DrillSim.AnalysisApi.Infrastructure;

public sealed partial class SqliteScenarioStore
{
    private static readonly ScenarioStatus[] RevealSourceStatuses =
    [
        ScenarioStatus.HumanApproved,
        ScenarioStatus.WorldBound,
        ScenarioStatus.Queued,
        ScenarioStatus.Drilling,
        ScenarioStatus.Surveying,
        ScenarioStatus.Logging,
        ScenarioStatus.CompletionDesigned,
        ScenarioStatus.Producing,
        ScenarioStatus.ReadyToReveal,
        ScenarioStatus.PublishFailed
    ];

    internal async Task<RevealReceipt> PrepareRevealAsync(
        ValidatedReveal reveal,
        AnalysisPackage clonePackage,
        string canonicalRequestJson,
        string canonicalRequestSha256,
        DateTimeOffset createdUtc,
        CancellationToken cancellationToken = default)
    {
        PreparedClonePackageSnapshot preparedSnapshot =
            PrepareClonePackageSnapshot(reveal, clonePackage, createdUtc);
        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: false);

        StoredReveal? existing = await FindStoredRevealAsync(
            connection,
            transaction,
            reveal.ScenarioId,
            reveal.RevealId,
            cancellationToken);
        if (existing is not null)
        {
            if (existing.ScenarioId != reveal.ScenarioId || existing.RevealId != reveal.RevealId ||
                !string.Equals(existing.CanonicalRequestJson, canonicalRequestJson, StringComparison.Ordinal) ||
                !string.Equals(existing.CanonicalRequestSha256, canonicalRequestSha256, StringComparison.Ordinal))
            {
                throw Conflict(
                    "Reveal identity conflict",
                    "The scenario or reveal identifier is already bound to different canonical reveal content.");
            }
            ScenarioClonePackageSnapshot? existingSnapshot = await FindClonePackageSnapshotAsync(
                connection, transaction, reveal.ScenarioId, cancellationToken);
            if (existingSnapshot is null)
                throw Conflict("Clone package snapshot missing", "The prepared reveal has no immutable clone package snapshot.");
            EnsureEquivalentCloneSnapshot(existingSnapshot, preparedSnapshot);
            RevealReceipt replay = existing.ToReceipt();
            transaction.Commit();
            return replay;
        }

        await EnsureProductionIdentityAvailableAsync(
            connection,
            transaction,
            reveal.ProductionSeries.SeriesId,
            cancellationToken);
        StoredRevealScenario scenario = await ReadRevealScenarioAsync(
            connection,
            transaction,
            reveal.ScenarioId,
            cancellationToken);
        ValidateRevealTransition(scenario, reveal);
        await EnsureEvidenceIsNewAsync(connection, transaction, reveal, cancellationToken);

        string evidenceJson = PredictionJson.Canonicalize(reveal.Evidence);
        string checkpointsJson = PredictionJson.Canonicalize(reveal.ProductionSeries.CheckpointYears);
        await InsertRevealManifestAsync(
            connection,
            transaction,
            reveal,
            evidenceJson,
            canonicalRequestJson,
            canonicalRequestSha256,
            createdUtc,
            cancellationToken);
        await InsertProductionSeriesAsync(
            connection,
            transaction,
            reveal,
            checkpointsJson,
            createdUtc,
            cancellationToken);
        await InsertClonePackageSnapshotAsync(
            connection,
            transaction,
            preparedSnapshot,
            cancellationToken);

        _beforeRevealPrepareCommit?.Invoke();
        transaction.Commit();
        return ToReceipt(reveal, RevealReceiptStatus.Prepared);
    }

    internal async Task<RevealReceipt> FinalizeRevealAsync(
        Guid scenarioId,
        Guid revealId,
        string manifestSha256,
        DateTimeOffset modifiedUtc,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: false);
        StoredReveal? stored = await FindStoredRevealAsync(
            connection,
            transaction,
            scenarioId,
            revealId,
            cancellationToken);
        if (stored is null)
            throw Conflict("Reveal is not prepared", "A matching prepared reveal manifest is required before finalization.");
        if (stored.ScenarioId != scenarioId || stored.RevealId != revealId ||
            !string.Equals(stored.ManifestSha256, manifestSha256, StringComparison.Ordinal))
        {
            throw Conflict("Reveal finalize conflict", "Finalize identity does not match the prepared reveal manifest.");
        }
        if (stored.Status == RevealReceiptStatus.Revealed)
        {
            RevealReceipt replay = stored.ToReceipt();
            transaction.Commit();
            return replay;
        }
        if (stored.Status != RevealReceiptStatus.Prepared)
            throw Conflict("Reveal is not prepared", "The reveal manifest is not in Prepared status.");

        ScenarioClonePackageSnapshot? cloneSnapshot = await FindClonePackageSnapshotAsync(
            connection, transaction, scenarioId, cancellationToken);
        if (cloneSnapshot is null ||
            cloneSnapshot.RevealId != revealId ||
            !string.Equals(cloneSnapshot.ManifestSha256, manifestSha256, StringComparison.Ordinal))
            throw Conflict("Clone package snapshot missing", "A matching immutable clone package snapshot is required before finalization.");

        ValidatedReveal reveal = ReadValidatedReveal(stored);
        StoredRevealScenario scenario = await ReadRevealScenarioAsync(
            connection,
            transaction,
            scenarioId,
            cancellationToken);
        ValidateRevealTransition(scenario, reveal);
        await EnsureEvidenceIsNewAsync(connection, transaction, reveal, cancellationToken);
        await InsertRevealVisibilityAsync(connection, transaction, reveal, cancellationToken);
        await AdvanceScenarioToRevealedAsync(
            connection,
            transaction,
            scenario,
            reveal,
            modifiedUtc,
            cancellationToken);
        await MarkRevealFinalizedAsync(connection, transaction, revealId, cancellationToken);

        _beforeRevealFinalizeCommit?.Invoke();
        transaction.Commit();
        return ToReceipt(reveal, RevealReceiptStatus.Revealed);
    }

    internal async Task<RevealReceipt> BackfillClonePackageSnapshotAsync(
        ValidatedReveal reveal,
        AnalysisPackage clonePackage,
        DateTimeOffset createdUtc,
        CancellationToken cancellationToken = default)
    {
        PreparedClonePackageSnapshot prepared = PrepareClonePackageSnapshot(reveal, clonePackage, createdUtc);
        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: false);
        StoredReveal? stored = await FindStoredRevealExactAsync(
            connection, transaction, reveal.ScenarioId, reveal.RevealId, cancellationToken);
        if (stored is null || stored.Status != RevealReceiptStatus.Revealed)
            throw Conflict("Reveal is not public", "Clone snapshot backfill requires an existing finalized reveal.");
        if (stored.ClonedFieldId != reveal.ClonedFieldId ||
            stored.ValidTimeUtc != reveal.ValidTimeUtc ||
            !string.Equals(stored.ManifestSha256, reveal.ManifestSha256, StringComparison.Ordinal) ||
            stored.EvidenceCount != reveal.Evidence.Count ||
            stored.ProductionSeriesId != reveal.ProductionSeries.SeriesId)
            throw Conflict("Reveal identity conflict", "Backfill content does not match the immutable reveal manifest.");

        ValidatedReveal storedReveal = ReadValidatedReveal(stored);
        if (!string.Equals(PredictionJson.Canonicalize(storedReveal), PredictionJson.Canonicalize(reveal), StringComparison.Ordinal))
            throw Conflict("Reveal identity conflict", "Backfill content does not match the immutable reveal manifest.");

        StoredRevealScenario scenario = await ReadRevealScenarioAsync(
            connection, transaction, reveal.ScenarioId, cancellationToken);
        if (scenario.Status is not ScenarioStatus.Revealed and not ScenarioStatus.Scored ||
            scenario.ClonedFieldId != reveal.ClonedFieldId)
            throw Conflict("Scenario is not revealed", "Clone snapshot backfill requires a Revealed or Scored scenario.");

        ScenarioClonePackageSnapshot? existing = await FindClonePackageSnapshotAsync(
            connection, transaction, reveal.ScenarioId, cancellationToken);
        if (existing is null)
            await InsertClonePackageSnapshotAsync(connection, transaction, prepared, cancellationToken);
        else
            EnsureEquivalentCloneSnapshot(existing, prepared);
        transaction.Commit();
        return stored.ToReceipt();
    }

    public async Task<RevealReceipt?> FindRevealAsync(
        Guid scenarioId,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: true);
        StoredReveal? stored = await FindStoredRevealByScenarioAsync(
            connection,
            transaction,
            scenarioId,
            cancellationToken);
        transaction.Commit();
        return stored?.Status == RevealReceiptStatus.Revealed ? stored.ToReceipt() : null;
    }

    public async Task<RevealReceipt?> FindRevealStatusAsync(
        Guid scenarioId,
        Guid revealId,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: true);
        StoredReveal? stored = await FindStoredRevealExactAsync(
            connection,
            transaction,
            scenarioId,
            revealId,
            cancellationToken);
        transaction.Commit();
        return stored?.ToReceipt();
    }

    public async Task<PublicProductionSeriesMetadata?> FindProductionSeriesAsync(
        Guid scenarioId,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: true);
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT p.scenario_id, p.reveal_id, p.series_id, p.model_version, p.content_sha256,
                   p.month_count, p.checkpoint_years_json, p.created_utc
            FROM public_production_series p
            JOIN reveal_manifests r ON r.reveal_id = p.reveal_id
            WHERE p.scenario_id = $scenario_id AND r.receipt_status = 'Revealed';
            """;
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            await reader.DisposeAsync();
            transaction.Commit();
            return null;
        }

        int[] checkpoints = PredictionJson.Deserialize<int[]>(reader.GetString(6), "production checkpoint years");
        if (!checkpoints.SequenceEqual(new[] { 1, 3, 5 }))
            throw new InvalidDataException($"Scenario {scenarioId:D} has invalid public production checkpoints.");
        var metadata = new PublicProductionSeriesMetadata(
            ParseGuid(reader.GetString(0)),
            ParseGuid(reader.GetString(1)),
            ParseGuid(reader.GetString(2)),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetInt32(5),
            checkpoints,
            ReadStoredInstant(reader, 7, "production created_utc", FormatGuid(scenarioId)));
        if (metadata.ScenarioId != scenarioId || metadata.MonthCount != 60)
            throw new InvalidDataException($"Scenario {scenarioId:D} has invalid public production metadata.");
        await reader.DisposeAsync();
        transaction.Commit();
        return metadata;
    }

    private static async Task<StoredReveal?> FindStoredRevealAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid scenarioId,
        Guid revealId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT scenario_id, reveal_id, cloned_field_id, valid_time_utc, receipt_status,
                   manifest_sha256, evidence_count, production_series_id,
                   canonical_request_json, canonical_request_sha256
            FROM reveal_manifests
            WHERE scenario_id = $scenario_id OR reveal_id = $reveal_id;
            """;
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
        command.Parameters.AddWithValue("$reveal_id", FormatGuid(revealId));
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        StoredReveal result = ReadStoredReveal(reader);
        if (await reader.ReadAsync(cancellationToken))
            throw Conflict("Reveal identity conflict", "Scenario and reveal identifiers are bound to different reveal records.");
        return result;
    }

    private static async Task<StoredReveal?> FindStoredRevealExactAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid scenarioId,
        Guid revealId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT scenario_id, reveal_id, cloned_field_id, valid_time_utc, receipt_status,
                   manifest_sha256, evidence_count, production_series_id,
                   canonical_request_json, canonical_request_sha256
            FROM reveal_manifests
            WHERE scenario_id = $scenario_id AND reveal_id = $reveal_id;
            """;
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
        command.Parameters.AddWithValue("$reveal_id", FormatGuid(revealId));
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadStoredReveal(reader) : null;
    }

    private static async Task<StoredReveal?> FindStoredRevealByScenarioAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid scenarioId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT scenario_id, reveal_id, cloned_field_id, valid_time_utc, receipt_status,
                   manifest_sha256, evidence_count, production_series_id,
                   canonical_request_json, canonical_request_sha256
            FROM reveal_manifests
            WHERE scenario_id = $scenario_id;
            """;
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadStoredReveal(reader) : null;
    }

    private static StoredReveal ReadStoredReveal(SqliteDataReader reader)
    {
        string scenarioIdText = reader.GetString(0);
        string requestJson = reader.GetString(8);
        string requestHash = reader.GetString(9);
        string computedHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(requestJson))).ToLowerInvariant();
        if (!string.Equals(requestHash, computedHash, StringComparison.Ordinal))
            throw new InvalidDataException($"Scenario {scenarioIdText} has invalid reveal request integrity data.");
        string statusText = reader.GetString(4);
        if (!Enum.TryParse(statusText, ignoreCase: false, out RevealReceiptStatus status))
            throw new InvalidDataException($"Scenario {scenarioIdText} has invalid reveal receipt status.");
        return new StoredReveal(
            ParseGuid(scenarioIdText),
            ParseGuid(reader.GetString(1)),
            ParseGuid(reader.GetString(2)),
            ReadStoredInstant(reader, 3, "valid_time_utc", scenarioIdText),
            status,
            reader.GetString(5),
            reader.GetInt32(6),
            ParseGuid(reader.GetString(7)),
            requestJson,
            requestHash);
    }

    private static async Task EnsureProductionIdentityAvailableAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid seriesId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT scenario_id, reveal_id
            FROM public_production_series
            WHERE series_id = $series_id;
            """;
        command.Parameters.AddWithValue("$series_id", FormatGuid(seriesId));
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
            throw Conflict("Production series identity conflict", "The production series ID is already bound to a reveal.");
    }

    private static async Task<StoredRevealScenario> ReadRevealScenarioAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid scenarioId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT source_field_id, cloned_field_id, initial_as_of_utc, as_of_utc,
                   observation_model_version, status
            FROM scenarios
            WHERE scenario_id = $scenario_id;
            """;
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new ScenarioApiException(
                StatusCodes.Status404NotFound,
                "Scenario not found",
                $"Scenario {scenarioId:D} does not exist.");
        }
        string scenarioIdText = FormatGuid(scenarioId);
        string statusText = reader.GetString(5);
        if (!Enum.TryParse(statusText, ignoreCase: false, out ScenarioStatus status))
            throw new InvalidDataException($"Scenario {scenarioId:D} has unsupported status '{statusText}'.");
        return new StoredRevealScenario(
            scenarioId,
            ParseGuid(reader.GetString(0)),
            reader.IsDBNull(1) ? null : ParseGuid(reader.GetString(1)),
            ReadStoredInstant(reader, 2, "initial_as_of_utc", scenarioIdText),
            ReadStoredInstant(reader, 3, "as_of_utc", scenarioIdText),
            reader.GetString(4),
            status);
    }

    private static void ValidateRevealTransition(StoredRevealScenario scenario, ValidatedReveal reveal)
    {
        if (!RevealSourceStatuses.Contains(scenario.Status))
            throw Conflict("Invalid scenario status", $"Scenario status {scenario.Status} cannot transition to Revealed.");
        if (reveal.ClonedFieldId == scenario.SourceFieldId)
            throw Conflict("Invalid cloned field", "ClonedFieldId must differ from SourceFieldId.");
        if (scenario.ClonedFieldId is Guid existingClone && existingClone != reveal.ClonedFieldId)
            throw Conflict("Cloned field conflict", "The scenario is already bound to a different cloned field.");
        if (reveal.ValidTimeUtc < scenario.InitialAsOfUtc || reveal.ValidTimeUtc <= scenario.AsOfUtc)
            throw Conflict("Invalid reveal time", "ValidTimeUtc must be at or after InitialAsOfUtc and later than the current scenario clock.");
        if (!string.Equals(reveal.ObservationModelVersion, scenario.ObservationModelVersion, StringComparison.Ordinal))
            throw Conflict("Observation model mismatch", "The reveal observation model does not match the scenario.");
    }

    private static async Task EnsureEvidenceIsNewAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        ValidatedReveal reveal,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT evidence_id FROM evidence_visibility WHERE scenario_id = $scenario_id;";
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(reveal.ScenarioId));
        var existingIds = new HashSet<string>(StringComparer.Ordinal);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            existingIds.Add(reader.GetString(0));
        int collisionCount = reveal.Evidence.Count(item => existingIds.Contains(item.EvidenceId));
        if (collisionCount > 0)
        {
            throw Conflict(
                "Reveal evidence conflict",
                $"The reveal contains {collisionCount} evidence ID(s) already registered for the scenario.");
        }
    }

    private static async Task InsertRevealManifestAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        ValidatedReveal reveal,
        string evidenceJson,
        string canonicalRequestJson,
        string canonicalRequestSha256,
        DateTimeOffset createdUtc,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO reveal_manifests (
                reveal_id, scenario_id, run_id, cloned_field_id, valid_time_utc,
                observation_model_version, manifest_sha256, evidence_json, evidence_count,
                production_series_id, canonical_request_json, canonical_request_sha256,
                receipt_status, created_utc)
            VALUES (
                $reveal_id, $scenario_id, $run_id, $cloned_field_id, $valid_time_utc,
                $observation_model_version, $manifest_sha256, $evidence_json, $evidence_count,
                $production_series_id, $canonical_request_json, $canonical_request_sha256,
                $receipt_status, $created_utc);
            """;
        command.Parameters.AddWithValue("$reveal_id", FormatGuid(reveal.RevealId));
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(reveal.ScenarioId));
        command.Parameters.AddWithValue("$run_id", FormatGuid(reveal.RunId));
        command.Parameters.AddWithValue("$cloned_field_id", FormatGuid(reveal.ClonedFieldId));
        command.Parameters.AddWithValue("$valid_time_utc", FormatInstant(reveal.ValidTimeUtc));
        command.Parameters.AddWithValue("$observation_model_version", reveal.ObservationModelVersion);
        command.Parameters.AddWithValue("$manifest_sha256", reveal.ManifestSha256);
        command.Parameters.AddWithValue("$evidence_json", evidenceJson);
        command.Parameters.AddWithValue("$evidence_count", reveal.Evidence.Count);
        command.Parameters.AddWithValue("$production_series_id", FormatGuid(reveal.ProductionSeries.SeriesId));
        command.Parameters.AddWithValue("$canonical_request_json", canonicalRequestJson);
        command.Parameters.AddWithValue("$canonical_request_sha256", canonicalRequestSha256);
        command.Parameters.AddWithValue("$receipt_status", RevealReceiptStatus.Prepared.ToString());
        command.Parameters.AddWithValue("$created_utc", FormatInstant(createdUtc));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertProductionSeriesAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        ValidatedReveal reveal,
        string checkpointsJson,
        DateTimeOffset createdUtc,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO public_production_series (
                series_id, scenario_id, reveal_id, model_version, content_sha256,
                month_count, checkpoint_years_json, created_utc)
            VALUES (
                $series_id, $scenario_id, $reveal_id, $model_version, $content_sha256,
                $month_count, $checkpoint_years_json, $created_utc);
            """;
        command.Parameters.AddWithValue("$series_id", FormatGuid(reveal.ProductionSeries.SeriesId));
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(reveal.ScenarioId));
        command.Parameters.AddWithValue("$reveal_id", FormatGuid(reveal.RevealId));
        command.Parameters.AddWithValue("$model_version", reveal.ProductionSeries.ModelVersion);
        command.Parameters.AddWithValue("$content_sha256", reveal.ProductionSeries.ContentSha256);
        command.Parameters.AddWithValue("$month_count", reveal.ProductionSeries.MonthCount);
        command.Parameters.AddWithValue("$checkpoint_years_json", checkpointsJson);
        command.Parameters.AddWithValue("$created_utc", FormatInstant(createdUtc));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static ValidatedReveal ReadValidatedReveal(StoredReveal stored)
    {
        ValidatedReveal reveal = PredictionJson.Deserialize<ValidatedReveal>(
            stored.CanonicalRequestJson,
            "prepared reveal manifest");
        if (!string.Equals(PredictionJson.Canonicalize(reveal), stored.CanonicalRequestJson, StringComparison.Ordinal) ||
            reveal.ScenarioId != stored.ScenarioId || reveal.RevealId != stored.RevealId ||
            reveal.ClonedFieldId != stored.ClonedFieldId || reveal.ValidTimeUtc != stored.ValidTimeUtc ||
            !string.Equals(reveal.ManifestSha256, stored.ManifestSha256, StringComparison.Ordinal) ||
            reveal.Evidence.Count != stored.EvidenceCount ||
            reveal.ProductionSeries.SeriesId != stored.ProductionSeriesId)
        {
            throw new InvalidDataException($"Scenario {stored.ScenarioId:D} has inconsistent prepared reveal content.");
        }
        return reveal;
    }

    private static async Task MarkRevealFinalizedAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid revealId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE reveal_manifests
            SET receipt_status = $revealed
            WHERE reveal_id = $reveal_id AND receipt_status = $prepared;
            """;
        command.Parameters.AddWithValue("$revealed", RevealReceiptStatus.Revealed.ToString());
        command.Parameters.AddWithValue("$reveal_id", FormatGuid(revealId));
        command.Parameters.AddWithValue("$prepared", RevealReceiptStatus.Prepared.ToString());
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
            throw Conflict("Concurrent finalize conflict", "The prepared reveal status changed during finalization.");
    }

    private static async Task InsertRevealVisibilityAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        ValidatedReveal reveal,
        CancellationToken cancellationToken)
    {
        foreach (ValidatedRevealEvidence evidence in reveal.Evidence.OrderBy(item => item.EvidenceId, StringComparer.Ordinal))
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO evidence_visibility (
                    scenario_id, evidence_id, record_kind, visible_from_utc, visible_until_utc, reveal_id)
                VALUES (
                    $scenario_id, $evidence_id, $record_kind, $visible_from_utc, NULL, $reveal_id);
                """;
            command.Parameters.AddWithValue("$scenario_id", FormatGuid(reveal.ScenarioId));
            command.Parameters.AddWithValue("$evidence_id", evidence.EvidenceId);
            command.Parameters.AddWithValue("$record_kind", evidence.RecordKind);
            command.Parameters.AddWithValue("$visible_from_utc", FormatInstant(reveal.ValidTimeUtc));
            command.Parameters.AddWithValue("$reveal_id", FormatGuid(reveal.RevealId));
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task AdvanceScenarioToRevealedAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        StoredRevealScenario scenario,
        ValidatedReveal reveal,
        DateTimeOffset modifiedUtc,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE scenarios
            SET cloned_field_id = $cloned_field_id,
                as_of_utc = $as_of_utc,
                status = $status,
                modified_utc = $modified_utc
            WHERE scenario_id = $scenario_id
              AND status = $expected_status
              AND as_of_utc = $expected_as_of_utc
              AND (cloned_field_id IS NULL OR cloned_field_id = $cloned_field_id);
            """;
        command.Parameters.AddWithValue("$cloned_field_id", FormatGuid(reveal.ClonedFieldId));
        command.Parameters.AddWithValue("$as_of_utc", FormatInstant(reveal.ValidTimeUtc));
        command.Parameters.AddWithValue("$status", ScenarioStatus.Revealed.ToString());
        command.Parameters.AddWithValue("$modified_utc", FormatInstant(modifiedUtc));
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(reveal.ScenarioId));
        command.Parameters.AddWithValue("$expected_status", scenario.Status.ToString());
        command.Parameters.AddWithValue("$expected_as_of_utc", FormatInstant(scenario.AsOfUtc));
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
            throw Conflict("Concurrent reveal conflict", "The scenario changed while the reveal was being published.");
    }

    private static RevealReceipt ToReceipt(ValidatedReveal reveal, RevealReceiptStatus status) => new(
        reveal.ScenarioId,
        reveal.RevealId,
        reveal.ClonedFieldId,
        reveal.ValidTimeUtc,
        status,
        reveal.ManifestSha256,
        reveal.Evidence.Count,
        reveal.ProductionSeries.SeriesId);

    private sealed record StoredReveal(
        Guid ScenarioId,
        Guid RevealId,
        Guid ClonedFieldId,
        DateTimeOffset ValidTimeUtc,
        RevealReceiptStatus Status,
        string ManifestSha256,
        int EvidenceCount,
        Guid ProductionSeriesId,
        string CanonicalRequestJson,
        string CanonicalRequestSha256)
    {
        public RevealReceipt ToReceipt() => new(
            ScenarioId,
            RevealId,
            ClonedFieldId,
            ValidTimeUtc,
            Status,
            ManifestSha256,
            EvidenceCount,
            ProductionSeriesId);
    }

    private sealed record StoredRevealScenario(
        Guid ScenarioId,
        Guid SourceFieldId,
        Guid? ClonedFieldId,
        DateTimeOffset InitialAsOfUtc,
        DateTimeOffset AsOfUtc,
        string ObservationModelVersion,
        ScenarioStatus Status);
}
