using System.Security.Cryptography;
using System.Text;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;
using Microsoft.Data.Sqlite;

namespace DrillSim.AnalysisApi.Infrastructure;

public sealed partial class SqliteScenarioStore
{
    internal async Task<PublicScorecard> PublishScorecardAsync(
        ValidatedScorecard scorecard,
        string canonicalBodyJson,
        string canonicalBodySha256,
        DateTimeOffset createdUtc,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: false);
        StoredScorecard? existing = await FindStoredScorecardAsync(
            connection,
            transaction,
            scorecard.ScenarioId,
            scorecard.ScorecardId,
            cancellationToken);
        if (existing is not null)
        {
            if (existing.Scorecard.ScenarioId != scorecard.ScenarioId ||
                existing.Scorecard.ScorecardId != scorecard.ScorecardId ||
                !string.Equals(existing.CanonicalBodyJson, canonicalBodyJson, StringComparison.Ordinal) ||
                !string.Equals(existing.CanonicalBodySha256, canonicalBodySha256, StringComparison.Ordinal))
            {
                throw Conflict(
                    "Scorecard identity conflict",
                    "The scenario or scorecard identifier is already bound to different canonical content.");
            }
            PublicScorecard replay = existing.Scorecard;
            transaction.Commit();
            return replay;
        }

        ScorecardScenarioState state = await ReadScorecardScenarioStateAsync(
            connection,
            transaction,
            scorecard.ScenarioId,
            cancellationToken);
        ValidateScorecardPublication(state, scorecard);
        string metricsJson = PredictionJson.Canonicalize(scorecard.Metrics);
        await using (var insert = connection.CreateCommand())
        {
            insert.Transaction = transaction;
            insert.CommandText = """
                INSERT INTO public_scorecards (
                    scorecard_id, scenario_id, run_id, reveal_id, scoring_model_version,
                    input_sha256, headline_metric, metrics_json, created_valid_time_utc,
                    limitation, canonical_body_json, canonical_body_sha256, created_utc)
                VALUES (
                    $scorecard_id, $scenario_id, $run_id, $reveal_id, $scoring_model_version,
                    $input_sha256, $headline_metric, $metrics_json, $created_valid_time_utc,
                    $limitation, $canonical_body_json, $canonical_body_sha256, $created_utc);
                """;
            insert.Parameters.AddWithValue("$scorecard_id", FormatGuid(scorecard.ScorecardId));
            insert.Parameters.AddWithValue("$scenario_id", FormatGuid(scorecard.ScenarioId));
            insert.Parameters.AddWithValue("$run_id", FormatGuid(scorecard.RunId));
            insert.Parameters.AddWithValue("$reveal_id", FormatGuid(scorecard.RevealId));
            insert.Parameters.AddWithValue("$scoring_model_version", scorecard.ScoringModelVersion);
            insert.Parameters.AddWithValue("$input_sha256", scorecard.InputSha256);
            insert.Parameters.AddWithValue("$headline_metric", DbValue(scorecard.HeadlineMetric));
            insert.Parameters.AddWithValue("$metrics_json", metricsJson);
            insert.Parameters.AddWithValue("$created_valid_time_utc", FormatInstant(scorecard.CreatedValidTimeUtc));
            insert.Parameters.AddWithValue("$limitation", DbValue(scorecard.Limitation));
            insert.Parameters.AddWithValue("$canonical_body_json", canonicalBodyJson);
            insert.Parameters.AddWithValue("$canonical_body_sha256", canonicalBodySha256);
            insert.Parameters.AddWithValue("$created_utc", FormatInstant(createdUtc));
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }
        await using (var update = connection.CreateCommand())
        {
            update.Transaction = transaction;
            update.CommandText = """
                UPDATE scenarios
                SET status = $scored, modified_utc = $modified_utc
                WHERE scenario_id = $scenario_id AND status = $revealed;
                """;
            update.Parameters.AddWithValue("$scored", ScenarioStatus.Scored.ToString());
            update.Parameters.AddWithValue("$modified_utc", FormatInstant(createdUtc));
            update.Parameters.AddWithValue("$scenario_id", FormatGuid(scorecard.ScenarioId));
            update.Parameters.AddWithValue("$revealed", ScenarioStatus.Revealed.ToString());
            if (await update.ExecuteNonQueryAsync(cancellationToken) != 1)
                throw Conflict("Concurrent scorecard conflict", "The scenario changed while the scorecard was being published.");
        }

        _beforeScorecardCommit?.Invoke();
        transaction.Commit();
        return ToPublicScorecard(scorecard, canonicalBodySha256);
    }

    public async Task<PublicScorecard?> FindScorecardAsync(
        Guid scenarioId,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: true);
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT c.scorecard_id, c.scenario_id, c.run_id, c.reveal_id,
                   c.scoring_model_version, c.input_sha256, c.headline_metric,
                   c.metrics_json, c.created_valid_time_utc, c.limitation,
                   c.canonical_body_json, c.canonical_body_sha256,
                   s.status
            FROM public_scorecards c
            JOIN scenarios s ON s.scenario_id = c.scenario_id
            WHERE c.scenario_id = $scenario_id
              AND s.status IN ($revealed, $scored);
            """;
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
        command.Parameters.AddWithValue("$revealed", ScenarioStatus.Revealed.ToString());
        command.Parameters.AddWithValue("$scored", ScenarioStatus.Scored.ToString());
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        StoredScorecard? stored = await reader.ReadAsync(cancellationToken)
            ? ReadStoredScorecard(reader)
            : null;
        await reader.DisposeAsync();
        transaction.Commit();
        return stored?.Scorecard;
    }

    private static async Task<StoredScorecard?> FindStoredScorecardAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid scenarioId,
        Guid scorecardId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT scorecard_id, scenario_id, run_id, reveal_id,
                   scoring_model_version, input_sha256, headline_metric,
                   metrics_json, created_valid_time_utc, limitation,
                   canonical_body_json, canonical_body_sha256
            FROM public_scorecards
            WHERE scenario_id = $scenario_id OR scorecard_id = $scorecard_id;
            """;
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
        command.Parameters.AddWithValue("$scorecard_id", FormatGuid(scorecardId));
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        StoredScorecard stored = ReadStoredScorecard(reader);
        if (await reader.ReadAsync(cancellationToken))
            throw Conflict("Scorecard identity conflict", "Scenario and scorecard identifiers are bound to different records.");
        return stored;
    }

    private static StoredScorecard ReadStoredScorecard(SqliteDataReader reader)
    {
        string canonicalJson = reader.GetString(10);
        string canonicalHash = reader.GetString(11);
        string computedHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalJson))).ToLowerInvariant();
        if (!string.Equals(canonicalHash, computedHash, StringComparison.Ordinal))
            throw new InvalidDataException("Public scorecard canonical content hash is invalid.");
        ValidatedScorecard body = PredictionJson.Deserialize<ValidatedScorecard>(canonicalJson, "public scorecard");
        if (!string.Equals(PredictionJson.Canonicalize(body), canonicalJson, StringComparison.Ordinal) ||
            body.ScorecardId != ParseGuid(reader.GetString(0)) ||
            body.ScenarioId != ParseGuid(reader.GetString(1)) ||
            body.RunId != ParseGuid(reader.GetString(2)) ||
            body.RevealId != ParseGuid(reader.GetString(3)) ||
            !string.Equals(body.ScoringModelVersion, reader.GetString(4), StringComparison.Ordinal) ||
            !string.Equals(body.InputSha256, reader.GetString(5), StringComparison.Ordinal) ||
            !string.Equals(body.HeadlineMetric, reader.IsDBNull(6) ? null : reader.GetString(6), StringComparison.Ordinal) ||
            !string.Equals(PredictionJson.Canonicalize(body.Metrics), reader.GetString(7), StringComparison.Ordinal) ||
            body.CreatedValidTimeUtc != ParseInstant(reader.GetString(8)) ||
            !string.Equals(body.Limitation, reader.IsDBNull(9) ? null : reader.GetString(9), StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Scenario {body.ScenarioId:D} has inconsistent public scorecard content.");
        }
        return new StoredScorecard(ToPublicScorecard(body, canonicalHash), canonicalJson, canonicalHash);
    }

    private static async Task<ScorecardScenarioState> ReadScorecardScenarioStateAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid scenarioId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT s.status, s.scoring_model_version, s.as_of_utc,
                   r.reveal_id, r.run_id, r.receipt_status
            FROM scenarios s
            LEFT JOIN reveal_manifests r ON r.scenario_id = s.scenario_id
            WHERE s.scenario_id = $scenario_id;
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
        string statusText = reader.GetString(0);
        if (!Enum.TryParse(statusText, ignoreCase: false, out ScenarioStatus status))
            throw new InvalidDataException($"Scenario {scenarioId:D} has unsupported status '{statusText}'.");
        return new ScorecardScenarioState(
            status,
            reader.GetString(1),
            ParseInstant(reader.GetString(2)),
            reader.IsDBNull(3) ? null : ParseGuid(reader.GetString(3)),
            reader.IsDBNull(4) ? null : ParseGuid(reader.GetString(4)),
            reader.IsDBNull(5) ? null : reader.GetString(5));
    }

    private static void ValidateScorecardPublication(ScorecardScenarioState state, ValidatedScorecard scorecard)
    {
        if (state.Status != ScenarioStatus.Revealed)
            throw Conflict("Invalid scenario status", $"Scenario status {state.Status} cannot transition to Scored.");
        if (state.RevealId != scorecard.RevealId ||
            !string.Equals(state.RevealStatus, RevealReceiptStatus.Revealed.ToString(), StringComparison.Ordinal))
        {
            throw Conflict("Scorecard reveal mismatch", "Scorecard revealId does not match the committed reveal.");
        }
        if (state.RunId != scorecard.RunId)
            throw Conflict("Scorecard run mismatch", "Scorecard runId does not match the committed reveal.");
        bool originalModel = scorecard.Correction is null &&
            string.Equals(scorecard.ScoringModelVersion, ScenarioModelVersions.Scoring, StringComparison.Ordinal);
        bool correctionModel = scorecard.Correction is { } correction &&
            correction.CorrectionVersion == ScoringCorrectionVersions.Correction &&
            correction.OriginalScoringModelVersion == state.ScoringModelVersion &&
            scorecard.ScoringModelVersion == ScoringCorrectionVersions.ScoringModel;
        if (state.ScoringModelVersion != ScenarioModelVersions.Scoring || !(originalModel || correctionModel))
        {
            throw Conflict("Scoring model mismatch", "Scorecard scoringModelVersion does not match the frozen scenario model.");
        }
        if (scorecard.CreatedValidTimeUtc != state.AsOfUtc)
            throw Conflict("Scorecard valid time mismatch", "createdValidTimeUtc must equal the current revealed scenario clock.");
    }

    private static PublicScorecard ToPublicScorecard(ValidatedScorecard body, string contentSha256) => new(
        body.ScenarioId,
        body.ScorecardId,
        body.RevealId,
        body.ScoringModelVersion,
        body.InputSha256,
        body.HeadlineMetric,
        body.Metrics,
        body.CreatedValidTimeUtc,
        body.Limitation,
        contentSha256)
    {
        Correction = body.Correction
    };

    private sealed record StoredScorecard(
        PublicScorecard Scorecard,
        string CanonicalBodyJson,
        string CanonicalBodySha256);

    private sealed record ScorecardScenarioState(
        ScenarioStatus Status,
        string ScoringModelVersion,
        DateTimeOffset AsOfUtc,
        Guid? RevealId,
        Guid? RunId,
        string? RevealStatus);
}
