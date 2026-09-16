using Microsoft.Data.Sqlite;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Persistence;

namespace ReservoirSimulation.Tests;

[TestFixture]
[NonParallelizable]
public sealed class CompletionConnectionCapTests
{
    private string? _databasePath;

    [TearDown]
    public void Cleanup()
    {
        SqliteConnection.ClearAllPools();
        if (_databasePath is not null && File.Exists(_databasePath)) File.Delete(_databasePath);
    }

    [Test]
    public async Task Register_RejectsMoreThanTenThousandUniqueMappedConnections()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"drillsim-completion-cap-{Guid.NewGuid():N}.db");
        string connectionString = $"Data Source={_databasePath}";
        var worldRepository = new SqliteReservoirRepository(connectionString);
        await worldRepository.InitializeAsync();
        var pathRepository = new SqliteTruthSamplingRepository(connectionString);
        await pathRepository.InitializeAsync();
        var completionRepository = new SqliteCompletionBindingRepository(connectionString);
        await completionRepository.InitializeAsync();
        var worldManager = new PersistentWorldManager(
            new ReservoirWorldFactory(), new InMemoryWorldStore(2), worldRepository, TimeProvider.System);
        ReservoirWorld world = await worldManager.CreateAsync(WorldRequest());
        var pathService = new RestrictedTruthSamplingService(pathRepository, TimeProvider.System);
        ApprovedPathBindingRequest pathRequest = CapPath();
        ApprovedPathBindingMetadata path = await pathService.RegisterAsync(world, pathRequest);
        var completions = new ApprovedCompletionBindingService(
            completionRepository, pathService, TimeProvider.System);
        var request = new ApprovedCompletionBindingRequest
        {
            PathBindingId = path.BindingId,
            CompletionModelVersion = "observed-log-completion-v1",
            Openings =
            [
                new ApprovedCompletionOpening
                {
                    OpeningId = "full-grid-open-hole", ReservoirName = "Cap Sand",
                    Type = CompletionOpeningType.OpenHole,
                    TopMeasuredDepthM = 0, BaseMeasuredDepthM = pathRequest.Stations[^1].MeasuredDepthM,
                    WellboreRadiusM = 0.1, Skin = 0, Efficiency = 1, UncertaintyM = 0
                }
            ]
        };

        ReservoirValidationException? exception = Assert.ThrowsAsync<ReservoirValidationException>(
            async () => await completions.RegisterAsync(world, request));
        Assert.That(exception!.Errors.Values.SelectMany(messages => messages), Has.Some.Contains("10,000"));
    }

    private static WorldGenerationRequest WorldRequest()
    {
        ConditioningPoint Control(double easting, double northing) => new()
        {
            EastingM = easting,
            NorthingM = northing,
            ReservoirTopDepthM = 1_000,
            ReservoirBaseDepthM = 1_030,
            Porosity = 0.2,
            PermeabilityM2 = 1e-12,
            PressurePa = 20_000_000,
            WaterSaturation = 0.2,
            GasSaturation = 0.05,
            NetToGross = 1
        };
        return new WorldGenerationRequest
        {
            FieldId = Guid.Parse("4a0d4203-ec01-4bbf-8adc-4fd5941f79b5"),
            ReservoirName = "Cap Sand",
            Seed = 5,
            CalibrationArtifact = TestData.CalibrationArtifact(),
            Grid = new GridOptions { CountX = 101, CountY = 100, CountZ = 1, HorizontalPaddingM = 50 },
            Heterogeneity = TestData.ZeroHeterogeneity(),
            ConditioningPoints = [Control(0, 0), Control(10_000, 9_900)]
        };
    }

    private static ApprovedPathBindingRequest CapPath()
    {
        var stations = new List<ApprovedPathStation>(200);
        double md = 0;
        double x = 0;
        stations.Add(Station(md, x, 0));
        for (int row = 0; row < 100; row++)
        {
            double targetX = row % 2 == 0 ? 10_000 : 0;
            md += Math.Abs(targetX - x);
            x = targetX;
            stations.Add(Station(md, x, row * 100));
            if (row + 1 < 100)
            {
                md += 100;
                stations.Add(Station(md, x, (row + 1) * 100));
            }
        }
        return new ApprovedPathBindingRequest
        {
            ScenarioId = Guid.Parse("63395a4d-740e-4491-bea6-d87b918b309f"),
            RunId = Guid.Parse("884f43cd-8c97-4d47-abf8-b3ae4a5e9798"),
            PathKind = ApprovedPathKind.AsDrilled,
            ApprovedSealedPredictionSha256 =
                "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee",
            Stations = stations
        };
    }

    private static ApprovedPathStation Station(double md, double easting, double northing) => new()
    {
        MeasuredDepthM = md,
        EastingM = easting,
        NorthingM = northing,
        TrueVerticalDepthM = 1_015
    };
}
