using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NORCE.Drilling.Trajectory.Service.Managers;
using TrajectoryModel = NORCE.Drilling.Trajectory.Model.Trajectory;
using TrajectoryJson = NORCE.Drilling.Trajectory.Service.JsonSettings;

namespace DrillingOperations.Tests;

[TestFixture, NonParallelizable]
public sealed class PublicationCoordinateTests
{
    private const double OriginEast = 480000;
    private const double OriginNorth = 6700000;
    private const string TemplateMarker = "STALE-SOURCE-LOCATION";
    private PublicationArtifactSet _artifacts = null!;
    private AnalysisPackageDocument _source = null!;

    [OneTimeSetUp]
    public async Task CreatePublicationInputs()
    {
        var upstream = new PublicationFakeHandler();
        await using var factory = new ApiFactory(publication: upstream);
        using HttpClient client = factory.CreateInternalClient();
        RunResponse run = await PublicationWorkflowTests.ReadyRun(client);
        _source = WithStaleTemplate(upstream.Source);
        _artifacts = await ArtifactsAsync(factory.Services.GetRequiredService<DrillingOperationsStore>(), run);
        PlanPathStation[] planned =
        [
            new(11, 9, 120, -70), new(111, 90, 142, -15), new(211, 170, 201, 38)
        ];
        SurveyStation[] observed = planned.Select((station, index) =>
            _artifacts.Survey.Stations[0] with
            {
                MeasuredDepthM = station.MeasuredDepthM,
                ObservedEastingM = station.EastingM + .5,
                ObservedNorthingM = station.NorthingM - .25,
                ObservedTrueVerticalDepthM = station.TrueVerticalDepthM + .125,
                InclinationDegrees = 4 + index * 3,
                AzimuthDegrees = 10 + index * 15
            }).ToArray();
        _artifacts = _artifacts with
        {
            Plan = _artifacts.Plan with { Stations = planned },
            Survey = _artifacts.Survey with { Stations = observed },
            Execution = _artifacts.Execution with { Stations = [new(0, 0, -88888888, 99999999)] }
        };
    }

    [Test]
    public void NewTrajectories_HaveAbsoluteNorthEastAxes_AndFirstAuthoritativeTieIn()
    {
        string unchanged = CanonicalJson.Serialize(_source);
        PublicationStaging publication = DeterministicPublicationPlanner.Create(_source, _artifacts);
        PublicationWriteOperation[] paths = NewTrajectories(publication, _artifacts).ToArray();
        Assert.That(paths, Has.Length.EqualTo(3));
        foreach (PublicationWriteOperation operation in paths)
        {
            using JsonDocument document = JsonDocument.Parse(operation.CanonicalPayloadJson);
            JsonElement trajectory = document.RootElement;
            bool planned = trajectory.GetProperty("TrajectoryType").GetString() == "Planned";
            JsonElement[] stations = trajectory.GetProperty("SurveyStationList").EnumerateArray().ToArray();
            Assert.That(stations, Has.Length.EqualTo(3));
            for (int index = 0; index < stations.Length; index++)
            {
                double east = planned ? _artifacts.Plan.Stations[index].EastingM : _artifacts.Survey.Stations[index].ObservedEastingM;
                double north = planned ? _artifacts.Plan.Stations[index].NorthingM : _artifacts.Survey.Stations[index].ObservedNorthingM;
                double tvd = planned ? _artifacts.Plan.Stations[index].TrueVerticalDepthM : _artifacts.Survey.Stations[index].ObservedTrueVerticalDepthM;
                Assert.Multiple(() =>
                {
                    Assert.That(stations[index].GetProperty("X").GetDouble(), Is.EqualTo(OriginNorth + north));
                    Assert.That(stations[index].GetProperty("Y").GetDouble(), Is.EqualTo(OriginEast + east));
                    Assert.That(stations[index].GetProperty("MD").GetDouble(), Is.EqualTo(_artifacts.Plan.Stations[index].MeasuredDepthM));
                    Assert.That(stations[index].GetProperty("Abscissa").GetDouble(), Is.EqualTo(_artifacts.Plan.Stations[index].MeasuredDepthM));
                    Assert.That(stations[index].GetProperty("TVD").GetDouble(), Is.EqualTo(tvd));
                    Assert.That(stations[index].GetProperty("Inclination").GetDouble(),
                        Is.EqualTo(planned ? 0 : _artifacts.Survey.Stations[index].InclinationDegrees * Math.PI / 180));
                });
            }
            Assert.That(CanonicalJson.Canonicalize(trajectory.GetProperty("TieInPoint")),
                Is.EqualTo(CanonicalJson.Canonicalize(stations[0])));
            Assert.That(operation.CanonicalPayloadJson, Does.Not.Contain(TemplateMarker));
            foreach (string forbidden in new[] { "Latitude", "Longitude", "\"Z\"", "Covariance", "SurveyRunSectionList", "ReferencePoint", "-88888888", "99999999" })
                Assert.That(operation.CanonicalPayloadJson, Does.Not.Contain(forbidden));
        }
        Assert.That(paths.Count(x => JsonDocument.Parse(x.CanonicalPayloadJson).RootElement.GetProperty("IsDefinitive").GetBoolean()), Is.EqualTo(1));
        Assert.That(CanonicalJson.Serialize(_source), Is.EqualTo(unchanged), "Existing source trajectories remain unchanged.");
    }

    [Test]
    public void NewWellAndBore_UseDedicatedObservedCollar_NotAnyTemplateLocation()
    {
        PublicationStaging publication = DeterministicPublicationPlanner.Create(_source, _artifacts);
        JsonObject well = Payload(publication.Operations.Single(x => x.EntityId == _artifacts.Plan.ScenarioWellId));
        JsonObject bore = Payload(publication.Operations.Single(x => x.EntityId == _artifacts.Plan.ScenarioWellBoreId));
        Guid clusterId = PublicationJson.RequiredGuid(well, "ClusterID");
        JsonObject cluster = Payload(publication.Operations.Single(x => x.EntityId == clusterId.ToString("D")));
        JsonObject definitive = NewTrajectories(publication, _artifacts).Select(Payload).Single(x => x["IsDefinitive"]!.GetValue<bool>());
        JsonNode firstStation = definitive["SurveyStationList"]![0]!;
        JsonObject anchor = (JsonObject)cluster["ReferencePoint"]!;
        Assert.Multiple(() =>
        {
            Assert.That(publication.Operations.Count(x => x.RecordKind == "Cluster"), Is.EqualTo(_source.Clusters.Count + 1));
            Assert.That(PublicationJson.RequiredGuid(cluster, "FieldID"), Is.EqualTo(Guid.Parse(publication.ClonedFieldId)));
            Assert.That(PublicationJson.RequiredGuid(bore, "WellID"), Is.EqualTo(Guid.Parse(_artifacts.Plan.ScenarioWellId)));
            Assert.That(well["IsSingleWell"]!.GetValue<bool>(), Is.True);
            Assert.That(cluster["IsSingleWell"]!.GetValue<bool>(), Is.True);
            Assert.That(well["SlotID"], Is.Null);
            Assert.That(bore["TieInPointAlongHoleDepth"], Is.Null);
            Assert.That(bore["RigID"], Is.Null);
            Assert.That(anchor["X"]!.GetValue<double>(), Is.EqualTo(firstStation["X"]!.GetValue<double>()));
            Assert.That(anchor["Y"]!.GetValue<double>(), Is.EqualTo(firstStation["Y"]!.GetValue<double>()));
            Assert.That(anchor.Select(x => x.Key), Is.EquivalentTo(new[] { "X", "Y" }));
            Assert.That(anchor["X"]!.GetValue<double>(), Is.EqualTo(OriginNorth + _artifacts.Survey.Stations[0].ObservedNorthingM));
            Assert.That(anchor["Y"]!.GetValue<double>(), Is.EqualTo(OriginEast + _artifacts.Survey.Stations[0].ObservedEastingM));
        });
        foreach (JsonObject path in NewTrajectories(publication, _artifacts).Select(Payload))
            Assert.That(PublicationJson.RequiredGuid(path, "ClusterID"), Is.EqualTo(clusterId));
        foreach (JsonObject entity in new[] { well, bore, cluster })
        {
            Assert.That(entity.ToJsonString(), Does.Not.Contain(TemplateMarker).And.Not.Contain("\"Latitude\"").And.Not.Contain("\"Longitude\""));
            Assert.That(entity["Description"]!.GetValue<string>(), Does.Contain(DeterministicPublicationPlanner.CoordinatePublicationVersion));
        }
        Assert.That(well["SurfacePosition"], Is.Null);
        Assert.That(bore["Collar"], Is.Null);
        JsonObject copiedSourceCluster = Payload(publication.Operations.First(x => x.RecordKind == "Cluster" && x.EntityId != clusterId.ToString("D")));
        Assert.That(copiedSourceCluster.ToJsonString(), Does.Contain(TemplateMarker), "Source records are cloned, not relocated.");
    }

    [Test]
    public void RealWellBoreAndHorizontalPointSerialization_PreserveNewSurfaceProjection()
    {
        PublicationStaging publication = DeterministicPublicationPlanner.Create(_source, _artifacts);
        PublicationWriteOperation wellOperation = publication.Operations.Single(x => x.EntityId == _artifacts.Plan.ScenarioWellId);
        PublicationWriteOperation boreOperation = publication.Operations.Single(x => x.EntityId == _artifacts.Plan.ScenarioWellBoreId);
        var well = JsonSerializer.Deserialize<OSDC.Drilling.Well.Model.Well>(wellOperation.CanonicalPayloadJson, TrajectoryJson.Options)!;
        var bore = JsonSerializer.Deserialize<NORCE.Drilling.WellBore.Model.WellBore>(boreOperation.CanonicalPayloadJson, TrajectoryJson.Options)!;
        foreach (var pair in new[]
        {
            (Expected: wellOperation.CanonicalPayloadJson, Actual: JsonSerializer.Serialize(well, TrajectoryJson.Options)),
            (Expected: boreOperation.CanonicalPayloadJson, Actual: JsonSerializer.Serialize(bore, TrajectoryJson.Options))
        })
        {
            using JsonDocument expected = JsonDocument.Parse(pair.Expected);
            using JsonDocument actual = JsonDocument.Parse(pair.Actual);
            Assert.That(PublicationJson.BusinessContentMatches(expected.RootElement, actual.RootElement), Is.True, pair.Actual);
        }
        JsonObject cluster = Payload(publication.Operations.Single(x => x.EntityId == well.ClusterID!.Value.ToString("D")));
        var point = JsonSerializer.Deserialize<OSDC.DotnetLibraries.General.Math.Point3DGlobalCoordinates>(
            cluster["ReferencePoint"]!.ToJsonString(), TrajectoryJson.Options)!;
        Assert.That(point.TVD, Is.Null, "Observed TVD is not a ground-level or WGS84 elevation.");
        Assert.That(point.RiemannianNorth, Is.EqualTo(OriginNorth + _artifacts.Survey.Stations[0].ObservedNorthingM));
        Assert.That(point.RiemannianEast, Is.EqualTo(OriginEast + _artifacts.Survey.Stations[0].ObservedEastingM));
        JsonObject enriched = (JsonObject)cluster.DeepClone();
        enriched["ReferencePoint"] = JsonSerializer.SerializeToNode(point, TrajectoryJson.Options);
        using JsonDocument original = JsonDocument.Parse(cluster.ToJsonString());
        using JsonDocument imported = JsonDocument.Parse(enriched.ToJsonString());
        Assert.That(PublicationJson.BusinessContentMatches(original.RootElement, imported.RootElement), Is.True);
    }

    [TestCase("X", 0)]
    [TestCase("x", 0)]
    [TestCase("RiemannianEast", 0)]
    [TestCase("Z", 10)]
    [TestCase("TVD", 10)]
    [TestCase("Latitude", 1000)]
    [TestCase("UnknownPosition", 1)]
    public void HorizontalCollarReadback_RejectsWrongAliasesOrInventedVerticalDatum(string property, double value)
    {
        using JsonDocument expected = JsonDocument.Parse("""{"ReferencePoint":{"X":6700000,"Y":480000}}""");
        JsonNode actual = JsonNode.Parse("""{"ReferencePoint":{"X":6700000,"Y":480000,"RiemannianNorth":6700000,"RiemannianEast":480000}}""")!;
        actual["ReferencePoint"]![property] = value;
        using JsonDocument changed = JsonDocument.Parse(actual.ToJsonString());
        Assert.That(PublicationJson.BusinessContentMatches(expected.RootElement, changed.RootElement), Is.False);
    }

    [Test]
    public void RealTrajectoryImportSerialization_RetainsAbsoluteCoordinatesAndMatchingTieIn()
    {
        PublicationStaging publication = DeterministicPublicationPlanner.Create(_source, _artifacts);
        foreach (PublicationWriteOperation operation in NewTrajectories(publication, _artifacts))
        {
            TrajectoryModel trajectory = JsonSerializer.Deserialize<TrajectoryModel>(operation.CanonicalPayloadJson, TrajectoryJson.Options)!;
            Assert.That(trajectory.TieInPoint, Is.Not.Null);
            for (int roundtrip = 0; roundtrip < 3; roundtrip++)
            {
                var stations = trajectory.SurveyStationList!;
                Assert.That(trajectory.TieInPoint!.MD, Is.EqualTo(stations[0].MD));
                Assert.That(trajectory.TieInPoint.RiemannianNorth, Is.EqualTo(stations[0].RiemannianNorth));
                Assert.That(trajectory.TieInPoint.RiemannianEast, Is.EqualTo(stations[0].RiemannianEast));
                Assert.That(trajectory.TieInPoint.TVD, Is.EqualTo(stations[0].TVD));
                Assert.That(stations[0].X, Is.EqualTo(stations[0].RiemannianNorth));
                Assert.That(stations[0].Y, Is.EqualTo(stations[0].RiemannianEast));
                Assert.That(stations[0].Z, Is.EqualTo(stations[0].TVD));
                Assert.That(stations[0].RiemannianNorth, Is.GreaterThan(6_000_000));
                Assert.That(stations[0].RiemannianEast, Is.GreaterThan(400_000));
                string readback = JsonSerializer.Serialize(trajectory, TrajectoryJson.Options);
                using JsonDocument expected = JsonDocument.Parse(operation.CanonicalPayloadJson);
                using JsonDocument actual = JsonDocument.Parse(readback);
                Assert.That(PublicationJson.BusinessContentMatches(expected.RootElement, actual.RootElement), Is.True, readback);
                trajectory = JsonSerializer.Deserialize<TrajectoryModel>(readback, TrajectoryJson.Options)!;
            }
        }
    }

    [Test]
    public void RealTrajectoryStationChunkStorage_ReadbackPreservesCoordinatesAndPublicationCommitments()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        string root = Path.Combine(Path.GetTempPath(), $"drillsim-coordinate-import-{Guid.NewGuid():N}");
        string working = Path.Combine(root, "work");
        Directory.CreateDirectory(working);
        try
        {
            // The real connection manager creates ../home even with an explicit connection string.
            Directory.SetCurrentDirectory(working);
            string connectionString = $"Data Source={Path.Combine(root, "trajectory-test.db")};Pooling=False";
            var manager = new SqlConnectionManagerTrajectory(
                connectionString,
                NullLogger<SqlConnectionManagerTrajectory>.Instance);
            PublicationStaging publication = DeterministicPublicationPlanner.Create(_source, _artifacts);
            foreach (PublicationWriteOperation operation in NewTrajectories(publication, _artifacts))
            {
                TrajectoryModel imported = JsonSerializer.Deserialize<TrajectoryModel>(operation.CanonicalPayloadJson, TrajectoryJson.Options)!;
                Guid id = imported.MetaInfo!.ID;
                var stations = imported.SurveyStationList!;
                imported.SurveyStationList = null;
                string metadata = JsonSerializer.Serialize(imported, TrajectoryJson.Options);
                using (SqliteConnection connection = manager.GetConnection()!)
                using (SqliteTransaction transaction = connection.BeginTransaction())
                {
                    using SqliteCommand insert = connection.CreateCommand();
                    insert.Transaction = transaction;
                    insert.CommandText = "INSERT INTO TrajectoryTable(ID,Trajectory) VALUES($id,$json);";
                    insert.Parameters.AddWithValue("$id", id.ToString());
                    insert.Parameters.AddWithValue("$json", metadata);
                    insert.ExecuteNonQuery();
                    Assert.That(SurveyStationChunkStore.ReplaceChunks(connection, transaction, id, "Trajectory", stations), Is.True);
                    transaction.Commit();
                }
                string stored;
                using (SqliteConnection connection = manager.GetConnection()!)
                {
                    using SqliteCommand read = connection.CreateCommand();
                    read.CommandText = "SELECT Trajectory FROM TrajectoryTable WHERE ID=$id;";
                    read.Parameters.AddWithValue("$id", id.ToString());
                    stored = (string)read.ExecuteScalar()!;
                }
                TrajectoryModel readback = JsonSerializer.Deserialize<TrajectoryModel>(stored, TrajectoryJson.Options)!;
                readback.SurveyStationList = SurveyStationChunkStore.GetStations(NullLogger.Instance, manager, id, "Trajectory");
                Assert.That(readback.SurveyStationList, Has.Count.EqualTo(stations.Count));
                Assert.That(readback.TieInPoint!.RiemannianNorth, Is.EqualTo(readback.SurveyStationList![0].RiemannianNorth));
                Assert.That(readback.TieInPoint.RiemannianEast, Is.EqualTo(readback.SurveyStationList[0].RiemannianEast));
                using JsonDocument expected = JsonDocument.Parse(operation.CanonicalPayloadJson);
                using JsonDocument actual = JsonSerializer.SerializeToDocument(readback, TrajectoryJson.Options);
                Assert.That(PublicationJson.BusinessContentMatches(expected.RootElement, actual.RootElement), Is.True);
            }
            var restarted = new SqlConnectionManagerTrajectory(connectionString, NullLogger<SqlConnectionManagerTrajectory>.Instance);
            using var reopened = restarted.GetConnection()!;
            using var count = reopened.CreateCommand();
            count.CommandText = "SELECT COUNT(*) FROM TrajectoryTable;";
            Assert.That(count.ExecuteScalar(), Is.EqualTo(3L));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            SqliteConnection.ClearAllPools();
            Directory.Delete(root, recursive: true);
        }
    }

    [TestCase("null")]
    [TestCase("{}")]
    [TestCase("{\"Latitude\":1.05,\"Longitude\":0.02}")]
    [TestCase("{\"RiemannianNorth\":6700000}")]
    [TestCase("{\"RiemannianNorth\":6700000,\"RiemannianEast\":null}")]
    [TestCase("{\"RiemannianNorth\":6700000,\"RiemannianEast\":\"480000\"}")]
    [TestCase("{\"RiemannianNorth\":1e400,\"RiemannianEast\":480000}")]
    [TestCase("{\"RiemannianNorth\":6700000,\"RiemannianEast\":480000,\"X\":6700001}")]
    [TestCase("{\"RiemannianNorth\":6700000,\"RiemannianEast\":480000,\"Y\":480001}")]
    [TestCase("{\"RiemannianNorth\":6700000,\"RiemannianEast\":480000,\"riemannianEast\":480001}")]
    public void InvalidFieldOrigin_IsRejectedWithoutTranslationGuess(string origin)
    {
        AnalysisPackageDocument source = WithOrigin(_source, JsonNode.Parse(origin));
        PublicationUpstreamException exception = Assert.Throws<PublicationUpstreamException>(() =>
            DeterministicPublicationPlanner.Create(source, _artifacts))!;
        Assert.That(exception.StatusCode, Is.EqualTo(409));
        Assert.That(exception.DiagnosticCode, Is.EqualTo("PublishedTrajectoryOriginInvalid"));
    }

    [Test]
    public void InvalidLocalStations_AreRejectedWithoutInventingCoordinatesOrDepth()
    {
        foreach (IReadOnlyList<PlanPathStation> stations in new IReadOnlyList<PlanPathStation>[]
        {
            [],
            [_artifacts.Plan.Stations[0]],
            [_artifacts.Plan.Stations[0], _artifacts.Plan.Stations[0]],
            [_artifacts.Plan.Stations[0] with { EastingM = double.NaN }, _artifacts.Plan.Stations[1]],
            [_artifacts.Plan.Stations[0] with { TrueVerticalDepthM = -1 }, _artifacts.Plan.Stations[1]]
        })
        {
            PublicationUpstreamException exception = Assert.Throws<PublicationUpstreamException>(() =>
                DeterministicPublicationPlanner.Create(_source, _artifacts with { Plan = _artifacts.Plan with { Stations = stations } }))!;
            Assert.That(exception.DiagnosticCode, Is.EqualTo("PublishedTrajectoryCoordinatesInvalid"));
        }
    }

    [Test]
    public void VerifiedMetricAliasesAndCanonicalOrigin_ProduceIdenticalStationCoordinates()
    {
        PublicationStaging canonical = DeterministicPublicationPlanner.Create(_source, _artifacts);
        PublicationStaging aliases = DeterministicPublicationPlanner.Create(
            WithOrigin(_source, new JsonObject { ["X"] = OriginNorth, ["Y"] = OriginEast }), _artifacts);
        Assert.That(NewTrajectories(aliases, _artifacts).Select(x => x.CanonicalPayloadJson),
            Is.EqualTo(NewTrajectories(canonical, _artifacts).Select(x => x.CanonicalPayloadJson)));
    }

    [Test]
    public void DeterministicHashes_ChangeForOriginOrObservablePath_NotUnpublishedExecutionCoordinates()
    {
        PublicationStaging first = DeterministicPublicationPlanner.Create(_source, _artifacts);
        PublicationStaging repeat = DeterministicPublicationPlanner.Create(_source, _artifacts);
        Assert.That(CanonicalJson.Serialize(repeat), Is.EqualTo(CanonicalJson.Serialize(first)));
        foreach (PublicationWriteOperation operation in first.Operations)
            Assert.That(operation.PayloadHash, Is.EqualTo(DeterministicIdentity.Sha256(operation.CanonicalPayloadJson)));
        PublicationStaging moved = DeterministicPublicationPlanner.Create(
            WithOrigin(_source, new JsonObject { ["RiemannianNorth"] = OriginNorth + 1, ["RiemannianEast"] = OriginEast }), _artifacts);
        Assert.That(moved.PublicationPlanId, Is.Not.EqualTo(first.PublicationPlanId));
        Assert.That(moved.ManifestHash, Is.Not.EqualTo(first.ManifestHash));
        PublicationStaging hiddenChange = DeterministicPublicationPlanner.Create(_source,
            _artifacts with { Execution = _artifacts.Execution with { Stations = [new(100, 99, 123456789, -987654321)] } });
        Assert.That(CanonicalJson.Serialize(hiddenChange), Is.EqualTo(CanonicalJson.Serialize(first)));
    }

    [Test]
    public void TieInModelEnrichment_IsNarrow_AndRejectsWrongAxesDepthOrUnapprovedFields()
    {
        PublicationWriteOperation path = NewTrajectories(DeterministicPublicationPlanner.Create(_source, _artifacts), _artifacts).First();
        var actual = JsonSerializer.Deserialize<TrajectoryModel>(path.CanonicalPayloadJson, TrajectoryJson.Options)!;
        string json = JsonSerializer.Serialize(actual, TrajectoryJson.Options);
        using JsonDocument expected = JsonDocument.Parse(path.CanonicalPayloadJson);
        foreach (string property in new[] { "RiemannianNorth", "RiemannianEast", "Z", "MD", "Latitude", "ExtraCoordinate" })
        {
            JsonObject changed = (JsonObject)JsonNode.Parse(json)!;
            ((JsonObject)changed["TieInPoint"]!)[property] = property == "Latitude" ? 1000 : -12345;
            using JsonDocument observed = JsonDocument.Parse(changed.ToJsonString());
            Assert.That(PublicationJson.BusinessContentMatches(expected.RootElement, observed.RootElement), Is.False, property);
        }
    }

    [Test]
    public async Task ExistingLegacyStaging_ReplaysAndPublishesWithoutCoordinateRewriteOrSourceFetch()
    {
        var upstream = new PublicationFakeHandler();
        await using var factory = new ApiFactory(publication: upstream);
        using HttpClient client = factory.CreateInternalClient();
        RunResponse run = await PublicationWorkflowTests.ReadyRun(client);
        var store = factory.Services.GetRequiredService<DrillingOperationsStore>();
        PublicationArtifactSet artifacts = await ArtifactsAsync(store, run);
        PublicationStaging current = DeterministicPublicationPlanner.Create(upstream.Source, artifacts);
        PublicationStaging legacy = LegacyCoordinates(current, artifacts);
        Assert.That(current.Operations.Count, Is.EqualTo(223));
        Assert.That(legacy.Operations.Count, Is.EqualTo(222));
        await store.StagePublicationAsync(legacy);
        string oldPayloads = CanonicalJson.Serialize(legacy.Operations);
        var restart = new DrillingOperationsStore($"Data Source={factory.DatabasePath}", TimeProvider.System);
        await restart.InitializeAsync();
        PublicationStaging read = (await restart.GetPublicationAsync(run.RunId))!;
        Assert.That(read.ManifestJson, Is.EqualTo(legacy.ManifestJson));
        Assert.That(read.ManifestHash, Is.EqualTo(legacy.ManifestHash));
        Assert.That(CanonicalJson.Serialize(read.Operations), Is.EqualTo(oldPayloads));
        PublicationStaging replay = await restart.StagePublicationAsync(current);
        Assert.That(replay.PublicationPlanId, Is.EqualTo(legacy.PublicationPlanId));
        Assert.That(CanonicalJson.Serialize(replay.Operations), Is.EqualTo(oldPayloads));
        upstream.SourceUnavailable = true;
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/drillingoperations/api/runs/{run.RunId}/publish");
        request.Headers.Add("Idempotency-Key", "legacy-coordinate-publication");
        using HttpResponseMessage response = await client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await response.Content.ReadAsStringAsync());
        PublicationStaging revealed = (await restart.GetPublicationAsync(run.RunId))!;
        Assert.That(CanonicalJson.Serialize(revealed.Operations), Is.EqualTo(oldPayloads));
        string finalManifest = revealed.ManifestJson;
        await restart.InitializeAsync();
        Assert.That((await restart.GetPublicationAsync(run.RunId))!.ManifestJson, Is.EqualTo(finalManifest));
        Assert.That(await restart.GetSideEffectCountsAsync(run.RunId), Is.EqualTo((1, 1)));
    }

    private static AnalysisPackageDocument WithOrigin(AnalysisPackageDocument source, JsonNode? origin)
    {
        var field = (JsonObject)JsonNode.Parse(source.Field.GetRawText())!;
        PublicationJson.Set(field, "ReferencePoint", origin);
        return source with { Field = JsonSerializer.SerializeToElement(field, CanonicalJson.SerializerOptions) };
    }

    private static AnalysisPackageDocument WithStaleTemplate(AnalysisPackageDocument source)
    {
        var trajectory = (JsonObject)JsonNode.Parse(source.Trajectories[0].GetRawText())!;
        trajectory["TieInPoint"] = new JsonObject
        {
            ["MD"] = 324, ["Abscissa"] = 324, ["X"] = 6123456, ["Y"] = 234567,
            ["Z"] = 9999, ["TVD"] = 9999, ["Latitude"] = 1.2, ["Longitude"] = -.3,
            ["Covariance"] = new JsonObject { ["Marker"] = TemplateMarker }
        };
        trajectory["SurveyRunSectionList"] = new JsonArray(new JsonObject { ["SurveyRunID"] = Guid.NewGuid(), ["StartAbscissa"] = 324 });
        trajectory["ReferencePoint"] = new JsonObject { ["Latitude"] = 1.2, ["Longitude"] = -.3, ["Marker"] = TemplateMarker };
        var well = (JsonObject)JsonNode.Parse(source.Wells[0].GetRawText())!;
        well["SurfacePosition"] = new JsonObject { ["X"] = 123, ["Y"] = 456, ["Marker"] = TemplateMarker };
        well["Latitude"] = 1.2; well["Longitude"] = -.3;
        var bore = (JsonObject)JsonNode.Parse(source.WellBores[0].GetRawText())!;
        bore["Collar"] = new JsonObject { ["Latitude"] = 1.2, ["Longitude"] = -.3, ["Marker"] = TemplateMarker };
        bore["TieInPointAlongHoleDepth"] = new JsonObject { ["Mean"] = 324 };
        bore["RigID"] = Guid.NewGuid();
        var cluster = (JsonObject)JsonNode.Parse(source.Clusters[0].GetRawText())!;
        cluster["ReferencePoint"] = new JsonObject { ["X"] = 123, ["Y"] = 456, ["Marker"] = TemplateMarker };
        return source with
        {
            Trajectories = source.Trajectories.Select((value, index) => index == 0
                ? JsonSerializer.SerializeToElement(trajectory, CanonicalJson.SerializerOptions) : value).ToArray(),
            Wells = source.Wells.Select((value, index) => index == 0
                ? JsonSerializer.SerializeToElement(well, CanonicalJson.SerializerOptions) : value).ToArray(),
            WellBores = source.WellBores.Select((value, index) => index == 0
                ? JsonSerializer.SerializeToElement(bore, CanonicalJson.SerializerOptions) : value).ToArray(),
            Clusters = source.Clusters.Select((value, index) => index == 0
                ? JsonSerializer.SerializeToElement(cluster, CanonicalJson.SerializerOptions) : value).ToArray()
        };
    }

    private static JsonObject Payload(PublicationWriteOperation operation) => (JsonObject)JsonNode.Parse(operation.CanonicalPayloadJson)!;

    private static IEnumerable<PublicationWriteOperation> NewTrajectories(PublicationStaging publication, PublicationArtifactSet artifacts)
    {
        HashSet<string> ids =
        [
            artifacts.Plan.PlannedTrajectoryId,
            DeterministicIdentity.Create("published-as-drilled-observation-v1", artifacts.Scenario.ScenarioId.ToString("D"), artifacts.Plan.RunId),
            DeterministicIdentity.Create("published-definitive-survey-v1", artifacts.Scenario.ScenarioId.ToString("D"), artifacts.Plan.RunId)
        ];
        return publication.Operations.Where(x => x.RecordKind == "Trajectory" && ids.Contains(x.EntityId));
    }

    private static async Task<PublicationArtifactSet> ArtifactsAsync(DrillingOperationsStore store, RunResponse run) =>
        new((await store.GetBindingAsync(run.ScenarioId))!, (await store.GetMaterializedPlanAsync(run.RunId))!,
            (await store.GetDrillingExecutionAsync(run.RunId))!, (await store.GetSurveyArtifactAsync(run.RunId))!,
            (await store.GetLogObservationBatchAsync(run.RunId))!, (await store.GetCompletionDesignAsync(run.RunId))!,
            (await store.GetProductionSeriesAsync(run.RunId))!, TestData.ScenarioSnapshot(run.ScenarioId));

    private static PublicationStaging LegacyCoordinates(PublicationStaging current, PublicationArtifactSet artifacts)
    {
        HashSet<string> newIds = NewTrajectories(current, artifacts).Select(x => x.EntityId).ToHashSet();
        Guid collarId = PublicationJson.RequiredGuid(Payload(current.Operations.Single(x => x.EntityId == artifacts.Plan.ScenarioWellId)), "ClusterID");
        string originalClusterId = current.Operations.First(x => x.RecordKind == "Cluster" && x.EntityId != collarId.ToString("D")).EntityId;
        PublicationWriteOperation[] operations = current.Operations.Where(x => x.EntityId != collarId.ToString("D")).Select(operation =>
        {
            bool trajectory = newIds.Contains(operation.EntityId);
            if (!trajectory && operation.EntityId != artifacts.Plan.ScenarioWellId && operation.EntityId != artifacts.Plan.ScenarioWellBoreId) return operation;
            var payload = (JsonObject)JsonNode.Parse(operation.CanonicalPayloadJson)!;
            payload["Description"] = payload["Description"]!.GetValue<string>()
                .Replace(DeterministicPublicationPlanner.CoordinatePublicationVersion + "; ", string.Empty, StringComparison.Ordinal);
            if (trajectory || operation.EntityId == artifacts.Plan.ScenarioWellId) payload["ClusterID"] = originalClusterId;
            if (operation.EntityId == artifacts.Plan.ScenarioWellId) payload["IsSingleWell"] = false;
            if (trajectory)
            {
                var stations = (JsonArray)payload["SurveyStationList"]!;
                double firstNorth = stations[0]!["X"]!.GetValue<double>(), firstEast = stations[0]!["Y"]!.GetValue<double>();
                foreach (JsonObject station in stations.Cast<JsonObject>())
                {
                    double north = station["X"]!.GetValue<double>(), east = station["Y"]!.GetValue<double>();
                    station["X"] = east - firstEast;
                    station["Y"] = north - firstNorth;
                }
                payload["TieInPoint"] = new JsonObject { ["MD"] = 324, ["X"] = 6700000, ["Y"] = 480000 };
            }
            string canonical = CanonicalJson.Serialize(payload);
            string hash = DeterministicIdentity.Sha256(canonical);
            return operation with
            {
                CanonicalPayloadJson = canonical, PayloadHash = hash,
                OperationId = DeterministicIdentity.Create("publication-operation-v1", current.ScenarioId, operation.RecordKind, operation.EntityId, hash)
            };
        }).Select((operation, index) => operation with { Sequence = index + 1 }).ToArray();
        PublicationEvidence[] evidence = operations.Select(x => new PublicationEvidence(x.EntityId, x.RecordKind, x.PayloadHash))
            .OrderBy(x => x.RecordKind, StringComparer.Ordinal).ThenBy(x => x.EvidenceId, StringComparer.Ordinal).ToArray();
        var manifest = (JsonObject)JsonNode.Parse(current.ManifestJson)!;
        manifest["evidence"] = JsonSerializer.SerializeToNode(evidence, CanonicalJson.SerializerOptions);
        string canonicalManifest = CanonicalJson.Serialize(manifest);
        return current with
        {
            Operations = operations, Evidence = evidence, ManifestJson = canonicalManifest,
            ManifestHash = DeterministicIdentity.Sha256(canonicalManifest),
            PublicationPlanId = DeterministicIdentity.Create("publication-plan-v1", current.ScenarioId, current.RunId,
                current.ClonedFieldId, string.Join("\n", operations.Select(x => x.PayloadHash)))
        };
    }
}
