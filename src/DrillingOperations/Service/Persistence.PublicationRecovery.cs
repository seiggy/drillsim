using System.Globalization;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace DrillingOperations;

public sealed partial class DrillingOperationsStore
{
    internal async Task<PublicationRecoverySnapshot> CapturePublicationRecoveryAsync(
        string scenarioId, string runId, CancellationToken ct)
    {
        await using var c = await OpenAsync(ct);
        await using var tx = c.BeginTransaction(deferred: true);
        PublicationRecoverySnapshot result = await CapturePublicationRecoveryAsync(c, tx, scenarioId, runId, ct);
        await tx.CommitAsync(ct);
        return result;
    }

    internal async Task RecheckPublicationRecoveryAsync(PublicationRecoverySnapshot expected, CancellationToken ct)
    {
        PublicationRecoverySnapshot actual = await CapturePublicationRecoveryAsync(expected.Plan.ScenarioId, expected.Plan.RunId, ct);
        RequireRecovery(actual.GuardHash == expected.GuardHash, "PublicationRecoveryReviewStale");
    }

    internal Task<ApiOutcome> RecoverPublicationAsync(
        string route, string key, string canonicalRequest, RecoverPublicationRequest request,
        PublicationRecoverySnapshot expected, CancellationToken ct) =>
        ExecuteIdempotentAsync(route, key, canonicalRequest, async (c, tx, token) =>
        {
            // All captured rows (including operation attempts/receipts and checkpoints) are checked under
            // the same write transaction as the transition, audit append and idempotency result.
            PublicationRecoverySnapshot actual = await CapturePublicationRecoveryAsync(
                c, tx, expected.Plan.ScenarioId, expected.Plan.RunId, token);
            RequireRecovery(actual.GuardHash == expected.GuardHash &&
                actual.Review.ReviewedPublicationHash == request.ReviewedPublicationHash, "PublicationRecoveryReviewStale");
            string now = NowText();
            await using var update = Command(c, tx, """
                UPDATE Runs SET Status='PublishFailed',EndedUtc=NULL,UpdatedUtc=$now
                WHERE RunId=$run AND ScenarioId=$scenario AND Status='Failed' AND CurrentStage=8
                  AND DiagnosticCode='PublicationWriteRejected' AND PublicationCount=0 AND ClockAdvanceCount=0;
                """);
            Add(update, "$now", now); Add(update, "$run", actual.Plan.RunId); Add(update, "$scenario", actual.Plan.ScenarioId);
            RequireRecovery(await update.ExecuteNonQueryAsync(token) == 1, "PublicationRecoveryRace");
            var audit = await AppendAuditAsync(c, tx, actual.Plan.ScenarioId, "publication.recovery-approved", actual.Plan.RunId,
                CanonicalJson.Serialize(new
                {
                    request.Actor, request.Reason, request.ReviewedPublicationHash,
                    previousStatus = "Failed", status = "PublishFailed", stage = "S8PublishReveal",
                    failureCode = "PublicationWriteRejected", actual.Review.StagedManifestSha256,
                    actual.Review.PublicationPlanSha256
                }), token);
            await using var auditQuery = Command(c, tx, "SELECT AuditId FROM AuditEntries WHERE ScenarioId=$scenario AND EntryHash=$hash;");
            Add(auditQuery, "$scenario", actual.Plan.ScenarioId); Add(auditQuery, "$hash", audit.EntryHash);
            string auditId = (string)(await auditQuery.ExecuteScalarAsync(token))!;
            return Json(200, new PublicationRecoveryResult(actual.Plan.ScenarioId, actual.Plan.RunId,
                "publication-recovered", "Failed", "PublishFailed", request.ReviewedPublicationHash, auditId));
        }, ct);

    private async Task<PublicationRecoverySnapshot> CapturePublicationRecoveryAsync(
        SqliteConnection c, SqliteTransaction tx, string scenarioId, string runId, CancellationToken ct)
    {
        RunResponse? run = await ReadRunAsync(c, tx, runId, ct);
        if (run is null || run.ScenarioId != scenarioId) throw new PublicationRecoveryException(404, "PublicationRecoveryRunNotOwned");
        RequireRecovery(run.Status == RunStatus.Failed && run.CurrentStage == RunStageKind.S8PublishReveal,
            "PublicationRecoveryWrongPhase");
        RequireRecovery(!await HasActiveRunAsync(c, tx, scenarioId, ct), "PublicationRecoveryOtherActiveRun");
        var captured = new SortedDictionary<string, IReadOnlyList<Dictionary<string, string?>>>(StringComparer.Ordinal);
        async Task<IReadOnlyList<Dictionary<string, string?>>> Rows(string name, string sql)
        {
            var rows = new List<Dictionary<string, string?>>();
            await using var q = Command(c, tx, sql);
            Add(q, "$run", runId); Add(q, "$scenario", scenarioId);
            await using var reader = await q.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                RequireRecovery(rows.Count < 20000, "PublicationRecoveryArtifactBoundExceeded");
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
            RequireRecovery(rows.Count == 1, "PublicationRecoveryArtifactMissing");
            return rows[0];
        }
        var runRow = One(await Rows("run", "SELECT * FROM Runs WHERE RunId=$run;"));
        RequireRecovery(runRow["DiagnosticCode"] == "PublicationWriteRejected" &&
            runRow["PublicationCount"] == "0" && runRow["ClockAdvanceCount"] == "0",
            "PublicationRecoveryFailureOrSideEffects");
        ValidateRecoveryHash(runRow, "CanonicalInputJson", "InputHash");
        var runInput = JsonSerializer.Deserialize<CreateRunRequest>(runRow["CanonicalInputJson"]!, CanonicalJson.SerializerOptions)!;
        RequireRecovery(runInput.ScenarioId == scenarioId && runInput.ApprovedSealedPredictionHash == runRow["ApprovedPredictionHash"] &&
            runInput.PlanArtifactId == runRow["PlanArtifactId"] && runInput.PlanArtifactSha256 == runRow["PlanArtifactSha256"] &&
            runId == DeterministicIdentity.Create("drilling-run", scenarioId, runInput.ApprovedSealedPredictionHash, runInput.PlanArtifactSha256),
            "PublicationRecoveryRunIntegrityMismatch");
        TruthBindingResponse binding = await ReadBindingAsync(c, tx, scenarioId, ct)
            ?? throw new PublicationRecoveryException(409, "PublicationRecoveryBindingMissing");
        var bindingRow = One(await Rows("binding", "SELECT * FROM TruthBindings WHERE ScenarioId=$scenario;"));
        RequireRecovery(binding.BindingId == runRow["BindingId"] &&
            binding.ApprovedSealedPredictionHash == runRow["ApprovedPredictionHash"], "PublicationRecoveryBindingChanged");
        var stages = await Rows("stages", "SELECT * FROM RunStages WHERE RunId=$run ORDER BY Stage;");
        RequireRecovery(stages.Count == 10, "PublicationRecoveryCheckpointMissing");
        for (int i = 0; i < 8; i++)
        {
            RequireRecovery(stages[i]["Stage"] == i.ToString(CultureInfo.InvariantCulture) &&
                stages[i]["Status"] == "Completed", "PublicationRecoveryCheckpointIncomplete");
            ValidateRecoveryHash(stages[i], "InputJson", "InputHash");
            ValidateRecoveryHash(stages[i], "OutputJson", "OutputHash");
        }
        RequireRecovery(stages[8]["Stage"] == "8" && stages[8]["Status"] == "Failed" &&
            stages[8]["Diagnostics"] == "PublicationWriteRejected" &&
            stages[8]["OutputJson"] is null && stages[8]["OutputHash"] is null &&
            stages[9]["Stage"] == "9" && stages[9]["Status"] == "Pending" &&
            stages[9]["InputJson"] is null && stages[9]["OutputJson"] is null &&
            stages[9]["InputHash"] is null && stages[9]["OutputHash"] is null &&
            stages[9]["AttemptCount"] == "0", "PublicationRecoveryWrongCheckpoint");
        string s0 = CanonicalJson.Serialize(new { binding.BindingId, bindingInputHash = bindingRow["InputHash"] });
        RequireRecovery(stages[0]["InputJson"] == s0 && stages[0]["OutputJson"] == s0, "PublicationRecoveryBindingChanged");

        foreach (string table in new[] { "MaterializedPlans", "DrillingExecutions", "SurveyArtifacts", "TruthSampleBatches",
            "ObservationBatches", "CompletionDesigns", "ProductionTruthArtifacts", "ProductionSeries", "DrillingObservationArtifacts" })
            await Rows(table, $"SELECT * FROM {table} WHERE RunId=$run ORDER BY rowid;");
        foreach (string table in new[] { "MaterializedPlans", "DrillingExecutions", "SurveyArtifacts", "TruthSampleBatches",
            "ObservationBatches", "CompletionDesigns", "ProductionTruthArtifacts", "ProductionSeries" })
            _ = One(captured[table]);
        await Rows("logDetails", "SELECT * FROM LogObservationDetails WHERE ObservationBatchId IN (SELECT ObservationBatchId FROM ObservationBatches WHERE RunId=$run);");
        await Rows("completionApproval", "SELECT * FROM CompletionApprovals WHERE CompletionDesignId IN (SELECT CompletionDesignId FROM CompletionDesigns WHERE RunId=$run);");
        _ = One(captured["completionApproval"]);
        await Rows("drillingDetails", "SELECT * FROM DrillingObservationDetails WHERE ArtifactId IN (SELECT ArtifactId FROM DrillingObservationArtifacts WHERE RunId=$run);");
        await Rows("drillingSeries", "SELECT * FROM DrillingObservationSeries WHERE ArtifactId IN (SELECT ArtifactId FROM DrillingObservationArtifacts WHERE RunId=$run);");
        foreach (var row in captured.Values.SelectMany(x => x))
            foreach (var pair in RecoveryHashColumns)
                if (row.ContainsKey(pair.Json)) ValidateRecoveryHash(row, pair.Json, pair.Hash);

        MaterializedPlan materialized = await ReadMaterializedPlanAsync(c, tx, runId, ct)
            ?? throw new PublicationRecoveryException(409, "PublicationRecoveryPlanMissing");
        RequireRecovery(materialized.BindingId == binding.BindingId && materialized.ScenarioId == scenarioId &&
            materialized.SourcePredictionSealSha256 == binding.ApprovedSealedPredictionHash &&
            materialized.SourcePackageSha256 == binding.SourcePackageSha256, "PublicationRecoveryBindingChanged");
        RequireRecovery(RecoveryProperty(stages[1]["OutputJson"], "planId") == materialized.PlanId &&
            RecoveryProperty(stages[1]["OutputJson"], "artifactHash") == materialized.ArtifactHash &&
            RecoveryProperty(stages[1]["OutputJson"], "pathHash") == materialized.PathHash, "PublicationRecoveryPlanCommitmentMismatch");
        foreach ((int stage, string table, string hash) in new[] {
            (2, "DrillingExecutions", "OutputHash"), (3, "SurveyArtifacts", "OutputHash"),
            (4, "TruthSampleBatches", "ResponseHash"), (7, "ProductionSeries", "OutputHash") })
            RequireRecovery(RecoveryProperty(stages[stage]["OutputJson"], "artifactHash") == One(captured[table])[hash],
                "PublicationRecoveryOutputCommitmentMismatch");
        RequireRecovery(stages[1]["InputJson"] == One(captured["MaterializedPlans"])["CanonicalInputJson"] &&
            stages[2]["InputJson"] == One(captured["DrillingExecutions"])["CanonicalInputJson"] &&
            stages[3]["InputJson"] == One(captured["SurveyArtifacts"])["CanonicalInputJson"] &&
            stages[6]["InputJson"] == One(captured["CompletionDesigns"])["CanonicalObservableInputJson"] &&
            stages[7]["InputJson"] == One(captured["ProductionSeries"])["CanonicalInputJson"],
            "PublicationRecoveryInputCommitmentMismatch");
        var completion = One(captured["CompletionDesigns"]);
        RequireRecovery(RecoveryProperty(stages[6]["OutputJson"], "completionDesignId") == completion["CompletionDesignId"] &&
            RecoveryProperty(stages[6]["OutputJson"], "openingsHash") == completion["OpeningsHash"],
            "PublicationRecoveryCompletionCommitmentMismatch");

        var planRow = One(await Rows("publicationPlan", "SELECT * FROM PublicationPlans WHERE RunId=$run;"));
        var state = One(await Rows("publicationState", "SELECT * FROM PublicationStates WHERE PublicationPlanId IN (SELECT PublicationPlanId FROM PublicationPlans WHERE RunId=$run);"));
        RequireRecovery(state["Status"] == "Staged" && state["Diagnostic"] is null &&
            new[] { "ReceiptJson", "ReceiptHash", "PreparedReceiptJson", "PreparedReceiptHash", "FinalReceiptJson",
                "FinalReceiptHash", "FinalManifestJson", "FinalManifestHash", "PublicProductionHash" }.All(x => state[x] is null),
            "PublicationRecoveryRevealAlreadyPrepared");
        RequireRecovery((await Rows("reveal", "SELECT * FROM RevealManifests WHERE RunId=$run;")).Count == 0,
            "PublicationRecoveryRevealAlreadyPrepared");
        PublicationStaging plan = await ReadPublicationAsync(c, tx, runId, ct)
            ?? throw new PublicationRecoveryException(409, "PublicationRecoveryPublicationMissing");
        RequireRecovery(plan.ScenarioId == scenarioId && plan.RunId == runId && plan.Operations.Count is > 0 and <= 20000 &&
            plan.Operations.Count.ToString(CultureInfo.InvariantCulture) == planRow["OperationCount"] &&
            plan.PublicationPlanId == DeterministicIdentity.Create("publication-plan-v1", scenarioId, runId, plan.ClonedFieldId,
                string.Join("\n", plan.Operations.Select(x => x.PayloadHash))), "PublicationRecoveryPlanIntegrityMismatch");
        string s8Input = CanonicalJson.Serialize(new { plan.PublicationPlanId, plan.ManifestHash });
        RequireRecovery(stages[8]["InputJson"] == s8Input && stages[8]["InputHash"] == DeterministicIdentity.Sha256(s8Input),
            "PublicationRecoveryStagingCommitmentMismatch");
        var audits = await Rows("audit", "SELECT * FROM AuditEntries WHERE ScenarioId=$scenario ORDER BY Sequence;");
        string previous = EmptyHash;
        for (int i = 0; i < audits.Count; i++)
        {
            var audit = audits[i];
            ValidateRecoveryHash(audit, "DataJson", "DataHash");
            RequireRecovery(audit["Sequence"] == (i + 1).ToString(CultureInfo.InvariantCulture) &&
                audit["PreviousHash"] == previous && audit["EntryHash"] == AuditHash(scenarioId, i + 1,
                    audit["Action"]!, audit["SubjectId"]!, audit["DataHash"]!, previous, audit["CreatedUtc"]!),
                "PublicationRecoveryAuditMismatch");
            previous = audit["EntryHash"]!;
        }
        string stagedAudit = CanonicalJson.Serialize(new
        {
            plan.RevealId, plan.ManifestHash, operationCount = plan.Operations.Count,
            evidenceCount = plan.Evidence.Count, plan.ClonedFieldId
        });
        RequireRecovery(audits.Count(a => a["Action"] == "publication.staged" && a["SubjectId"] == plan.RevealId &&
            a["DataJson"] == stagedAudit) == 1, "PublicationRecoveryStagingAuditMismatch");
        foreach (int i in new[] { 0, 1, 2, 3, 4, 5, 7 })
            RequireRecovery(audits.Any(a => a["Action"] == "stage.completed" && a["SubjectId"] == runId &&
                a["PreviousHash"] == stages[i]["PreviousAuditHash"] &&
                RecoveryProperty(a["DataJson"], "stage") == ((RunStageKind)i).ToString() &&
                RecoveryProperty(a["DataJson"], "inputHash") == stages[i]["InputHash"] &&
                RecoveryProperty(a["DataJson"], "outputHash") == stages[i]["OutputHash"]),
                "PublicationRecoveryCheckpointAuditMismatch");
        using (var manifest = JsonDocument.Parse(plan.ManifestJson))
        {
            JsonElement root = manifest.RootElement;
            RequireRecovery(root.GetProperty("scenarioId").GetString() == scenarioId &&
                root.GetProperty("runId").GetString() == runId && root.GetProperty("revealId").GetString() == plan.RevealId &&
                root.GetProperty("clonedFieldId").GetString() == plan.ClonedFieldId &&
                root.GetProperty("validTimeUtc").GetDateTimeOffset() == plan.ValidTimeUtc &&
                root.GetProperty("observationModelVersion").GetString() == plan.ObservationModelVersion &&
                root.GetProperty("sourcePackageSha256").GetString() == binding.SourcePackageSha256,
                "PublicationRecoveryManifestIdentityMismatch");
        }
        var opRows = await Rows("operations", "SELECT * FROM PublicationOperations WHERE PublicationPlanId IN (SELECT PublicationPlanId FROM PublicationPlans WHERE RunId=$run) ORDER BY Sequence;");
        var opStates = await Rows("operationStates", "SELECT s.* FROM PublicationOperationStates s JOIN PublicationOperations o ON o.OperationId=s.OperationId WHERE o.PublicationPlanId IN (SELECT PublicationPlanId FROM PublicationPlans WHERE RunId=$run) ORDER BY o.Sequence;");
        RequireRecovery(opStates.Count == plan.Operations.Count, "PublicationRecoveryOperationMissing");
        var operations = new List<PublicationRecoveryOperation>();
        for (int i = 0; i < plan.Operations.Count; i++)
        {
            PublicationWriteOperation op = plan.Operations[i];
            var os = opStates[i];
            RequireRecovery(op.Sequence == i + 1 && op.OperationId == os["OperationId"] &&
                op.OperationId == DeterministicIdentity.Create("publication-operation-v1", scenarioId, op.RecordKind, op.EntityId, op.PayloadHash) &&
                plan.Evidence.Count(e => e.EvidenceId == op.EntityId && e.RecordKind == op.RecordKind && e.ContentSha256 == op.PayloadHash) == 1,
                "PublicationRecoveryOperationIntegrityMismatch");
            RequireRecovery(os["ActivationStatus"] == "Pending" && os["ActivationAttemptCount"] == "0" &&
                os["ActivatedUtc"] is null && os["ActivationDiagnostic"] is null &&
                os["ReconciliationAttemptCount"] == "0" && os["LastReconciledUtc"] is null &&
                os["ReconciliationDiagnostic"] is null, "PublicationRecoveryActivationStarted");
            RequireRecovery(os["Status"] is "Pending" or "Verified" or "Failed" &&
                (os["Status"] != "Failed" || os["Diagnostic"] == "PublicationWriteRejected"),
                "PublicationRecoveryOperationFailureMismatch");
            ValidateRecoveryRoute(op);
            using var payload = JsonDocument.Parse(op.CanonicalPayloadJson);
            RequireRecovery(PublicationJsonElement.EntityId(payload.RootElement).ToString("D") == op.EntityId &&
                !PublicationJson.ContainsForbiddenProperty(payload.RootElement), "PublicationRecoveryPayloadInvalid");
            if (os["Status"] == "Verified")
            {
                ValidateRecoveryHash(os, "ResultBusinessJson", "ResultHash");
                using var business = JsonDocument.Parse(os["ResultBusinessJson"]!);
                RequireRecovery(os["VerifiedUtc"] is not null && os["Diagnostic"] is null &&
                    PublicationJson.BusinessContentMatches(payload.RootElement, business.RootElement),
                    "PublicationRecoveryVerifiedContentMismatch");
                RequireRecovery(op.EntityId == plan.ProductionEntityId
                    ? os["ProductionRepresentationHash"] == PublicationJson.MonthlyProductionHash(business.RootElement)
                    : os["ProductionRepresentationHash"] is null, "PublicationRecoveryProductionCommitmentMismatch");
            }
            else
                RequireRecovery(os["ResultHash"] is null && os["ResultBusinessJson"] is null &&
                    os["VerifiedUtc"] is null && os["ProductionRepresentationHash"] is null,
                    "PublicationRecoveryOperationReceiptMismatch");
            operations.Add(new(op, os["Status"]!, os["ResultHash"], os["ResultBusinessJson"]));
        }
        int verified = operations.Count(x => x.Status == "Verified");
        RequireRecovery(operations.Count(x => x.Status == "Failed") == 1 &&
            state["VerifiedOperationCount"] == verified.ToString(CultureInfo.InvariantCulture),
            "PublicationRecoveryOperationCountsMismatch");
        string guardHash = DeterministicIdentity.Sha256(CanonicalJson.Serialize(captured));
        string planHash = DeterministicIdentity.Sha256(CanonicalJson.Serialize(new { plan = planRow, operations = opRows }));
        var review = new PublicationRecoveryReview(scenarioId, runId, true,
            "Review this unchanged staged publication. Recovery only enables a separate explicit publish attempt.",
            DeterministicIdentity.Sha256("publication-recovery-review-v1\n" + guardHash),
            plan.ManifestHash, planHash, operations.Count, verified, operations.Count - verified, 8);
        return new(review, plan, binding, materialized, guardHash, operations);
    }

    internal async Task ValidatePublicationRecoveryArtifactsAsync(PublicationRecoverySnapshot snapshot, CancellationToken ct)
    {
        string runId = snapshot.Plan.RunId;
        var execution = await GetDrillingExecutionAsync(runId, ct);
        var survey = await GetSurveyArtifactAsync(runId, ct);
        var truth = await GetTruthSampleBatchAsync(runId, ct);
        var logs = await GetLogObservationBatchAsync(runId, ct);
        var completion = await GetCompletionDesignAsync(runId, ct);
        var production = await GetProductionSeriesAsync(runId, ct);
        var productionTruth = await GetProductionTruthAsync(runId, ct);
        var drilling = await GetDrillingObservationArtifactAsync(runId, ct);
        RequireRecovery(execution is not null && survey is not null && truth is not null && logs is not null &&
            completion is not null && production is not null && productionTruth is not null, "PublicationRecoveryArtifactMissing");
        RequireRecovery(execution!.PlanId == snapshot.MaterializedPlan.PlanId && survey!.ExecutionId == execution.ExecutionId &&
            logs!.ObservationBatchId == snapshot.Plan.ObservationBatchId && completion!.Status == "Approved" &&
            production!.ProductionSeriesId == snapshot.Plan.ProductionSeriesId && production.OutputHash == snapshot.Plan.ProductionHash &&
            production.ModelVersion == snapshot.Plan.ProductionModelVersion &&
            snapshot.Plan.ProductionEntityId == snapshot.MaterializedPlan.ScenarioWellId &&
            snapshot.Plan.RevealId == DeterministicIdentity.Create("reveal-v1", snapshot.Plan.ScenarioId, runId, production.OutputHash),
            "PublicationRecoveryArtifactBindingMismatch");
        // Legacy S5 has no drilling-observation artifact. Its immutable log commitment is still checked.
        await using var c = await OpenAsync(ct);
        await using var q = c.CreateCommand();
        q.CommandText = "SELECT OutputJson FROM RunStages WHERE RunId=$run AND Stage=5;";
        Add(q, "$run", runId);
        using var s5 = JsonDocument.Parse((string)(await q.ExecuteScalarAsync(ct))!);
        if (s5.RootElement.TryGetProperty("contractVersion", out _))
            RequireRecovery(drilling is not null, "PublicationRecoveryDrillingObservationMissing");
        else
            RequireRecovery(drilling is null && s5.RootElement.GetProperty("observationBatchId").GetString() == logs!.ObservationBatchId &&
                s5.RootElement.GetProperty("artifactHash").GetString() == logs.OutputHash,
                "PublicationRecoveryLogCommitmentMismatch");
    }

    private static readonly (string Json, string Hash)[] RecoveryHashColumns =
    [
        ("CanonicalInputJson", "InputHash"), ("CanonicalOutputJson", "OutputHash"), ("CanonicalPathJson", "PathHash"),
        ("SurveyPathJson", "PathHash"), ("CanonicalOptionsJson", "OptionsHash"),
        ("CanonicalBindingJson", "BindingHash"), ("CanonicalBindingRequestJson", "BindingRequestHash"),
        ("CanonicalSamplingOptionsJson", "SamplingOptionsHash"), ("CanonicalResponseJson", "ResponseHash"),
        ("CanonicalObservableInputJson", "ObservableInputHash"), ("OpeningsJson", "OpeningsHash"),
        ("BindingMetadataJson", "BindingMetadataHash"), ("CanonicalExecutionOptionsJson", "ExecutionOptionsHash"),
        ("CanonicalRequestJson", "RequestHash"), ("SamplesJson", "SamplesHash"), ("CuttingsJson", "CuttingsHash"),
        ("EventsJson", "EventsHash"), ("CurvesJson", "CurvesHash"), ("SummaryJson", "SummaryHash")
    ];

    private static void ValidateRecoveryHash(Dictionary<string, string?> row, string json, string hash) =>
        RequireRecovery(row.TryGetValue(json, out string? value) && value is not null &&
            row.TryGetValue(hash, out string? expected) && DeterministicIdentity.Sha256(value) == expected,
            "PublicationRecoveryIntegrityMismatch");

    private static string? RecoveryProperty(string? json, string property)
    {
        using JsonDocument document = JsonDocument.Parse(json!);
        return document.RootElement.GetProperty(property).GetString();
    }

    private static void ValidateRecoveryRoute(PublicationWriteOperation op)
    {
        RequireRecovery(op.RecordKind is "Field" or "Cluster" or "Well" or "WellBore" or "WellBoreArchitecture" or "Trajectory" or "GeologicalProperties",
            "PublicationRecoveryRouteInvalid");
        string prefix = $"/{op.RecordKind.ToLowerInvariant()}/api";
        string route = op.RecordKind == "GeologicalProperties"
            ? prefix + "/internal/publication/GeologicalProperties" : prefix + "/" + op.RecordKind;
        RequireRecovery(op.TargetService == op.RecordKind + "Service" && op.Route == route &&
            op.ReadRoute == prefix + "/" + op.RecordKind + "/" + op.EntityId +
                (op.RecordKind == "Trajectory" ? "?includeCalculatedStations=true" : ""), "PublicationRecoveryRouteInvalid");
    }

    private static void RequireRecovery(bool condition, string code)
    {
        if (!condition) throw new PublicationRecoveryException(409, code);
    }
}
