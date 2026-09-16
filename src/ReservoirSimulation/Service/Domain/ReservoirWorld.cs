using ReservoirSimulation.Contracts;

namespace ReservoirSimulation.Domain;

internal readonly record struct GridGeometry(
    int CountX,
    int CountY,
    int CountZ,
    double OriginEastingM,
    double OriginNorthingM,
    double CellSizeXM,
    double CellSizeYM)
{
    internal int CellCount => CountX * CountY * CountZ;
    internal int ColumnCount => CountX * CountY;
    internal int ColumnIndex(int i, int j) => j * CountX + i;
    internal int CellIndex(int i, int j, int k) => (k * CountY + j) * CountX + i;
    internal double Easting(int i) => OriginEastingM + (i + 0.5) * CellSizeXM;
    internal double Northing(int j) => OriginNorthingM + (j + 0.5) * CellSizeYM;
}

internal readonly record struct GridFace(
    int CellA,
    int CellB,
    double GeometricTransmissibility,
    double DepthDifferenceM);

internal sealed class ReservoirWorld
{
    internal ReservoirWorld(
        WorldSummary summary,
        bool hasExplicitFluidContacts,
        GridGeometry grid,
        double[] topDepthM,
        double[] baseDepthM,
        double[] cellDepthM,
        double[] cellThicknessM,
        double[] netToGross,
        double[] porosity,
        double[] logPermeability,
        double[] pressurePa,
        double[] oilSaturation,
        double[] waterSaturation,
        double[] gasSaturation)
    {
        Summary = summary;
        HasExplicitFluidContacts = hasExplicitFluidContacts;
        Grid = grid;
        TopDepthM = topDepthM;
        BaseDepthM = baseDepthM;
        CellDepthM = cellDepthM;
        CellThicknessM = cellThicknessM;
        NetToGross = netToGross;
        Porosity = porosity;
        LogPermeability = logPermeability;
        PermeabilityM2 = logPermeability.Select(Math.Exp).ToArray();
        HorizontalPermeabilityXM2 = (double[])PermeabilityM2.Clone();
        HorizontalPermeabilityYM2 = (double[])PermeabilityM2.Clone();
        PressurePa = pressurePa;
        OilSaturation = oilSaturation;
        WaterSaturation = waterSaturation;
        GasSaturation = gasSaturation;
        PoreVolumeM3 = BuildPoreVolumes();
        Faces = BuildFaces();
        (FaceOffsets, IncidentFaceIndices) = BuildFaceTopology();
        CellColors = BuildCellColors();
    }

    internal WorldSummary Summary { get; }
    internal bool HasExplicitFluidContacts { get; }
    internal GridGeometry Grid { get; }
    internal double[] TopDepthM { get; }
    internal double[] BaseDepthM { get; }
    internal double[] CellDepthM { get; }
    internal double[] CellThicknessM { get; }
    internal double[] NetToGross { get; }
    internal double[] Porosity { get; }
    internal double[] LogPermeability { get; }
    internal double[] PermeabilityM2 { get; }
    internal double[] HorizontalPermeabilityXM2 { get; }
    internal double[] HorizontalPermeabilityYM2 { get; }
    internal double[] PressurePa { get; }
    internal double[] OilSaturation { get; }
    internal double[] WaterSaturation { get; }
    internal double[] GasSaturation { get; }
    internal double[] PoreVolumeM3 { get; }
    internal GridFace[] Faces { get; }
    internal int[] FaceOffsets { get; }
    internal int[] IncidentFaceIndices { get; }
    internal byte[] CellColors { get; }

    private double[] BuildPoreVolumes()
    {
        var values = new double[Grid.CellCount];
        double horizontalArea = Grid.CellSizeXM * Grid.CellSizeYM;
        for (int cell = 0; cell < values.Length; cell++)
            values[cell] = horizontalArea * CellThicknessM[cell] * Porosity[cell];
        return values;
    }

    private byte[] BuildCellColors()
    {
        var colors = new byte[Grid.CellCount];
        for (int k = 0; k < Grid.CountZ; k++)
        for (int j = 0; j < Grid.CountY; j++)
        for (int i = 0; i < Grid.CountX; i++)
            colors[Grid.CellIndex(i, j, k)] = (byte)((i + j + k) & 1);
        return colors;
    }


    private (int[] Offsets, int[] FaceIndices) BuildFaceTopology()
    {
        var counts = new int[Grid.CellCount];
        foreach (GridFace face in Faces)
        {
            counts[face.CellA]++;
            counts[face.CellB]++;
        }
        var offsets = new int[Grid.CellCount + 1];
        for (int cell = 0; cell < counts.Length; cell++)
            offsets[cell + 1] = offsets[cell] + counts[cell];
        var positions = (int[])offsets.Clone();
        var indices = new int[2 * Faces.Length];
        for (int faceIndex = 0; faceIndex < Faces.Length; faceIndex++)
        {
            GridFace face = Faces[faceIndex];
            indices[positions[face.CellA]++] = faceIndex;
            indices[positions[face.CellB]++] = faceIndex;
        }
        return (offsets, indices);
    }


    private GridFace[] BuildFaces()
    {
        int expected = (Grid.CountX - 1) * Grid.CountY * Grid.CountZ +
            Grid.CountX * (Grid.CountY - 1) * Grid.CountZ +
            Grid.CountX * Grid.CountY * (Grid.CountZ - 1);
        var faces = new List<GridFace>(expected);

        for (int k = 0; k < Grid.CountZ; k++)
        for (int j = 0; j < Grid.CountY; j++)
        for (int i = 0; i < Grid.CountX; i++)
        {
            int cell = Grid.CellIndex(i, j, k);
            if (i + 1 < Grid.CountX)
                AddFace(faces, cell, Grid.CellIndex(i + 1, j, k),
                    Grid.CellSizeYM * 0.5 * (CellThicknessM[cell] + CellThicknessM[Grid.CellIndex(i + 1, j, k)]),
                    Grid.CellSizeXM);
            if (j + 1 < Grid.CountY)
                AddFace(faces, cell, Grid.CellIndex(i, j + 1, k),
                    Grid.CellSizeXM * 0.5 * (CellThicknessM[cell] + CellThicknessM[Grid.CellIndex(i, j + 1, k)]),
                    Grid.CellSizeYM);
            if (k + 1 < Grid.CountZ)
            {
                int neighbor = Grid.CellIndex(i, j, k + 1);
                AddFace(faces, cell, neighbor, Grid.CellSizeXM * Grid.CellSizeYM,
                    0.5 * (CellThicknessM[cell] + CellThicknessM[neighbor]));
            }
        }

        return faces.ToArray();
    }

    private void AddFace(List<GridFace> faces, int cellA, int cellB, double areaM2, double distanceM)
    {
        double permeabilityA = PermeabilityM2[cellA];
        double permeabilityB = PermeabilityM2[cellB];
        double harmonicPermeability = 2 * permeabilityA * permeabilityB / (permeabilityA + permeabilityB);
        double transmissibility = harmonicPermeability * areaM2 / distanceM;
        faces.Add(new GridFace(cellA, cellB, transmissibility, CellDepthM[cellA] - CellDepthM[cellB]));
    }
}

internal static class RangeCalculator
{
    internal static ScalarRange Of(ReadOnlySpan<double> values)
    {
        double minimum = double.PositiveInfinity;
        double maximum = double.NegativeInfinity;
        foreach (double value in values)
        {
            minimum = Math.Min(minimum, value);
            maximum = Math.Max(maximum, value);
        }
        return new ScalarRange(minimum, maximum);
    }
}
