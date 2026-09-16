using System.Globalization;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace DrillingOperations;

public sealed partial class DrillingOperationsStore
{
    public async Task<ScorecardDraft?> GetScorecardArtifactAsync(string runId, string scorecardId, CancellationToken ct = default)
    {
        await using var c = await OpenAsync(ct);
        ScorecardDraft? original = await ReadScorecardAsync(c, null, runId, ct);
        if (original?.ScorecardId == scorecardId) return original;
        ScorecardDraft? correction = await ReadScoreCorrectionAsync(c, null, runId, ct);
        return correction?.ScorecardId == scorecardId ? correction : null;
    }

    internal async Task<ScoringCorrectionSnapshot> CaptureScoringCorrectionAsync(string scenarioId, string runId, CancellationToken ct)
    {
        await using var c = await OpenAsync(ct);
        await using var tx = c.BeginTransaction(deferred: true);
        var snapshot = await CaptureScoringCorrectionAsync(c, tx, scenarioId, runId, ct);
        await tx.CommitAsync(ct);
        return snapshot;
    }

    internal async Task RecheckScoringCorrectionAsync(ScoringCorrectionSnapshot expected, CancellationToken ct)
    {
        var actual = await CaptureScoringCorrectionAsync(expected.Run.ScenarioId, expected.Run.RunId, ct);
        ScoreCorrection.Require(actual.GuardHash == expected.GuardHash, "ScoringCorrectionReviewStale");
    }

    private async Task<ScoringCorrectionSnapshot> CaptureScoringCorrectionAsync(
        SqliteConnection c, SqliteTransaction tx, string scenarioId, string runId, CancellationToken ct)
    {
        RunResponse? run = await ReadRunAsync(c, tx, runId, ct);
        if (run is null || run.ScenarioId != scenarioId) throw new ScoringCorrectionException(404, "ScoringCorrectionRunNotOwned");
        ScoreCorrection.Require(run.Status == RunStatus.Failed && run.CurrentStage == RunStageKind.S9Score,
            "ScoringCorrectionWrongPhase");
        ScoreCorrection.Require(!await HasActiveRunAsync(c, tx, scenarioId, ct), "ScoringCorrectionOtherActiveRun");
        var captured = new SortedDictionary<string, IReadOnlyList<Dictionary<string, string?>>>(StringComparer.Ordinal);
        async Task<IReadOnlyList<Dictionary<string, string?>>> Rows(string name, string sql)
        {
            var rows = new List<Dictionary<string, string?>>();
            await using var q = Command(c, tx, sql);
            Add(q, "$run", runId); Add(q, "$scenario", scenarioId);
            await using var reader = await q.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                ScoreCorrection.Require(rows.Count < 20000, "ScoringCorrectionArtifactBoundExceeded");
                var row = new Dictionary<string, string?>(StringComparer.Ordinal);
                for (int i = 0; i < reader.FieldCount; i++)
                    row.Add(reader.GetName(i), reader.IsDBNull(i) ? null : Convert.ToString(reader.GetValue(i), CultureInfo.InvariantCulture));
                rows.Add(row);
            }
            captured.Add(name, rows);
            return rows;
        }
        static Dictionary<string, string?> One(IReadOnlyList<Dictionary<string, string?>> rows)
        {
            ScoreCorrection.Require(rows.Count == 1, "ScoringCorrectionArtifactMissing");
            return rows[0];
        }
        var runRow = One(await Rows("run", "SELECT * FROM Runs WHERE RunId=$run;"));
        ScoreCorrection.Require(runRow["DiagnosticCode"] == "ScorecardCallbackMismatch" &&
            runRow["PublicationCount"] == "1" && runRow["ClockAdvanceCount"] == "1", "ScoringCorrectionFailureOrRevealCounts");
        var stages = await Rows("stages", "SELECT * FROM RunStages WHERE RunId=$run ORDER BY Stage;");
        ScoreCorrection.Require(stages.Count == 10, "ScoringCorrectionCheckpointMissing");
        for (int i = 0; i < 9; i++)
        {
            ScoreCorrection.Require(stages[i]["Stage"] == i.ToString(CultureInfo.InvariantCulture) &&
                stages[i]["Status"] == "Completed", "ScoringCorrectionCheckpointIncomplete");
            CheckHash(stages[i], "InputJson", "InputHash");
            CheckHash(stages[i], "OutputJson", "OutputHash");
        }
        ScoreCorrection.Require(stages[9]["Stage"] == "9" && stages[9]["Status"] == "Failed" &&
            stages[9]["Diagnostics"] == "ScorecardCallbackMismatch", "ScoringCorrectionWrongFailure");
        ScorecardDraft rejected = await ReadScorecardAsync(c, tx, runId, ct)
            ?? throw new ScoringCorrectionException(409, "ScoringCorrectionRejectedArtifactMissing");
        ScoreCorrection.Require(rejected.ScenarioId == scenarioId && rejected.ScoringModelVersion == ScoreArtifactIntegrity.ModelVersion &&
            stages[9]["InputJson"] == rejected.CanonicalInputJson && stages[9]["InputHash"] == rejected.InputSha256 &&
            stages[9]["OutputHash"] == rejected.OutputSha256, "ScoringCorrectionRejectedCheckpointMismatch");
        var card = One(await Rows("scorecard", "SELECT * FROM Scorecards WHERE RunId=$run;"));
        var metrics = await Rows("metrics", "SELECT * FROM ScorecardMetrics WHERE ScorecardId IN (SELECT ScorecardId FROM Scorecards WHERE RunId=$run) ORDER BY Sequence;");
        ScoreCorrection.Require(metrics.Count == rejected.Metrics.Count, "ScoringCorrectionMetricRowsMismatch");
        for (int i = 0; i < metrics.Count; i++)
        {
            var row = metrics[i];
            ScorecardMetric metric = rejected.Metrics[i];
            ScoreCorrection.Require(row["Name"] == metric.Name && row["Basis"] == metric.Basis.ToString() &&
                row["Status"] == metric.Status.ToString() && SameNumber(row["Value"], metric.Value) &&
                SameNumber(row["LowerBound"], metric.LowerBound) && SameNumber(row["UpperBound"], metric.UpperBound) &&
                row["Unit"] == metric.Unit && row["Limitation"] == metric.Limitation, "ScoringCorrectionMetricRowsMismatch");
        }
        var delivery = One(await Rows("delivery", "SELECT * FROM ScorecardDeliveries WHERE ScorecardId IN (SELECT ScorecardId FROM Scorecards WHERE RunId=$run);"));
        ScoreCorrection.Require(delivery["Status"] == "Failed" && delivery["Diagnostic"] == "ScorecardCallbackMismatch",
            "ScoringCorrectionWrongFailure");
        ScoreCorrection.Require((await Rows("receipts", "SELECT * FROM ScorecardReceipts WHERE ScorecardId IN (SELECT ScorecardId FROM Scorecards WHERE RunId=$run);")).Count == 0,
            "ScoringCorrectionAlreadyPublished");
        bool storageExists = await ScoreCorrectionStorageExistsAsync(c, tx, ct);
        foreach (string table in new[] { "ScorecardCorrections", "ScorecardCorrectionDeliveries", "ScorecardCorrectionReceipts" })
        {
            if (!storageExists) captured.Add(table, []);
            else await Rows(table, table == "ScorecardCorrections"
                ? "SELECT * FROM ScorecardCorrections WHERE RunId=$run;"
                : $"SELECT * FROM {table} WHERE ScorecardId IN (SELECT ScorecardId FROM ScorecardCorrections WHERE RunId=$run);");
            ScoreCorrection.Require(captured[table].Count == 0, "ScoringCorrectionAlreadyExists");
        }
        var binding = await ReadBindingAsync(c, tx, scenarioId, ct)
            ?? throw new ScoringCorrectionException(409, "ScoringCorrectionBindingMissing");
        var bindingRow = One(await Rows("binding", "SELECT * FROM TruthBindings WHERE ScenarioId=$scenario;"));
        ScoreCorrection.Require(binding.BindingId == runRow["BindingId"] && binding.ApprovedSealedPredictionHash == runRow["ApprovedPredictionHash"],
            "ScoringCorrectionBindingChanged");
        foreach (string table in new[] { "MaterializedPlans", "DrillingExecutions", "SurveyArtifacts", "TruthSampleBatches",
            "ObservationBatches", "CompletionDesigns", "ProductionTruthArtifacts", "ProductionSeries", "DrillingObservationArtifacts" })
            await Rows(table, $"SELECT * FROM {table} WHERE RunId=$run ORDER BY rowid;");
        foreach (string table in new[] { "MaterializedPlans", "DrillingExecutions", "SurveyArtifacts", "TruthSampleBatches",
            "ObservationBatches", "CompletionDesigns", "ProductionTruthArtifacts", "ProductionSeries" })
            _ = One(captured[table]);
        await Rows("completionApprovals", "SELECT * FROM CompletionApprovals WHERE CompletionDesignId IN (SELECT CompletionDesignId FROM CompletionDesigns WHERE RunId=$run);");
        _ = One(captured["completionApprovals"]);
        await Rows("logDetails", "SELECT * FROM LogObservationDetails WHERE ObservationBatchId IN (SELECT ObservationBatchId FROM ObservationBatches WHERE RunId=$run);");
        await Rows("drillingDetails", "SELECT * FROM DrillingObservationDetails WHERE ArtifactId IN (SELECT ArtifactId FROM DrillingObservationArtifacts WHERE RunId=$run);");
        await Rows("drillingSeries", "SELECT * FROM DrillingObservationSeries WHERE ArtifactId IN (SELECT ArtifactId FROM DrillingObservationArtifacts WHERE RunId=$run);");
        foreach (var row in captured.Values.SelectMany(x => x))
            foreach (var pair in RecoveryHashColumns)
                if (row.ContainsKey(pair.Json) && row.ContainsKey(pair.Hash)) CheckHash(row, pair.Json, pair.Hash);
        string s0 = CanonicalJson.Serialize(new { binding.BindingId, bindingInputHash = bindingRow["InputHash"] });
        ScoreCorrection.Require(stages[0]["InputJson"] == s0 && stages[0]["OutputJson"] == s0, "ScoringCorrectionBindingChanged");
        PublicationStaging publication = await ReadPublicationAsync(c, tx, runId, ct)
            ?? throw new ScoringCorrectionException(409, "ScoringCorrectionRevealMissing");
        PublicationJson.ValidateStagedPublication(publication);
        var publicationRow = One(await Rows("publicationPlan", "SELECT * FROM PublicationPlans WHERE RunId=$run;"));
        var publicationState = One(await Rows("publicationState", "SELECT * FROM PublicationStates WHERE PublicationPlanId IN (SELECT PublicationPlanId FROM PublicationPlans WHERE RunId=$run);"));
        var reveal = One(await Rows("reveal", "SELECT * FROM RevealManifests WHERE RunId=$run;"));
        await Rows("operations", "SELECT * FROM PublicationOperations WHERE PublicationPlanId IN (SELECT PublicationPlanId FROM PublicationPlans WHERE RunId=$run) ORDER BY Sequence;");
        var operationStates = await Rows("operationStates", "SELECT s.* FROM PublicationOperationStates s JOIN PublicationOperations o ON o.OperationId=s.OperationId WHERE o.PublicationPlanId IN (SELECT PublicationPlanId FROM PublicationPlans WHERE RunId=$run) ORDER BY o.Sequence;");
        ScoreCorrection.Require(publication.ScenarioId == scenarioId && publication.RunId == runId &&
            publication.RevealId == rejected.RevealId && publication.ValidTimeUtc == rejected.CreatedValidTimeUtc &&
            publicationState["Status"] == "Revealed" && reveal["Status"] == "Published" && reveal["PublishedUtc"] is not null &&
            operationStates.Count == publication.Operations.Count &&
            operationStates.All(x => x["Status"] == "Verified" && x["ActivationStatus"] == "Activated"),
            "ScoringCorrectionRevealChanged");
        CheckHash(publicationRow, "ManifestJson", "ManifestHash");
        CheckHash(publicationState, "FinalManifestJson", "FinalManifestHash");
        CheckHash(publicationState, "PreparedReceiptJson", "PreparedReceiptHash");
        CheckHash(publicationState, "FinalReceiptJson", "FinalReceiptHash");
        CheckHash(publicationState, "ReceiptJson", "ReceiptHash");
        ScoreCorrection.Require(publicationState["FinalManifestHash"] == publication.ManifestHash &&
            reveal["ManifestHash"] == publication.ManifestHash && reveal["CanonicalManifestJson"] == publication.ManifestJson &&
            publicationState["ReceiptJson"] == publicationState["FinalReceiptJson"], "ScoringCorrectionRevealReceiptChanged");
        string receiptJson = publicationState["FinalReceiptJson"]!;
        var receipt = JsonSerializer.Deserialize<AnalysisRevealReceipt>(receiptJson, CanonicalJson.SerializerOptions)!;
        ScoreCorrection.Require(receipt.Status == "Revealed" && receipt.ScenarioId.ToString("D") == scenarioId &&
            receipt.RevealId.ToString("D") == publication.RevealId && receipt.ClonedFieldId.ToString("D") == publication.ClonedFieldId &&
            receipt.ManifestSha256 == publication.ManifestHash && receipt.AsOfUtc == publication.ValidTimeUtc &&
            receipt.EvidenceCount == publication.Operations.Count && receipt.ProductionSeriesId.ToString("D") == publication.ProductionSeriesId,
            "ScoringCorrectionRevealReceiptChanged");
        var audits = await Rows("audit", "SELECT * FROM AuditEntries WHERE ScenarioId=$scenario ORDER BY Sequence;");
        string previous = EmptyHash;
        for (int i = 0; i < audits.Count; i++)
        {
            var audit = audits[i];
            CheckHash(audit, "DataJson", "DataHash");
            ScoreCorrection.Require(audit["Sequence"] == (i + 1).ToString(CultureInfo.InvariantCulture) &&
                audit["PreviousHash"] == previous && audit["EntryHash"] == AuditHash(scenarioId, i + 1,
                    audit["Action"]!, audit["SubjectId"]!, audit["DataHash"]!, previous, audit["CreatedUtc"]!),
                "ScoringCorrectionAuditMismatch");
            previous = audit["EntryHash"]!;
        }
        ScoreCorrection.Require(audits.Any(x => x["Action"] == "scorecard.calculated" && x["SubjectId"] == rejected.ScorecardId &&
            RecoveryProperty(x["DataJson"], "inputSha256") == rejected.InputSha256 &&
            RecoveryProperty(x["DataJson"], "outputSha256") == rejected.OutputSha256), "ScoringCorrectionCalculationAuditMismatch");
        ScoreCorrection.Require(card["CanonicalInputJson"] == rejected.CanonicalInputJson && card["CanonicalOutputJson"] == rejected.CanonicalOutputJson,
            "ScoringCorrectionRejectedCheckpointMismatch");
        return new(run, rejected, publication, receiptJson, DeterministicIdentity.Sha256(CanonicalJson.Serialize(captured)));

        static bool SameNumber(string? stored, double? value) => stored is null ? value is null :
            value is not null && double.TryParse(stored, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed) && parsed == value;
        static void CheckHash(Dictionary<string, string?> row, string json, string hash) =>
            ScoreCorrection.Require(row.TryGetValue(json, out string? value) && value is not null &&
                row.TryGetValue(hash, out string? expected) && DeterministicIdentity.Sha256(value) == expected,
                "ScoringCorrectionIntegrityMismatch");
    }

    internal Task<ApiOutcome> CorrectScoreAsync(
        string route, string key, string canonical, CorrectScoreRequest request, ScoringCorrectionCandidate candidate, CancellationToken ct) =>
        ExecuteIdempotentAsync(route, key, canonical, async (c, tx, token) =>
        {
            var actual = await CaptureScoringCorrectionAsync(c, tx, candidate.Snapshot.Run.ScenarioId, candidate.Snapshot.Run.RunId, token);
            ScoreCorrection.Require(actual.GuardHash == candidate.Snapshot.GuardHash, "ScoringCorrectionReviewStale");
            ScoreCorrection.Require(await ScoreCorrectionStorageExistsAsync(c, tx, token), "ScoringCorrectionStorageUnavailable");
            ScorecardDraft corrected = candidate.Corrected, rejected = actual.Rejected;
            ValidateCorrection(corrected, rejected);
            string now = NowText();
            await using (var q = Command(c, tx, """
                INSERT INTO ScorecardCorrections(ScorecardId,RunId,ScenarioId,RevealId,OriginalScorecardId,
                    OriginalInputSha256,OriginalOutputSha256,CorrectionVersion,ScoringModelVersion,
                    CanonicalInputJson,InputSha256,CanonicalOutputJson,OutputSha256,CreatedValidTimeUtc,CreatedUtc)
                VALUES($id,$run,$scenario,$reveal,$old,$oldInput,$oldOutput,$version,$model,$input,$inputHash,$output,$outputHash,$valid,$now);
                INSERT INTO ScorecardCorrectionDeliveries(ScorecardId,Status,AttemptCount,Diagnostic,UpdatedUtc)
                VALUES($id,'Pending',0,NULL,$now);
                """))
            {
                Add(q, "$id", corrected.ScorecardId); Add(q, "$run", corrected.RunId); Add(q, "$scenario", corrected.ScenarioId);
                Add(q, "$reveal", corrected.RevealId); Add(q, "$old", rejected.ScorecardId);
                Add(q, "$oldInput", rejected.InputSha256); Add(q, "$oldOutput", rejected.OutputSha256);
                Add(q, "$version", ScoreCorrection.Version); Add(q, "$model", corrected.ScoringModelVersion);
                Add(q, "$input", corrected.CanonicalInputJson); Add(q, "$inputHash", corrected.InputSha256);
                Add(q, "$output", corrected.CanonicalOutputJson); Add(q, "$outputHash", corrected.OutputSha256);
                Add(q, "$valid", Format(corrected.CreatedValidTimeUtc)); Add(q, "$now", now);
                await q.ExecuteNonQueryAsync(token);
            }
            var audit = await AppendAuditAsync(c, tx, corrected.ScenarioId, "scorecard.correction-approved", corrected.ScorecardId,
                CanonicalJson.Serialize(new
                {
                    request.Actor, request.Reason, request.ReviewedCorrectionHash,
                    previousStatus = "Failed", status = "AwaitingDependency", stage = "S9Score",
                    rejectedScorecardId = rejected.ScorecardId, rejectedScorecardSha256 = rejected.OutputSha256,
                    scoringInputSha256 = rejected.InputSha256, correctedScorecardId = corrected.ScorecardId,
                    correctedScorecardSha256 = corrected.OutputSha256, correctedInputSha256 = corrected.InputSha256,
                    correctionVersion = ScoreCorrection.Version
                }), token);
            string stageOutput = CorrectionStageOutput(corrected, "PendingCallback", null);
            await using (var stage = Command(c, tx, """
                UPDATE RunStages SET InputJson=$input,InputHash=$inputHash,OutputJson=$output,OutputHash=$outputHash,
                    Status='AwaitingDependency',EndedUtc=NULL,Diagnostics='ScorecardCorrectionApproved',PreviousAuditHash=$previous
                WHERE RunId=$run AND Stage=9 AND Status='Failed' AND Diagnostics='ScorecardCallbackMismatch';
                """))
            {
                Add(stage, "$input", corrected.CanonicalInputJson); Add(stage, "$inputHash", corrected.InputSha256);
                Add(stage, "$output", stageOutput); Add(stage, "$outputHash", DeterministicIdentity.Sha256(stageOutput));
                Add(stage, "$previous", audit.PreviousHash); Add(stage, "$run", corrected.RunId);
                ScoreCorrection.Require(await stage.ExecuteNonQueryAsync(token) == 1, "ScoringCorrectionRace");
            }
            await UpdateRunAsync(c, tx, corrected.RunId, RunStatus.AwaitingDependency, RunStageKind.S9Score,
                timeProvider.GetUtcNow(), "ScorecardCorrectionApproved", null, token);
            await using var auditId = Command(c, tx, "SELECT AuditId FROM AuditEntries WHERE ScenarioId=$scenario AND EntryHash=$hash;");
            Add(auditId, "$scenario", corrected.ScenarioId); Add(auditId, "$hash", audit.EntryHash);
            return Json(200, new ScoringCorrectionResult(corrected.ScenarioId, corrected.RunId, "score-corrected", "AwaitingDependency",
                rejected.ScorecardId, rejected.OutputSha256, rejected.InputSha256, corrected.ScorecardId,
                corrected.OutputSha256, corrected.InputSha256, ScoreCorrection.Version, request.ReviewedCorrectionHash,
                (string)(await auditId.ExecuteScalarAsync(token))!));
        }, ct);

    private static void ValidateCorrection(ScorecardDraft corrected, ScorecardDraft original)
    {
        try { ScoreMetricContract.ValidateForPublication(corrected.Metrics); }
        catch (ArgumentException e) { throw new PersistenceIntegrityException("Corrected scorecard metric contract failure: " + e.Message); }
        var body = JsonSerializer.Deserialize<AnalysisScorecardRequest>(corrected.CanonicalOutputJson, CanonicalJson.SerializerOptions)
            ?? throw new PersistenceIntegrityException("Corrected scorecard output missing.");
        ScoreCorrectionProvenance expected = new(ScoreCorrection.Version, original.ScoringModelVersion,
            original.ScorecardId, original.OutputSha256, original.InputSha256);
        string input = ScoreCorrection.InputJson(original.RunId, original.ScenarioId, original.RevealId, expected);
        if (corrected.CanonicalInputJson != input || DeterministicIdentity.Sha256(input) != corrected.InputSha256 ||
            corrected.ScorecardId != ScoreCorrection.Identity(original.RunId, expected, corrected.InputSha256) ||
            corrected.ScoringModelVersion != ScoreCorrection.ModelVersion || corrected.ScenarioId != original.ScenarioId ||
            corrected.RunId != original.RunId || corrected.RevealId != original.RevealId ||
            corrected.CreatedValidTimeUtc != original.CreatedValidTimeUtc || corrected.HeadlineMetric != original.HeadlineMetric ||
            body.Correction != expected || body.ScorecardId != corrected.ScorecardId || body.RunId != corrected.RunId ||
            body.RevealId != corrected.RevealId || body.ScoringModelVersion != corrected.ScoringModelVersion ||
            body.InputSha256 != corrected.InputSha256 || body.HeadlineMetric != corrected.HeadlineMetric ||
            body.CreatedValidTimeUtc != corrected.CreatedValidTimeUtc ||
            CanonicalJson.Serialize(body) != corrected.CanonicalOutputJson ||
            DeterministicIdentity.Sha256(corrected.CanonicalOutputJson) != corrected.OutputSha256 ||
            CanonicalJson.Serialize(body.Metrics) != CanonicalJson.Serialize(corrected.Metrics) ||
            original.Metrics.Count != corrected.Metrics.Count)
            throw new PersistenceIntegrityException("Corrected scorecard provenance integrity failure.");
        for (int i = 0; i < original.Metrics.Count; i++)
        {
            var old = original.Metrics[i]; var current = corrected.Metrics[i];
            if (CanonicalJson.Serialize(old) == CanonicalJson.Serialize(current)) continue;
            if (!(old.LowerBound > 0) || current.LowerBound != 0 || old.Status != ScoreMetricStatus.Scored ||
                old.Unit != "m" || !old.Name.Contains("P50AbsoluteError.", StringComparison.Ordinal) ||
                CanonicalJson.Serialize(old with { LowerBound = 0 }) != CanonicalJson.Serialize(current))
                throw new PersistenceIntegrityException("Correction changed a metric beyond the approved bounds-only scope.");
        }
    }

    private static async Task<bool> ScoreCorrectionStorageExistsAsync(SqliteConnection c, SqliteTransaction? tx, CancellationToken ct)
    {
        await using var q = Command(c, tx, "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('ScorecardCorrections','ScorecardCorrectionDeliveries','ScorecardCorrectionReceipts');");
        int count = Convert.ToInt32(await q.ExecuteScalarAsync(ct), CultureInfo.InvariantCulture);
        if (count is not (0 or 3)) throw new PersistenceIntegrityException("Scoring correction schema is incomplete.");
        return count == 3;
    }
    private async Task<bool> HasScoreCorrectionAsync(string runId, CancellationToken ct)
    {
        await using var c = await OpenAsync(ct);
        return await ReadScoreCorrectionAsync(c, null, runId, ct) is not null;
    }
    private static async Task<ScorecardDraft?> ReadScoreCorrectionAsync(SqliteConnection c, SqliteTransaction? tx, string runId, CancellationToken ct)
    {
        if (!await ScoreCorrectionStorageExistsAsync(c, tx, ct)) return null;
        ScorecardDraft corrected;
        string originalId, originalInput, originalOutput, version;
        await using (var q = Command(c, tx, "SELECT * FROM ScorecardCorrections WHERE RunId=$run;"))
        {
            Add(q, "$run", runId); await using var r = await q.ExecuteReaderAsync(ct);
            if (!await r.ReadAsync(ct)) return null;
            var request = JsonSerializer.Deserialize<AnalysisScorecardRequest>(r.GetString(r.GetOrdinal("CanonicalOutputJson")), CanonicalJson.SerializerOptions)
                ?? throw new PersistenceIntegrityException("Corrected scorecard output invalid.");
            string S(string name) => r.GetString(r.GetOrdinal(name));
            corrected = new(S("ScorecardId"), S("RunId"), S("ScenarioId"), S("RevealId"), S("ScoringModelVersion"),
                S("InputSha256"), S("CanonicalInputJson"), S("CanonicalOutputJson"), S("OutputSha256"),
                request.HeadlineMetric, request.Metrics, Parse(S("CreatedValidTimeUtc")));
            originalId = S("OriginalScorecardId"); originalInput = S("OriginalInputSha256"); originalOutput = S("OriginalOutputSha256"); version = S("CorrectionVersion");
        }
        var original = await ReadScorecardAsync(c, tx, runId, ct)
            ?? throw new PersistenceIntegrityException("Rejected scorecard referenced by correction is missing.");
        if (originalId != original.ScorecardId || originalInput != original.InputSha256 || originalOutput != original.OutputSha256 ||
            version != ScoreCorrection.Version) throw new PersistenceIntegrityException("Correction superseded artifact changed.");
        ValidateCorrection(corrected, original);
        await using (var q = Command(c, tx, "SELECT DataJson,DataHash FROM AuditEntries WHERE ScenarioId=$scenario AND Action='scorecard.correction-approved' AND SubjectId=$id;"))
        {
            Add(q, "$scenario", corrected.ScenarioId); Add(q, "$id", corrected.ScorecardId);
            await using var r = await q.ExecuteReaderAsync(ct);
            if (!await r.ReadAsync(ct)) throw new PersistenceIntegrityException("Correction approval audit missing.");
            string json = r.GetString(0);
            if (DeterministicIdentity.Sha256(json) != r.GetString(1) ||
                RecoveryProperty(json, "correctedScorecardSha256") != corrected.OutputSha256 ||
                RecoveryProperty(json, "correctedInputSha256") != corrected.InputSha256 ||
                RecoveryProperty(json, "rejectedScorecardSha256") != original.OutputSha256 ||
                RecoveryProperty(json, "scoringInputSha256") != original.InputSha256 ||
                await r.ReadAsync(ct))
                throw new PersistenceIntegrityException("Correction approval commitments changed.");
        }
        return corrected;
    }
    private static async Task<string?> ReadScoreCorrectionReceiptAsync(SqliteConnection c, SqliteTransaction? tx, string scorecardId, CancellationToken ct)
    {
        if (!await ScoreCorrectionStorageExistsAsync(c, tx, ct)) return null;
        await using var q = Command(c, tx, "SELECT CanonicalReceiptJson,ReceiptHash FROM ScorecardCorrectionReceipts WHERE ScorecardId=$id;");
        Add(q, "$id", scorecardId); await using var r = await q.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct)) return null;
        string json = r.GetString(0);
        if (DeterministicIdentity.Sha256(json) != r.GetString(1)) throw new PersistenceIntegrityException("Correction receipt integrity failure.");
        return json;
    }
    private async Task<ScorecardSummary?> GetCorrectedScorecardSummaryAsync(string runId, CancellationToken ct)
    {
        await using var c = await OpenAsync(ct);
        if ((await ReadRunAsync(c, null, runId, ct))?.Status != RunStatus.Scored) return null;
        var card = await ReadScoreCorrectionAsync(c, null, runId, ct) ?? throw new PersistenceIntegrityException("Correction missing.");
        string json = await ReadScoreCorrectionReceiptAsync(c, null, card.ScorecardId, ct)
            ?? throw new PersistenceIntegrityException("Correction receipt missing.");
        var request = JsonSerializer.Deserialize<AnalysisScorecardRequest>(card.CanonicalOutputJson, CanonicalJson.SerializerOptions)!;
        var receipt = JsonSerializer.Deserialize<AnalysisScorecardReceipt>(json, CanonicalJson.SerializerOptions)!;
        if (!AnalysisScorecardClient.ReceiptMatches(receipt, card.ScenarioId, request))
            throw new PersistenceIntegrityException("Correction receipt does not match the corrected artifact.");
        return new(card.ScorecardId, card.RunId, card.RevealId, card.ScoringModelVersion, card.InputSha256, card.OutputSha256,
            card.HeadlineMetric, card.Metrics, card.CreatedValidTimeUtc, "Scored", DeterministicIdentity.Sha256(json)) { Correction = request.Correction };
    }
    private async Task MarkScoreCorrectionDeliveryAsync(string runId, string diagnostic, bool failed, CancellationToken ct)
    {
        await using var c = await OpenAsync(ct); await using var tx = c.BeginTransaction(false);
        var card = await ReadScoreCorrectionAsync(c, tx, runId, ct) ?? throw new PersistenceIntegrityException("Correction missing.");
        var run = await ReadRunAsync(c, tx, runId, ct) ?? throw new PersistenceIntegrityException("Correction run missing.");
        if (run.Status is RunStatus.Scored or RunStatus.Failed) { await tx.CommitAsync(ct); return; }
        if (await ReadScoreCorrectionReceiptAsync(c, tx, card.ScorecardId, ct) is not null)
            throw new PersistenceIntegrityException("Published correction cannot be deferred or failed.");
        string status = failed ? "Failed" : "AwaitingDependency"; DateTimeOffset now = timeProvider.GetUtcNow();
        await using (var q = Command(c, tx, """
            UPDATE ScorecardCorrectionDeliveries SET Status=$status,AttemptCount=AttemptCount+1,Diagnostic=$diagnostic,UpdatedUtc=$now WHERE ScorecardId=$id;
            UPDATE RunStages SET Status=$status,Diagnostics=$diagnostic,EndedUtc=$ended WHERE RunId=$run AND Stage=9;
            """))
        {
            Add(q, "$status", status); Add(q, "$diagnostic", diagnostic); Add(q, "$now", Format(now));
            Add(q, "$id", card.ScorecardId); Add(q, "$run", runId); Add(q, "$ended", failed ? Format(now) : null);
            if (await q.ExecuteNonQueryAsync(ct) != 2) throw new PersistenceIntegrityException("Correction delivery checkpoint missing.");
        }
        await UpdateRunAsync(c, tx, runId, failed ? RunStatus.Failed : RunStatus.AwaitingDependency, RunStageKind.S9Score,
            now, diagnostic, failed ? now : null, ct);
        await AppendAuditAsync(c, tx, run.ScenarioId, failed ? "scorecard.correction-failed" : "scorecard.correction-deferred",
            card.ScorecardId, CanonicalJson.Serialize(new { diagnostic }), ct);
        await tx.CommitAsync(ct);
    }
    private async Task CompleteScoreCorrectionAsync(ScorecardDraft card, string receiptJson, CancellationToken ct)
    {
        await using var c = await OpenAsync(ct); await using var tx = c.BeginTransaction(false);
        var persisted = await ReadScoreCorrectionAsync(c, tx, card.RunId, ct) ?? throw new PersistenceIntegrityException("Correction missing.");
        var run = await ReadRunAsync(c, tx, card.RunId, ct) ?? throw new PersistenceIntegrityException("Correction run missing.");
        if (persisted.CanonicalInputJson != card.CanonicalInputJson || persisted.CanonicalOutputJson != card.CanonicalOutputJson)
            throw new PersistenceIntegrityException("Correction changed before receipt completion.");
        var request = JsonSerializer.Deserialize<AnalysisScorecardRequest>(card.CanonicalOutputJson, CanonicalJson.SerializerOptions)!;
        var receipt = JsonSerializer.Deserialize<AnalysisScorecardReceipt>(receiptJson, CanonicalJson.SerializerOptions)!;
        if (!AnalysisScorecardClient.ReceiptMatches(receipt, card.ScenarioId, request)) throw new PersistenceIntegrityException("Correction receipt mismatch.");
        string? existing = await ReadScoreCorrectionReceiptAsync(c, tx, card.ScorecardId, ct);
        if (run.Status == RunStatus.Scored)
        {
            if (existing != receiptJson) throw new PersistenceIntegrityException("Correction replay receipt changed.");
            await tx.CommitAsync(ct); return;
        }
        if (run.CurrentStage != RunStageKind.S9Score || run.Status is not (RunStatus.AwaitingDependency or RunStatus.Running))
            throw new PersistenceIntegrityException("Correction run is not retryable.");
        string receiptHash = DeterministicIdentity.Sha256(receiptJson); DateTimeOffset now = timeProvider.GetUtcNow();
        if (existing is null)
        {
            await using var q = Command(c, tx, "INSERT INTO ScorecardCorrectionReceipts VALUES($id,$json,$hash,$now);");
            Add(q, "$id", card.ScorecardId); Add(q, "$json", receiptJson); Add(q, "$hash", receiptHash); Add(q, "$now", Format(now));
            await q.ExecuteNonQueryAsync(ct);
        }
        else if (existing != receiptJson) throw new PersistenceIntegrityException("Correction receipt changed.");
        string output = CorrectionStageOutput(card, "Scored", receiptHash);
        var audit = await AppendAuditAsync(c, tx, card.ScenarioId, "scorecard.correction-published", card.ScorecardId, output, ct);
        await using (var q = Command(c, tx, """
            UPDATE ScorecardCorrectionDeliveries SET Status='Completed',AttemptCount=AttemptCount+1,Diagnostic=NULL,UpdatedUtc=$now WHERE ScorecardId=$id;
            UPDATE RunStages SET Status='Completed',OutputJson=$output,OutputHash=$hash,EndedUtc=$now,Diagnostics=NULL,PreviousAuditHash=$previous WHERE RunId=$run AND Stage=9;
            """))
        {
            Add(q, "$id", card.ScorecardId); Add(q, "$run", card.RunId); Add(q, "$now", Format(now));
            Add(q, "$output", output); Add(q, "$hash", DeterministicIdentity.Sha256(output)); Add(q, "$previous", audit.PreviousHash);
            if (await q.ExecuteNonQueryAsync(ct) != 2) throw new PersistenceIntegrityException("Correction completion checkpoint missing.");
        }
        await UpdateRunAsync(c, tx, card.RunId, RunStatus.Scored, RunStageKind.S9Score, now, null, now, ct);
        await tx.CommitAsync(ct);
    }
    private static string CorrectionStageOutput(ScorecardDraft card, string status, string? receiptHash) =>
        CanonicalJson.Serialize(new { card.ScorecardId, card.ScoringModelVersion, card.InputSha256, card.OutputSha256,
            card.HeadlineMetric, metricCount = card.Metrics.Count, status, receiptHash, correctionVersion = ScoreCorrection.Version });

    private const string ScoringCorrectionSchema = """
        CREATE TABLE IF NOT EXISTS ScorecardCorrections(
            ScorecardId TEXT PRIMARY KEY,RunId TEXT NOT NULL UNIQUE,ScenarioId TEXT NOT NULL,RevealId TEXT NOT NULL,
            OriginalScorecardId TEXT NOT NULL UNIQUE,OriginalInputSha256 TEXT NOT NULL,OriginalOutputSha256 TEXT NOT NULL,
            CorrectionVersion TEXT NOT NULL,ScoringModelVersion TEXT NOT NULL,
            CanonicalInputJson TEXT NOT NULL,InputSha256 TEXT NOT NULL,CanonicalOutputJson TEXT NOT NULL,OutputSha256 TEXT NOT NULL,
            CreatedValidTimeUtc TEXT NOT NULL,CreatedUtc TEXT NOT NULL,
            FOREIGN KEY(RunId) REFERENCES Runs(RunId),FOREIGN KEY(OriginalScorecardId) REFERENCES Scorecards(ScorecardId));
        CREATE TABLE IF NOT EXISTS ScorecardCorrectionDeliveries(
            ScorecardId TEXT PRIMARY KEY,Status TEXT NOT NULL,AttemptCount INTEGER NOT NULL,Diagnostic TEXT NULL,UpdatedUtc TEXT NOT NULL,
            FOREIGN KEY(ScorecardId) REFERENCES ScorecardCorrections(ScorecardId));
        CREATE TABLE IF NOT EXISTS ScorecardCorrectionReceipts(
            ScorecardId TEXT PRIMARY KEY,CanonicalReceiptJson TEXT NOT NULL,ReceiptHash TEXT NOT NULL,ReceivedUtc TEXT NOT NULL,
            FOREIGN KEY(ScorecardId) REFERENCES ScorecardCorrections(ScorecardId));
        CREATE TRIGGER IF NOT EXISTS TR_ScorecardCorrections_NoUpdate BEFORE UPDATE ON ScorecardCorrections BEGIN SELECT RAISE(ABORT,'Scorecard corrections are immutable');END;
        CREATE TRIGGER IF NOT EXISTS TR_ScorecardCorrections_NoDelete BEFORE DELETE ON ScorecardCorrections BEGIN SELECT RAISE(ABORT,'Scorecard corrections are immutable');END;
        CREATE TRIGGER IF NOT EXISTS TR_ScorecardCorrectionReceipts_NoUpdate BEFORE UPDATE ON ScorecardCorrectionReceipts BEGIN SELECT RAISE(ABORT,'Scorecard correction receipts are immutable');END;
        CREATE TRIGGER IF NOT EXISTS TR_ScorecardCorrectionReceipts_NoDelete BEFORE DELETE ON ScorecardCorrectionReceipts BEGIN SELECT RAISE(ABORT,'Scorecard correction receipts are immutable');END;
        """;
}
