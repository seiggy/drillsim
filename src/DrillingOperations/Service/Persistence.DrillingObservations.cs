using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DrillingOperations;

public sealed partial class DrillingOperationsStore
{
    public const string CombinedS5ObservationContractVersion = "s5-combined-observations-v4";

    private async Task InitializeDrillingObservationSchemaAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand(); command.CommandText = DrillingObservationSchema; await command.ExecuteNonQueryAsync(cancellationToken);
        await using SqliteCommand inspect = connection.CreateCommand(); inspect.CommandText = "SELECT COUNT(*) FROM pragma_table_info('DrillingObservationArtifacts') WHERE name='LogContentHash';";
        if (Convert.ToInt32(await inspect.ExecuteScalarAsync(cancellationToken)) == 0) { await using SqliteCommand alter = connection.CreateCommand(); alter.CommandText = "ALTER TABLE DrillingObservationArtifacts ADD COLUMN LogContentHash TEXT NULL;"; await alter.ExecuteNonQueryAsync(cancellationToken); }
    }

    private async Task<DrillingObservationArtifact> PersistDrillingObservationsAsync(SqliteConnection connection, SqliteTransaction transaction, RunResponse run, DrillingExecutionArtifact execution, SurveyArtifact survey, LogObservationArtifact logs, string logHash, string logContentHash, DrillingObservationOptions options, DateTimeOffset createdUtc, CancellationToken cancellationToken)
    {
        string optionsJson = CanonicalJson.Serialize(options), optionsHash = DeterministicIdentity.Sha256(optionsJson);
        string input = CanonicalJson.Serialize(new { contractVersion = CombinedS5ObservationContractVersion, runId = run.RunId, scenarioId = run.ScenarioId, executionId = execution.ExecutionId, executionHash = execution.OutputHash, surveyArtifactId = survey.SurveyArtifactId, surveyHash = survey.OutputHash, logObservationBatchId = logs.ObservationBatchId, logContentHash, optionsHash, options.ModelVersion, options.CalibrationVersion });
        string inputHash = DeterministicIdentity.Sha256(input), artifactId = DeterministicIdentity.Create("drilling-observation-artifact-v1", run.RunId, inputHash);
        var generated = DeterministicDrillingObservationModel.Generate(run.ScenarioId, run.RunId, execution, survey, logs with { OutputHash = logHash }, options);
        var artifact = new DrillingObservationArtifact(artifactId, run.RunId, run.ScenarioId, execution.ExecutionId, execution.OutputHash, survey.SurveyArtifactId, survey.OutputHash, logs.ObservationBatchId, logHash, logContentHash, options.ModelVersion, options.CalibrationVersion, optionsHash, DeterministicDrillingObservationModel.CurveDefinitions(options), generated.Samples, generated.Cuttings, generated.Events, generated.Summary, inputHash, string.Empty, createdUtc);
        string output = CanonicalDrillingObservationContent(artifact), outputHash = DeterministicIdentity.Sha256(output), curvesJson = CanonicalJson.Serialize(artifact.Curves), samplesJson = CanonicalJson.Serialize(artifact.Samples), cuttingsJson = CanonicalJson.Serialize(artifact.Cuttings), eventsJson = CanonicalJson.Serialize(artifact.Events), summaryJson = CanonicalJson.Serialize(artifact.Summary);
        await using (SqliteCommand insert = Command(connection, transaction, """
            INSERT INTO DrillingObservationArtifacts(ArtifactId,RunId,ScenarioId,ExecutionId,ExecutionHash,SurveyArtifactId,SurveyHash,LogObservationBatchId,LogHash,LogContentHash,ModelVersion,CalibrationVersion,OptionsHash,CanonicalInputJson,InputHash,CanonicalOutputJson,OutputHash,CreatedUtc)
            VALUES($id,$run,$scenario,$execution,$executionHash,$survey,$surveyHash,$log,$logHash,$logContentHash,$model,$calibration,$optionsHash,$input,$inputHash,$output,$outputHash,$created);
            """))
        {
            Add(insert,"$id",artifactId);Add(insert,"$run",run.RunId);Add(insert,"$scenario",run.ScenarioId);Add(insert,"$execution",execution.ExecutionId);Add(insert,"$executionHash",execution.OutputHash);Add(insert,"$survey",survey.SurveyArtifactId);Add(insert,"$surveyHash",survey.OutputHash);Add(insert,"$log",logs.ObservationBatchId);Add(insert,"$logHash",logHash);Add(insert,"$logContentHash",logContentHash);Add(insert,"$model",options.ModelVersion);Add(insert,"$calibration",options.CalibrationVersion);Add(insert,"$optionsHash",optionsHash);Add(insert,"$input",input);Add(insert,"$inputHash",inputHash);Add(insert,"$output",output);Add(insert,"$outputHash",outputHash);Add(insert,"$created",Format(createdUtc));await insert.ExecuteNonQueryAsync(cancellationToken);
        }
        await using (SqliteCommand details = Command(connection, transaction, """
            INSERT INTO DrillingObservationDetails(ArtifactId,CanonicalOptionsJson,OptionsHash,CurvesJson,CurvesHash,SummaryJson,SummaryHash,SampleCount,CuttingsCount,EventCount)
            VALUES($id,$options,$optionsHash,$curves,$curvesHash,$summary,$summaryHash,$samples,$cuttings,$events);
            """))
        {
            Add(details,"$id",artifactId);Add(details,"$options",optionsJson);Add(details,"$optionsHash",optionsHash);Add(details,"$curves",curvesJson);Add(details,"$curvesHash",DeterministicIdentity.Sha256(curvesJson));Add(details,"$summary",summaryJson);Add(details,"$summaryHash",DeterministicIdentity.Sha256(summaryJson));Add(details,"$samples",artifact.Samples.Count);Add(details,"$cuttings",artifact.Cuttings.Count);Add(details,"$events",artifact.Events.Count);await details.ExecuteNonQueryAsync(cancellationToken);
        }
        await using (SqliteCommand series = Command(connection, transaction, """
            INSERT INTO DrillingObservationSeries(ArtifactId,SamplesJson,SamplesHash,CuttingsJson,CuttingsHash,EventsJson,EventsHash)
            VALUES($id,$samples,$samplesHash,$cuttings,$cuttingsHash,$events,$eventsHash);
            """))
        {
            Add(series,"$id",artifactId);Add(series,"$samples",samplesJson);Add(series,"$samplesHash",DeterministicIdentity.Sha256(samplesJson));Add(series,"$cuttings",cuttingsJson);Add(series,"$cuttingsHash",DeterministicIdentity.Sha256(cuttingsJson));Add(series,"$events",eventsJson);Add(series,"$eventsHash",DeterministicIdentity.Sha256(eventsJson));await series.ExecuteNonQueryAsync(cancellationToken);
        }
        return artifact with { OutputHash = outputHash };
    }

    public async Task<DrillingObservationArtifact?> GetDrillingObservationArtifactAsync(string runId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand query = connection.CreateCommand(); query.CommandText = """
            SELECT a.ArtifactId,a.RunId,a.ScenarioId,a.ExecutionId,a.ExecutionHash,a.SurveyArtifactId,a.SurveyHash,a.LogObservationBatchId,a.LogHash,a.ModelVersion,a.CalibrationVersion,a.OptionsHash,a.CanonicalInputJson,a.InputHash,a.CanonicalOutputJson,a.OutputHash,a.CreatedUtc,
                   d.CanonicalOptionsJson,d.OptionsHash,d.CurvesJson,d.CurvesHash,d.SummaryJson,d.SummaryHash,d.SampleCount,d.CuttingsCount,d.EventCount,
                   s.SamplesJson,s.SamplesHash,s.CuttingsJson,s.CuttingsHash,s.EventsJson,s.EventsHash,r.ScenarioId,e.OutputHash,sv.OutputHash,o.OutputHash,a.LogContentHash,o.CanonicalOutputJson,rs.InputJson,rs.InputHash,rs.OutputJson,rs.OutputHash
            FROM DrillingObservationArtifacts a JOIN DrillingObservationDetails d ON d.ArtifactId=a.ArtifactId JOIN DrillingObservationSeries s ON s.ArtifactId=a.ArtifactId JOIN Runs r ON r.RunId=a.RunId JOIN DrillingExecutions e ON e.ExecutionId=a.ExecutionId JOIN SurveyArtifacts sv ON sv.SurveyArtifactId=a.SurveyArtifactId JOIN ObservationBatches o ON o.ObservationBatchId=a.LogObservationBatchId JOIN RunStages rs ON rs.RunId=a.RunId AND rs.Stage=5 WHERE a.RunId=$run;
            """; Add(query,"$run",runId);
        await using SqliteDataReader reader = await query.ExecuteReaderAsync(cancellationToken); if (!await reader.ReadAsync(cancellationToken)) return null;
        try
        {
            DrillingObservationOptions options = JsonSerializer.Deserialize<DrillingObservationOptions>(reader.GetString(17), CanonicalJson.SerializerOptions) ?? throw new JsonException(); options.Validate();
            DrillingObservationCurve[] curves = JsonSerializer.Deserialize<DrillingObservationCurve[]>(reader.GetString(19), CanonicalJson.SerializerOptions) ?? throw new JsonException();
            DrillingObservationSummary summary = JsonSerializer.Deserialize<DrillingObservationSummary>(reader.GetString(21), CanonicalJson.SerializerOptions) ?? throw new JsonException();
            DrillingObservationSample[] samples = JsonSerializer.Deserialize<DrillingObservationSample[]>(reader.GetString(26), CanonicalJson.SerializerOptions) ?? throw new JsonException();
            CuttingsObservation[] cuttings = JsonSerializer.Deserialize<CuttingsObservation[]>(reader.GetString(28), CanonicalJson.SerializerOptions) ?? throw new JsonException();
            DrillingObservationEvent[] events = JsonSerializer.Deserialize<DrillingObservationEvent[]>(reader.GetString(30), CanonicalJson.SerializerOptions) ?? throw new JsonException();
            LogObservationArtifact sourceLog = JsonSerializer.Deserialize<LogObservationArtifact>(reader.GetString(37), CanonicalJson.SerializerOptions) ?? throw new JsonException();
            if (reader.IsDBNull(36)) throw new PersistenceIntegrityException("Drilling observation artifact predates stable log-content provenance and cannot be used; a new run is required.");
            string logContentHash = reader.GetString(36);
            if (!DateTimeOffset.TryParseExact(reader.GetString(16), "O", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out DateTimeOffset storedCreated) || storedCreated.Offset != TimeSpan.Zero) throw new PersistenceIntegrityException("Drilling observation artifact integrity failure.");
            var artifact = new DrillingObservationArtifact(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetString(7), reader.GetString(8), logContentHash, reader.GetString(9), reader.GetString(10), reader.GetString(11), curves, samples, cuttings, events, summary, reader.GetString(13), string.Empty, storedCreated);
            string expectedInput = CanonicalJson.Serialize(new { contractVersion = CombinedS5ObservationContractVersion, runId = reader.GetString(1), scenarioId = reader.GetString(2), executionId = reader.GetString(3), executionHash = reader.GetString(4), surveyArtifactId = reader.GetString(5), surveyHash = reader.GetString(6), logObservationBatchId = reader.GetString(7), logContentHash, optionsHash = reader.GetString(11), modelVersion = reader.GetString(9), calibrationVersion = reader.GetString(10) });
            string expectedOutput = CanonicalDrillingObservationContent(artifact);
            bool invalid = reader.GetString(1) != runId || reader.GetString(2) != reader.GetString(32) || reader.GetString(4) != reader.GetString(33) || reader.GetString(6) != reader.GetString(34) || reader.GetString(8) != reader.GetString(35) || logContentHash != ComputeLogObservationContentHash(sourceLog) || reader.GetString(9) != DrillingObservationOptions.CurrentModelVersion || reader.GetString(11) != options.Hash() || reader.GetString(12) != expectedInput || reader.GetString(13) != DeterministicIdentity.Sha256(expectedInput) || reader.GetString(0) != DeterministicIdentity.Create("drilling-observation-artifact-v1", runId, reader.GetString(13)) || reader.GetString(14) != expectedOutput || reader.GetString(15) != DeterministicIdentity.Sha256(expectedOutput) || reader.GetString(18) != reader.GetString(11) || reader.GetString(20) != DeterministicIdentity.Sha256(reader.GetString(19)) || reader.GetString(22) != DeterministicIdentity.Sha256(reader.GetString(21)) || reader.GetString(27) != DeterministicIdentity.Sha256(reader.GetString(26)) || reader.GetString(29) != DeterministicIdentity.Sha256(reader.GetString(28)) || reader.GetString(31) != DeterministicIdentity.Sha256(reader.GetString(30)) || reader.GetInt32(23) != samples.Length || reader.GetInt32(24) != cuttings.Length || reader.GetInt32(25) != events.Length || CanonicalJson.Serialize(curves) != reader.GetString(19) || CanonicalJson.Serialize(summary) != reader.GetString(21) || CanonicalJson.Serialize(samples) != reader.GetString(26) || CanonicalJson.Serialize(cuttings) != reader.GetString(28) || CanonicalJson.Serialize(events) != reader.GetString(30) || !ValidateCombinedS5Commitment(reader.GetString(38), reader.GetString(39), reader.GetString(40), reader.GetString(41), sourceLog, reader.GetString(35), logContentHash, reader.GetString(15), artifact) || !ValidateDrillingObservationSeries(artifact, options);
            if (invalid) throw new PersistenceIntegrityException("Drilling observation artifact integrity failure.");
            return artifact with { OutputHash = reader.GetString(15) };
        }
        catch (PersistenceIntegrityException) { throw; }
        catch (Exception exception) when (exception is JsonException or OptionsValidationException or ArgumentException or InvalidOperationException) { throw new PersistenceIntegrityException("Drilling observation artifact integrity failure: " + exception.Message); }
    }

    private static bool ValidateCombinedS5Commitment(string inputJson, string inputHash, string outputJson, string outputHash, LogObservationArtifact sourceLog, string legacyLogOutputHash, string logContentHash, string drillingOutputHash, DrillingObservationArtifact drilling)
    {
        if (DeterministicIdentity.Sha256(inputJson) != inputHash || DeterministicIdentity.Sha256(outputJson) != outputHash) return false;
        using JsonDocument input = JsonDocument.Parse(inputJson); using JsonDocument output = JsonDocument.Parse(outputJson);
        if (!input.RootElement.TryGetProperty("contractVersion", out JsonElement inputVersion) || !output.RootElement.TryGetProperty("contractVersion", out JsonElement outputVersion)) return false;
        string? version = inputVersion.GetString(); if (version != outputVersion.GetString()) return false;
        if (!input.RootElement.TryGetProperty("logObservation", out JsonElement inputLog) || !output.RootElement.TryGetProperty("logObservation", out JsonElement outputLog) || !input.RootElement.TryGetProperty("drillingObservation", out JsonElement inputDrilling) || !output.RootElement.TryGetProperty("drillingObservation", out JsonElement outputDrilling)) return false;
        bool identities = inputLog.GetProperty("observationBatchId").GetString() == sourceLog.ObservationBatchId && outputLog.GetProperty("observationBatchId").GetString() == sourceLog.ObservationBatchId && inputDrilling.GetProperty("artifactId").GetString() == drilling.ArtifactId && outputDrilling.GetProperty("artifactId").GetString() == drilling.ArtifactId && inputDrilling.GetProperty("inputHash").GetString() == drilling.InputHash && inputDrilling.GetProperty("artifactHash").GetString() == drillingOutputHash && outputDrilling.GetProperty("artifactHash").GetString() == drillingOutputHash;
        if (!identities) return false;
        return version switch
        {
            "s5-combined-observations-v2" or "s5-combined-observations-v3" => inputLog.TryGetProperty("artifactHash", out JsonElement oldInputHash) && outputLog.TryGetProperty("artifactHash", out JsonElement oldOutputHash) && oldInputHash.GetString() == legacyLogOutputHash && oldOutputHash.GetString() == legacyLogOutputHash,
            CombinedS5ObservationContractVersion => inputLog.TryGetProperty("contentHash", out JsonElement stableInputHash) && outputLog.TryGetProperty("contentHash", out JsonElement stableOutputHash) && stableInputHash.GetString() == logContentHash && stableOutputHash.GetString() == logContentHash,
            _ => false
        };
    }

    public static string ComputeDrillingObservationOutputHash(DrillingObservationArtifact artifact) => DeterministicIdentity.Sha256(CanonicalDrillingObservationContent(artifact));
    private static string CanonicalDrillingObservationContent(DrillingObservationArtifact artifact) => CanonicalJson.Serialize(new
    {
        artifact.ArtifactId, artifact.RunId, artifact.ScenarioId, artifact.ExecutionId, artifact.ExecutionHash, artifact.SurveyArtifactId, artifact.SurveyHash,
        artifact.LogObservationBatchId, artifact.LogContentHash, artifact.ModelVersion, artifact.CalibrationVersion, artifact.OptionsHash, artifact.Curves, artifact.Samples,
        artifact.Cuttings, artifact.Events, artifact.Summary, artifact.InputHash
    });

    private static bool ValidateDrillingObservationSeries(DrillingObservationArtifact artifact, DrillingObservationOptions options)
    {
        if (artifact.Samples.Count is < 1 or > 10_000 || artifact.Summary.SampleCount != artifact.Samples.Count || artifact.Summary.CuttingsSampleCount != artifact.Cuttings.Count || artifact.Summary.LossEventCount != artifact.Events.Count(x => x.EventType == "Losses") || artifact.Summary.MissingSampleCount != artifact.Samples.Count(x => x.RopMPerHour is null) || artifact.Summary.DysfunctionEventCount != artifact.Events.Count(x => x.EventType != "Losses") || artifact.Curves.Count == 0 || artifact.Curves.Select(x => x.Mnemonic).Distinct(StringComparer.Ordinal).Count() != artifact.Curves.Count || artifact.Samples.Select(x => x.Ordinal).Where((x, i) => x != i).Any()) return false;
        double priorMd = double.NegativeInfinity, priorTime = double.NegativeInfinity;
        foreach (DrillingObservationSample sample in artifact.Samples)
        {
            if (sample.MeasuredDepthM <= priorMd || sample.SimulatedElapsedSeconds < priorTime || sample.QcFlags.Count == 0 || !Finite(sample.MeasuredDepthM, sample.ObservedTrueVerticalDepthM, sample.SimulatedElapsedSeconds) || !NullableFinite(sample.RopMPerHour, sample.HookloadKN, sample.SurfaceTorqueKNm, sample.DragForceKN, sample.LateralVibrationG, sample.AxialVibrationG, sample.StickSlipFraction, sample.StandpipePressurePa, sample.AnnularEcdKgM3, sample.FlowInM3PerSecond, sample.FlowOutM3PerSecond, sample.LossRateM3PerSecond, sample.DownholeTemperatureC, sample.DownholeTemperatureK, sample.RockStrengthProxyMpa) || !Between(sample.RopMPerHour, .01, 1000) || !Between(sample.HookloadKN, 0, 10_000) || !Between(sample.SurfaceTorqueKNm, 0, 1000) || !Between(sample.DragForceKN, 0, 10_000) || !Between(sample.LateralVibrationG, 0, 20) || !Between(sample.AxialVibrationG, 0, 20) || !Between(sample.StickSlipFraction, 0, 1) || !Between(sample.StandpipePressurePa, 0, 200_000_000) || !Between(sample.AnnularEcdKgM3, 500, 5000) || !Between(sample.FlowInM3PerSecond, 0, 1) || !Between(sample.FlowOutM3PerSecond, 0, 1) || !Between(sample.LossRateM3PerSecond, 0, 1) || !Between(sample.DownholeTemperatureC, -100, 350) || !Between(sample.DownholeTemperatureK, 173.15, 623.15) || (sample.DownholeTemperatureC is null) != (sample.DownholeTemperatureK is null) || sample.DownholeTemperatureC is not null && Math.Abs(sample.DownholeTemperatureK!.Value - sample.DownholeTemperatureC.Value - 273.15) > 1e-10 || sample.RopMPerHour is null && sample.DysfunctionFlags.Count != 0) return false;
            if (sample.RopMPerHour is not null)
            {
                bool loss = sample.FlowInM3PerSecond is > 0 && sample.LossRateM3PerSecond > Math.Max(1e-9, sample.FlowInM3PerSecond.Value * .001);
                if (sample.DysfunctionFlags.Contains("Losses") != loss || sample.DysfunctionFlags.Contains("StickSlip") != (sample.StickSlipFraction >= options.StickSlipThresholdFraction) || sample.DysfunctionFlags.Contains("LateralVibration") != (sample.LateralVibrationG >= options.LateralVibrationThresholdG) || sample.DysfunctionFlags.Contains("AxialVibration") != (sample.AxialVibrationG >= options.AxialVibrationThresholdG) || sample.DysfunctionFlags.Except(new[] { "Losses", "StickSlip", "LateralVibration", "AxialVibration" }, StringComparer.Ordinal).Any() || Math.Abs(sample.LossRateM3PerSecond!.Value - Math.Max(0, sample.FlowInM3PerSecond!.Value - sample.FlowOutM3PerSecond!.Value)) > 1e-12) return false;
            }
            priorMd = sample.MeasuredDepthM; priorTime = sample.SimulatedElapsedSeconds;
        }
        if (artifact.Events.Any(x => x.StartOrdinal < 0 || x.EndOrdinal < x.StartOrdinal || x.EndOrdinal >= artifact.Samples.Count || x.Qc != "ObservableThreshold" || !new[] { "Losses", "StickSlip", "LateralVibration", "AxialVibration" }.Contains(x.EventType, StringComparer.Ordinal) || x.EventId != DeterministicIdentity.Create("drilling-observation-event-v1", artifact.RunId, x.StartOrdinal.ToString(System.Globalization.CultureInfo.InvariantCulture), x.EndOrdinal.ToString(System.Globalization.CultureInfo.InvariantCulture), x.EventType) || Enumerable.Range(x.StartOrdinal, x.EndOrdinal - x.StartOrdinal + 1).Any(i => !artifact.Samples[i].DysfunctionFlags.Contains(x.EventType)) || x.StartOrdinal > 0 && artifact.Samples[x.StartOrdinal - 1].DysfunctionFlags.Contains(x.EventType) || x.EndOrdinal + 1 < artifact.Samples.Count && artifact.Samples[x.EndOrdinal + 1].DysfunctionFlags.Contains(x.EventType))) return false;
        foreach (string type in new[] { "Losses", "StickSlip", "LateralVibration", "AxialVibration" })
        {
            int expectedRuns = artifact.Samples.Select((sample, index) => (sample, index)).Count(x => x.sample.DysfunctionFlags.Contains(type) && (x.index == 0 || !artifact.Samples[x.index - 1].DysfunctionFlags.Contains(type)));
            if (artifact.Events.Count(x => x.EventType == type) != expectedRuns) return false;
        }
        if (artifact.Cuttings.Select(x => x.Ordinal).Where((x, i) => x != i).Any() || artifact.Cuttings.Any(x => !Finite(x.SurfaceArrivalSimulatedSeconds, x.ObservedSourceMeasuredDepthM, x.ObservedSourceTrueVerticalDepthM, x.DepthUncertaintyM, x.SampleMeasuredDepthM, x.RecoveredSourceMeasuredDepthM) || !NullableFinite(x.LaggedGrApi, x.RecoveryFraction) || x.DepthUncertaintyM < 0 || x.RecoveryFraction is < 0 or > 1 || x.QcFlags.Count == 0 || x.RecoveredSourceMeasuredDepthM != x.ObservedSourceMeasuredDepthM)) return false;
        double meanRop = artifact.Samples.Where(x => x.RopMPerHour is not null).Select(x => x.RopMPerHour!.Value).DefaultIfEmpty(0).Average();
        double maximumEcd = artifact.Samples.Where(x => x.AnnularEcdKgM3 is not null).Select(x => x.AnnularEcdKgM3!.Value).DefaultIfEmpty(0).Max();
        double meanTemperature = artifact.Samples.Where(x => x.DownholeTemperatureC is not null).Select(x => x.DownholeTemperatureC!.Value).DefaultIfEmpty(0).Average();
        return Finite(artifact.Summary.MeanRopMPerHour, artifact.Summary.MaximumEcdKgM3, artifact.Summary.MeanDownholeTemperatureC) && artifact.Summary.MeanRopMPerHour == meanRop && artifact.Summary.MaximumEcdKgM3 == maximumEcd && artifact.Summary.MeanDownholeTemperatureC == meanTemperature;
        static bool Finite(params double[] values) => values.All(double.IsFinite);
        static bool NullableFinite(params double?[] values) => values.All(x => x is null || double.IsFinite(x.Value));
        static bool Between(double? value, double minimum, double maximum) => value is null || value >= minimum && value <= maximum;
    }

    private const string DrillingObservationSchema = """
        CREATE TABLE IF NOT EXISTS DrillingObservationArtifacts(ArtifactId TEXT PRIMARY KEY,RunId TEXT NOT NULL UNIQUE,ScenarioId TEXT NOT NULL,ExecutionId TEXT NOT NULL,ExecutionHash TEXT NOT NULL,SurveyArtifactId TEXT NOT NULL,SurveyHash TEXT NOT NULL,LogObservationBatchId TEXT NOT NULL,LogHash TEXT NOT NULL,ModelVersion TEXT NOT NULL,CalibrationVersion TEXT NOT NULL,OptionsHash TEXT NOT NULL,CanonicalInputJson TEXT NOT NULL,InputHash TEXT NOT NULL,CanonicalOutputJson TEXT NOT NULL,OutputHash TEXT NOT NULL,CreatedUtc TEXT NOT NULL,LogContentHash TEXT NULL,FOREIGN KEY(RunId)REFERENCES Runs(RunId),FOREIGN KEY(ExecutionId)REFERENCES DrillingExecutions(ExecutionId),FOREIGN KEY(SurveyArtifactId)REFERENCES SurveyArtifacts(SurveyArtifactId),FOREIGN KEY(LogObservationBatchId)REFERENCES ObservationBatches(ObservationBatchId));
        CREATE TABLE IF NOT EXISTS DrillingObservationDetails(ArtifactId TEXT PRIMARY KEY,CanonicalOptionsJson TEXT NOT NULL,OptionsHash TEXT NOT NULL,CurvesJson TEXT NOT NULL,CurvesHash TEXT NOT NULL,SummaryJson TEXT NOT NULL,SummaryHash TEXT NOT NULL,SampleCount INTEGER NOT NULL,CuttingsCount INTEGER NOT NULL,EventCount INTEGER NOT NULL,FOREIGN KEY(ArtifactId)REFERENCES DrillingObservationArtifacts(ArtifactId));
        CREATE TABLE IF NOT EXISTS DrillingObservationSeries(ArtifactId TEXT PRIMARY KEY,SamplesJson TEXT NOT NULL,SamplesHash TEXT NOT NULL,CuttingsJson TEXT NOT NULL,CuttingsHash TEXT NOT NULL,EventsJson TEXT NOT NULL,EventsHash TEXT NOT NULL,FOREIGN KEY(ArtifactId)REFERENCES DrillingObservationArtifacts(ArtifactId));
        CREATE TRIGGER IF NOT EXISTS TR_DrillingObservationArtifacts_NoUpdate BEFORE UPDATE ON DrillingObservationArtifacts BEGIN SELECT RAISE(ABORT,'Drilling observation artifacts are immutable');END;
        CREATE TRIGGER IF NOT EXISTS TR_DrillingObservationArtifacts_NoDelete BEFORE DELETE ON DrillingObservationArtifacts BEGIN SELECT RAISE(ABORT,'Drilling observation artifacts are immutable');END;
        CREATE TRIGGER IF NOT EXISTS TR_DrillingObservationDetails_NoUpdate BEFORE UPDATE ON DrillingObservationDetails BEGIN SELECT RAISE(ABORT,'Drilling observation details are immutable');END;
        CREATE TRIGGER IF NOT EXISTS TR_DrillingObservationDetails_NoDelete BEFORE DELETE ON DrillingObservationDetails BEGIN SELECT RAISE(ABORT,'Drilling observation details are immutable');END;
        CREATE TRIGGER IF NOT EXISTS TR_DrillingObservationSeries_NoUpdate BEFORE UPDATE ON DrillingObservationSeries BEGIN SELECT RAISE(ABORT,'Drilling observation series are immutable');END;
        CREATE TRIGGER IF NOT EXISTS TR_DrillingObservationSeries_NoDelete BEFORE DELETE ON DrillingObservationSeries BEGIN SELECT RAISE(ABORT,'Drilling observation series are immutable');END;
        """;
}







