using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;

namespace ReservoirSimulation.Tests;

internal static class TestData
{
    internal static WorldGenerationRequest ConditionedRequest(int seed = 1234) => new()
    {
        FieldId = Guid.Parse("72cf31ce-5d1a-44ca-a7cd-d3d2bfb34eb0"),
        ReservoirName = "Test Sand",
        Seed = seed,
        CalibrationArtifact = CalibrationArtifact(),
        Grid = new GridOptions { CountX = 8, CountY = 8, CountZ = 3, HorizontalPaddingM = 50 },
        Heterogeneity = new HeterogeneityOptions
        {
            IdwPower = 2,
            SpectralModeCount = 16,
            CorrelationLengthXM = 250,
            CorrelationLengthYM = 250,
            CorrelationLengthZM = 15,
            ControlFadeDistanceM = 200,
            TopDepthStdDevM = 3,
            BaseDepthStdDevM = 3,
            PorosityStdDev = 0.02,
            LogPermeabilityStdDev = 0.5,
            PressureStdDevPa = 300_000,
            WaterSaturationStdDev = 0.02,
            GasSaturationStdDev = 0.01
        },
        ConditioningPoints =
        [
            new ConditioningPoint
            {
                EastingM = 0, NorthingM = 0,
                ReservoirTopDepthM = 1_000, ReservoirBaseDepthM = 1_030,
                Porosity = 0.20, PermeabilityM2 = 8e-13, PressurePa = 20_000_000,
                WaterSaturation = 0.20, GasSaturation = 0.05
            },
            new ConditioningPoint
            {
                EastingM = 700, NorthingM = 700,
                ReservoirTopDepthM = 1_020, ReservoirBaseDepthM = 1_065,
                Porosity = 0.28, PermeabilityM2 = 2e-12, PressurePa = 23_000_000,
                WaterSaturation = 0.32, GasSaturation = 0.08
            }
        ]
    };

    internal static ReservoirWorld UniformWorld(int countX = 4, int countY = 3, int countZ = 1) =>
        new ReservoirWorldFactory().Create(UniformWorldRequest(countX, countY, countZ));

    internal static WorldGenerationRequest UniformWorldRequest(
        int countX = 4,
        int countY = 3,
        int countZ = 1) => new()
        {
            FieldId = Guid.Parse("bcb11c53-7c77-473a-bf9e-538c04bc52b9"),
            ReservoirName = "Uniform Sand",
            Seed = 99,
            CalibrationArtifact = CalibrationArtifact(),
            Grid = new GridOptions
            {
                CountX = countX,
                CountY = countY,
                CountZ = countZ,
                HorizontalPaddingM = 50
            },
            Heterogeneity = ZeroHeterogeneity(),
            ConditioningPoints =
            [
                UniformControl(0, 0),
                UniformControl(700, 0)
            ]
        };

    internal static CalibrationArtifactReference CalibrationArtifact(
        string sha256 = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa") => new()
        {
            Id = "test-calibration-v1",
            Sha256 = sha256
        };


    internal static HeterogeneityOptions ZeroHeterogeneity() => new()
    {
        IdwPower = 2,
        SpectralModeCount = 8,
        CorrelationLengthXM = 200,
        CorrelationLengthYM = 200,
        CorrelationLengthZM = 20,
        ControlFadeDistanceM = 100,
        TopDepthStdDevM = 0,
        BaseDepthStdDevM = 0,
        PorosityStdDev = 0,
        LogPermeabilityStdDev = 0,
        PressureStdDevPa = 0,
        WaterSaturationStdDev = 0,
        GasSaturationStdDev = 0
    };

    private static ConditioningPoint UniformControl(double easting, double northing) => new()
    {
        EastingM = easting,
        NorthingM = northing,
        ReservoirTopDepthM = 1_000,
        ReservoirBaseDepthM = 1_030,
        Porosity = 0.20,
        PermeabilityM2 = 1e-12,
        PressurePa = 25_000_000,
        WaterSaturation = 0.20,
        GasSaturation = 0.05
    };
}
