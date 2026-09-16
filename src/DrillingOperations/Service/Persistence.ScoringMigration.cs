using Microsoft.Data.Sqlite;
using System.Globalization;
using System.Text.Json;

namespace DrillingOperations;

public sealed partial class DrillingOperationsStore
{
    private sealed record LegacyScorecardCandidate(
        string RunId,
        string ScenarioId,
        string ScorecardId,
        string RevealId,
        string ScoringModelVersion,
        string InputSha256,
        string CanonicalInputJson,
        string HeadlineMetric,
        string CanonicalOutputJson,
        string OutputSha256,
        DateTimeOffset CreatedValidTimeUtc);

    private async Task MigrateLegacyScorecardContractAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        const string legacyDiagnostic = "ScorecardCallbackMismatch";
        const string repairedDiagnostic = "ScorecardContractRepaired";

        await using SqliteTransaction transaction = connection.BeginTransaction(deferred: false);
        var candidates = new List<LegacyScorecardCandidate>();
        await using (SqliteCommand select = Command(connection, transaction, """
            SELECT r.RunId,r.ScenarioId,c.ScorecardId,c.RevealId,c.ScoringModelVersion,
                   c.InputSha256,c.CanonicalInputJson,c.HeadlineMetric,c.CanonicalOutputJson,
                   c.OutputSha256,c.CreatedValidTimeUtc
            FROM Runs r
            JOIN RunStages s9 ON s9.RunId=r.RunId AND s9.Stage=$s9
            JOIN Scorecards c ON c.RunId=r.RunId
            JOIN ScorecardDeliveries d ON d.ScorecardId=c.ScorecardId
            WHERE r.Status=$failed AND r.CurrentStage=$s9 AND r.DiagnosticCode=$legacy
              AND s9.Status=$stageFailed AND s9.Diagnostics=$legacy
              AND d.Status='Failed'
              AND r.PublicationCount=1 AND r.ClockAdvanceCount=1
              AND NOT EXISTS(SELECT 1 FROM ScorecardReceipts sr WHERE sr.ScorecardId=c.ScorecardId)
            ORDER BY r.RunId;
            """))
        {
            Add(select, "$s9", (int)RunStageKind.S9Score);
            Add(select, "$failed", RunStatus.Failed.ToString());
            Add(select, "$legacy", legacyDiagnostic);
            Add(select, "$stageFailed", StageStatus.Failed.ToString());
            await using SqliteDataReader reader = await select.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                candidates.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
                    reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetString(7), reader.GetString(8),
                    reader.GetString(9), Parse(reader.GetString(10))));
        }

        foreach (LegacyScorecardCandidate candidate in candidates)
        {
            if (await HasActiveRunAsync(connection, transaction, candidate.ScenarioId, cancellationToken)) continue;
            IReadOnlyList<ScorecardMetric>? repaired = await TryBuildLegacyScorecardRepairAsync(connection, transaction, candidate, cancellationToken);
            if (repaired is null) continue;

            AnalysisScorecardRequest prior = JsonSerializer.Deserialize<AnalysisScorecardRequest>(candidate.CanonicalOutputJson, CanonicalJson.SerializerOptions)!;
            AnalysisScorecardRequest request = prior with { Metrics = repaired };
            string outputJson = CanonicalJson.Serialize(request);
            string outputHash = DeterministicIdentity.Sha256(outputJson);
            string stageOutput = CanonicalJson.Serialize(new
            {
                candidate.ScorecardId,
                candidate.ScoringModelVersion,
                candidate.InputSha256,
                OutputSha256 = outputHash,
                candidate.HeadlineMetric,
                metricCount = repaired.Count,
                status = "PendingCallback"
            });
            DateTimeOffset now = timeProvider.GetUtcNow();

            await ExecuteAsync(connection, transaction, "DROP TRIGGER TR_Scorecards_NoUpdate;DROP TRIGGER TR_ScorecardMetrics_NoUpdate;DROP TRIGGER TR_ScorecardMetrics_NoDelete;", cancellationToken);
            await using (SqliteCommand delete = Command(connection, transaction, "DELETE FROM ScorecardMetrics WHERE ScorecardId=$id;"))
            {
                Add(delete, "$id", candidate.ScorecardId);
                await delete.ExecuteNonQueryAsync(cancellationToken);
            }
            for (int sequence = 0; sequence < repaired.Count; sequence++)
                await InsertScorecardMetricAsync(connection, transaction, candidate.ScorecardId, sequence, repaired[sequence], cancellationToken);
            await using (SqliteCommand scorecard = Command(connection, transaction, "UPDATE Scorecards SET CanonicalOutputJson=$json,OutputSha256=$hash WHERE ScorecardId=$id;"))
            {
                Add(scorecard, "$json", outputJson); Add(scorecard, "$hash", outputHash); Add(scorecard, "$id", candidate.ScorecardId);
                if (await scorecard.ExecuteNonQueryAsync(cancellationToken) != 1) throw new PersistenceIntegrityException("Legacy scorecard repair lost its scorecard target.");
            }
            await ExecuteAsync(connection, transaction, """
                CREATE TRIGGER TR_Scorecards_NoUpdate BEFORE UPDATE ON Scorecards BEGIN SELECT RAISE(ABORT,'Scorecards are immutable');END;
                CREATE TRIGGER TR_ScorecardMetrics_NoUpdate BEFORE UPDATE ON ScorecardMetrics BEGIN SELECT RAISE(ABORT,'Scorecard metrics are immutable');END;
                CREATE TRIGGER TR_ScorecardMetrics_NoDelete BEFORE DELETE ON ScorecardMetrics BEGIN SELECT RAISE(ABORT,'Scorecard metrics are immutable');END;
                """, cancellationToken);

            await using (SqliteCommand delivery = Command(connection, transaction, "UPDATE ScorecardDeliveries SET Status='AwaitingDependency',Diagnostic=$diagnostic,UpdatedUtc=$now WHERE ScorecardId=$id AND Status='Failed' AND NOT EXISTS(SELECT 1 FROM ScorecardReceipts WHERE ScorecardId=$id);"))
            {
                Add(delivery, "$diagnostic", repairedDiagnostic); Add(delivery, "$now", Format(now)); Add(delivery, "$id", candidate.ScorecardId);
                if (await delivery.ExecuteNonQueryAsync(cancellationToken) != 1) throw new PersistenceIntegrityException("Legacy scorecard repair lost its delivery target.");
            }
            await using (SqliteCommand run = Command(connection, transaction, "UPDATE Runs SET Status=$status,UpdatedUtc=$now,EndedUtc=NULL,DiagnosticCode=$diagnostic WHERE RunId=$run AND Status=$failed AND CurrentStage=$s9 AND DiagnosticCode=$legacy AND PublicationCount=1 AND ClockAdvanceCount=1;"))
            {
                Add(run, "$status", RunStatus.AwaitingDependency.ToString()); Add(run, "$now", Format(now)); Add(run, "$diagnostic", repairedDiagnostic);
                Add(run, "$run", candidate.RunId); Add(run, "$failed", RunStatus.Failed.ToString()); Add(run, "$s9", (int)RunStageKind.S9Score); Add(run, "$legacy", legacyDiagnostic);
                if (await run.ExecuteNonQueryAsync(cancellationToken) != 1) throw new PersistenceIntegrityException("Legacy scorecard repair lost its run target.");
            }
            await using (SqliteCommand stage = Command(connection, transaction, "UPDATE RunStages SET Status=$status,OutputJson=$output,OutputHash=$hash,EndedUtc=NULL,Diagnostics=$diagnostic WHERE RunId=$run AND Stage=$s9 AND Status=$failed AND Diagnostics=$legacy;"))
            {
                Add(stage, "$status", StageStatus.AwaitingDependency.ToString()); Add(stage, "$output", stageOutput); Add(stage, "$hash", outputHash);
                Add(stage, "$diagnostic", repairedDiagnostic); Add(stage, "$run", candidate.RunId); Add(stage, "$s9", (int)RunStageKind.S9Score);
                Add(stage, "$failed", StageStatus.Failed.ToString()); Add(stage, "$legacy", legacyDiagnostic);
                if (await stage.ExecuteNonQueryAsync(cancellationToken) != 1) throw new PersistenceIntegrityException("Legacy scorecard repair lost its S9 target.");
            }
            await AppendAuditAsync(connection, transaction, candidate.ScenarioId, "scorecard.contract-repaired", candidate.ScorecardId,
                CanonicalJson.Serialize(new { fromDiagnostic = legacyDiagnostic, toDiagnostic = repairedDiagnostic, outputSha256 = outputHash, metricCount = repaired.Count }), cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
    }

    private static async Task<IReadOnlyList<ScorecardMetric>?> TryBuildLegacyScorecardRepairAsync(
        SqliteConnection connection, SqliteTransaction transaction, LegacyScorecardCandidate candidate, CancellationToken cancellationToken)
    {
        if (candidate.ScoringModelVersion != ScoreArtifactIntegrity.ModelVersion ||
            DeterministicIdentity.Sha256(candidate.CanonicalInputJson) != candidate.InputSha256 ||
            DeterministicIdentity.Sha256(candidate.CanonicalOutputJson) != candidate.OutputSha256 ||
            candidate.ScorecardId != DeterministicIdentity.Create("scorecard-v1", candidate.RunId, candidate.RevealId, candidate.ScoringModelVersion, candidate.InputSha256)) return null;

        AnalysisScorecardRequest request;
        try { request = JsonSerializer.Deserialize<AnalysisScorecardRequest>(candidate.CanonicalOutputJson, CanonicalJson.SerializerOptions) ?? throw new JsonException(); }
        catch (JsonException) { return null; }
        if (request.ScorecardId != candidate.ScorecardId || request.RunId != candidate.RunId || request.RevealId != candidate.RevealId ||
            request.ScoringModelVersion != candidate.ScoringModelVersion || request.InputSha256 != candidate.InputSha256 ||
            request.HeadlineMetric != candidate.HeadlineMetric || request.CreatedValidTimeUtc != candidate.CreatedValidTimeUtc) return null;

        var metrics = new List<ScorecardMetric>();
        await using (SqliteCommand rows = Command(connection, transaction, "SELECT Sequence,Name,Basis,Status,Value,Unit,LowerBound,UpperBound,Limitation,CanonicalJson,ContentSha256 FROM ScorecardMetrics WHERE ScorecardId=$id ORDER BY Sequence;"))
        {
            Add(rows, "$id", candidate.ScorecardId);
            await using SqliteDataReader reader = await rows.ExecuteReaderAsync(cancellationToken);
            int sequence = 0;
            while (await reader.ReadAsync(cancellationToken))
            {
                string json = reader.GetString(9);
                ScorecardMetric? metric;
                try { metric = JsonSerializer.Deserialize<ScorecardMetric>(json, CanonicalJson.SerializerOptions); }
                catch (JsonException) { return null; }
                if (metric is null || reader.GetInt32(0) != sequence++ || DeterministicIdentity.Sha256(json) != reader.GetString(10) || CanonicalJson.Serialize(metric) != json ||
                    metric.Name != reader.GetString(1) || metric.Basis.ToString() != reader.GetString(2) || metric.Status.ToString() != reader.GetString(3) ||
                    !NullableDoubleEquals(metric.Value, reader, 4) || !NullableStringEquals(metric.Unit, reader, 5) ||
                    !NullableDoubleEquals(metric.LowerBound, reader, 6) || !NullableDoubleEquals(metric.UpperBound, reader, 7) ||
                    !NullableStringEquals(metric.Limitation, reader, 8)) return null;
                metrics.Add(metric);
            }
        }
        if (metrics.Count != request.Metrics.Count || CanonicalJson.Serialize(metrics) != CanonicalJson.Serialize(request.Metrics) ||
            metrics.Any(x => x.Name.Contains("ScoredMonthCount", StringComparison.Ordinal))) return null;

        bool hadViolation = false;
        var counts = new Dictionary<(string Phase, ScoreMetricBasis Basis), int>();
        foreach (ScorecardMetric metric in metrics)
        {
            if (metric.Status != ScoreMetricStatus.Scored || metric.Limitation is null) continue;
            hadViolation = true;
            if (TryLegacyMonthlyMetric(metric, out string phase, out int count))
            {
                var key = (phase, metric.Basis);
                if (counts.TryGetValue(key, out int prior) && prior != count) return null;
                counts[key] = count;
            }
            else if (!metric.Name.StartsWith("expectedPaydirtP50AbsoluteError.Baseline.", StringComparison.Ordinal) &&
                     !(metric.Name == "expectedPaydirtAiMinusBestBaselineAbsoluteError.Baseline" && metric.Limitation == "Negative values indicate the approved prediction outperformed the best deterministic baseline.")) return null;
        }
        if (!hadViolation) return null;
        foreach (string phase in new[] { "Oil", "Gas", "Water" })
        {
            var key = (phase, ScoreMetricBasis.RevealedObservation);
            ScorecardMetric? rmse = metrics.SingleOrDefault(x => x.Name == $"monthly{phase}Rmse.RevealedObservation");
            if (!counts.ContainsKey(key) && rmse is { Status: ScoreMetricStatus.Unavailable, Limitation: "No corrected monthly values were available." }) counts[key] = 0;
        }
        foreach (string phase in new[] { "Oil", "Gas", "Water" })
        foreach (ScoreMetricBasis basis in new[] { ScoreMetricBasis.HiddenTruth, ScoreMetricBasis.RevealedObservation })
            if (!counts.ContainsKey((phase, basis))) return null;

        var repaired = new List<ScorecardMetric>(metrics.Count + 6);
        foreach (ScorecardMetric metric in metrics)
        {
            repaired.Add(metric.Status == ScoreMetricStatus.Scored ? metric with { Limitation = null } : metric);
            foreach (string phase in new[] { "Oil", "Gas", "Water" })
            foreach (ScoreMetricBasis basis in new[] { ScoreMetricBasis.HiddenTruth, ScoreMetricBasis.RevealedObservation })
                if (metric.Name == $"monthly{phase}Nrmse.{basis}") repaired.Add(new($"monthly{phase}ScoredMonthCount.{basis}", basis, ScoreMetricStatus.Scored, counts[(phase, basis)], "count", null, null, null));
        }
        if (repaired.Count != metrics.Count + 6) return null;
        try { ScoreMetricContract.Validate(repaired); }
        catch (ArgumentException) { return null; }
        return repaired;
    }

    private static bool TryLegacyMonthlyMetric(ScorecardMetric metric, out string phase, out int count)
    {
        phase = string.Empty; count = 0;
        foreach (string candidate in new[] { "Oil", "Gas", "Water" })
        {
            bool named = metric.Name is not null && (metric.Name.StartsWith("monthly" + candidate, StringComparison.Ordinal) || metric.Name.StartsWith("initial" + candidate, StringComparison.Ordinal));
            if (!named) continue;
            string suffix = metric.Basis == ScoreMetricBasis.HiddenTruth
                ? " of 60 months; prediction checkpoints are linearly interpolated to monthly volumes."
                : metric.Basis == ScoreMetricBasis.RevealedObservation
                    ? " of 60 months; null corrected months were excluded and prediction checkpoints were linearly interpolated."
                    : string.Empty;
            const string prefix = "Scored ";
            if (suffix.Length == 0 || metric.Limitation is null || !metric.Limitation.StartsWith(prefix, StringComparison.Ordinal) || !metric.Limitation.EndsWith(suffix, StringComparison.Ordinal)) return false;
            string number = metric.Limitation[prefix.Length..^suffix.Length];
            if (!int.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out count) || count is < 0 or > 60) return false;
            if (metric.Basis == ScoreMetricBasis.HiddenTruth && count != 60) return false;
            phase = candidate;
            return true;
        }
        return false;
    }

    private static bool NullableDoubleEquals(double? value, SqliteDataReader reader, int ordinal) =>
        value is null ? reader.IsDBNull(ordinal) : !reader.IsDBNull(ordinal) && value.Value.Equals(reader.GetDouble(ordinal));
    private static bool NullableStringEquals(string? value, SqliteDataReader reader, int ordinal) =>
        value is null ? reader.IsDBNull(ordinal) : !reader.IsDBNull(ordinal) && value == reader.GetString(ordinal);

    private static async Task InsertScorecardMetricAsync(SqliteConnection connection, SqliteTransaction transaction, string scorecardId, int sequence, ScorecardMetric metric, CancellationToken cancellationToken)
    {
        string json = CanonicalJson.Serialize(metric);
        await using SqliteCommand insert = Command(connection, transaction, "INSERT INTO ScorecardMetrics(ScorecardId,Sequence,Name,Basis,Status,Value,Unit,LowerBound,UpperBound,Limitation,CanonicalJson,ContentSha256)VALUES($card,$sequence,$name,$basis,$status,$value,$unit,$lower,$upper,$limitation,$json,$hash);");
        Add(insert, "$card", scorecardId); Add(insert, "$sequence", sequence); Add(insert, "$name", metric.Name); Add(insert, "$basis", metric.Basis.ToString()); Add(insert, "$status", metric.Status.ToString());
        Add(insert, "$value", metric.Value ?? (object)DBNull.Value); Add(insert, "$unit", metric.Unit ?? (object)DBNull.Value); Add(insert, "$lower", metric.LowerBound ?? (object)DBNull.Value);
        Add(insert, "$upper", metric.UpperBound ?? (object)DBNull.Value); Add(insert, "$limitation", metric.Limitation ?? (object)DBNull.Value); Add(insert, "$json", json); Add(insert, "$hash", DeterministicIdentity.Sha256(json));
        await insert.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task ExecuteAsync(SqliteConnection connection, SqliteTransaction transaction, string sql, CancellationToken cancellationToken)
    {
        await using SqliteCommand command = Command(connection, transaction, sql);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}

