using System.Diagnostics;
using ReservoirSimulation.Domain;

namespace ReservoirSimulation.Simulation;

internal sealed record NumericalWorkDiagnostics(
    long MatrixApplications,
    TimeSpan MatrixTime,
    TimeSpan PreconditionerTime,
    TimeSpan UpwindTime,
    TimeSpan WellTime,
    string PressurePath);

internal sealed class NumericalWorkTracker
{
    internal long MatrixApplications { get; private set; }
    internal long MatrixTicks { get; private set; }
    internal long PreconditionerTicks { get; private set; }
    internal long UpwindTicks { get; private set; }
    internal long WellTicks { get; private set; }
    internal string PressurePath { get; private set; } = "unselected";

    internal void AddMatrix(long started)
    {
        MatrixApplications++;
        MatrixTicks += Stopwatch.GetTimestamp() - started;
    }
    internal void AddPreconditioner(long started) => PreconditionerTicks += Stopwatch.GetTimestamp() - started;
    internal void AddUpwind(long started) => UpwindTicks += Stopwatch.GetTimestamp() - started;
    internal void AddWell(long started) => WellTicks += Stopwatch.GetTimestamp() - started;
    internal void SelectPressurePath(string value) => PressurePath = value;
    internal NumericalWorkDiagnostics Snapshot() => new(
        MatrixApplications,
        Stopwatch.GetElapsedTime(0, MatrixTicks),
        Stopwatch.GetElapsedTime(0, PreconditionerTicks),
        Stopwatch.GetElapsedTime(0, UpwindTicks),
        Stopwatch.GetElapsedTime(0, WellTicks),
        PressurePath);
}

internal sealed class MatrixFreePressureSystem(
    double[] storageDiagonal,
    GridFace[] faces,
    int[] faceOffsets,
    int[] incidentFaceIndices,
    byte[] cellColors,
    double[] faceConductance,
    NumericalWorkTracker? work = null,
    bool parallelApply = false)
{
    internal int Size => storageDiagonal.Length;

    internal void Apply(double[] value, double[] result)
    {
        long started = Stopwatch.GetTimestamp();
        if (!parallelApply || result.Length < 4_096)
        {
            for (int cell = 0; cell < result.Length; cell++)
                result[cell] = storageDiagonal[cell] * value[cell];
            for (int faceIndex = 0; faceIndex < faces.Length; faceIndex++)
            {
                GridFace face = faces[faceIndex];
                double flux = faceConductance[faceIndex] * (value[face.CellA] - value[face.CellB]);
                result[face.CellA] += flux;
                result[face.CellB] -= flux;
            }
        }
        else
        {
            Parallel.For(0, result.Length, cell => result[cell] = ApplyCell(cell, value));
        }
        work?.AddMatrix(started);
    }


    private double ApplyCell(int cell, double[] value)
    {
        double result = storageDiagonal[cell] * value[cell];
        for (int position = faceOffsets[cell]; position < faceOffsets[cell + 1]; position++)
        {
            int faceIndex = incidentFaceIndices[position];
            GridFace face = faces[faceIndex];
            int neighbor = face.CellA == cell ? face.CellB : face.CellA;
            result += faceConductance[faceIndex] * (value[cell] - value[neighbor]);
        }
        return result;
    }


    internal void ApplySymmetricJacobiSweep(
        double[] residual, double[] result, double[] diagonal)
    {
        Array.Clear(result);
        ApplyColor(0, correction: false);
        ApplyColor(1, correction: false);
        ApplyColor(0, correction: true);

        void ApplyColor(byte color, bool correction)
        {
            for (int cell = 0; cell < result.Length; cell++)
            {
                if (cellColors[cell] != color)
                    continue;
                double value = correction ? result[cell] : residual[cell] / diagonal[cell];
                for (int position = faceOffsets[cell]; position < faceOffsets[cell + 1]; position++)
                {
                    int faceIndex = incidentFaceIndices[position];
                    GridFace face = faces[faceIndex];
                    int neighbor = face.CellA == cell ? face.CellB : face.CellA;
                    if (cellColors[neighbor] != color)
                        value += faceConductance[faceIndex] * result[neighbor] / diagonal[cell];
                }
                result[cell] = value;
            }
        }
    }


    internal double[] BuildJacobiDiagonal()
    {
        var diagonal = (double[])storageDiagonal.Clone();
        for (int faceIndex = 0; faceIndex < faces.Length; faceIndex++)
        {
            GridFace face = faces[faceIndex];
            double value = faceConductance[faceIndex];
            diagonal[face.CellA] += value;
            diagonal[face.CellB] += value;
        }
        return diagonal;
    }
}

internal sealed class CoarsePressurePreconditioner
{
    private readonly int[] _fineToCoarse;
    private readonly (int Column, double Value)[][] _lowerRows;
    private readonly (int Row, double Value)[][] _upperColumns;
    private readonly double[] _diagonal;
    private readonly double[] _coarseResidual;
    private readonly double[] _coarseSolution;

    internal CoarsePressurePreconditioner(
        int[] fineToCoarse, double[] cellDiagonal, GridFace[] faces, double[] conductance)
    {
        _fineToCoarse = fineToCoarse;
        int count = cellDiagonal.Length;
        var matrixLower = Enumerable.Range(0, count)
            .Select(_ => new Dictionary<int, double>())
            .ToArray();
        var matrixDiagonal = (double[])cellDiagonal.Clone();
        for (int faceIndex = 0; faceIndex < faces.Length; faceIndex++)
        {
            int lower = Math.Min(faces[faceIndex].CellA, faces[faceIndex].CellB);
            int upperCell = Math.Max(faces[faceIndex].CellA, faces[faceIndex].CellB);
            double value = conductance[faceIndex];
            matrixDiagonal[lower] += value;
            matrixDiagonal[upperCell] += value;
            matrixLower[upperCell][lower] = matrixLower[upperCell].GetValueOrDefault(lower) - value;
        }

        var factors = Enumerable.Range(0, count)
            .Select(_ => new Dictionary<int, double>())
            .ToArray();
        _diagonal = new double[count];
        for (int row = 0; row < count; row++)
        {
            foreach ((int column, double matrixValue) in matrixLower[row].OrderBy(pair => pair.Key))
            {
                double value = matrixValue;
                foreach ((int inner, double rowFactor) in factors[row])
                    if (inner < column && factors[column].TryGetValue(inner, out double columnFactor))
                        value -= rowFactor * columnFactor;
                factors[row][column] = value / _diagonal[column];
            }
            double diagonalValue = matrixDiagonal[row] - factors[row].Values.Sum(value => value * value);
            if (!(diagonalValue > 0) || !double.IsFinite(diagonalValue))
                throw new SimulationFailureException("Sparse coarse pressure factor is not positive definite.");
            _diagonal[row] = Math.Sqrt(diagonalValue);
        }
        _lowerRows = factors
            .Select(row => row.OrderBy(pair => pair.Key).Select(pair => (pair.Key, pair.Value)).ToArray())
            .ToArray();
        var upperColumns = Enumerable.Range(0, count)
            .Select(_ => new List<(int Row, double Value)>())
            .ToArray();
        for (int row = 0; row < count; row++)
            foreach ((int column, double value) in _lowerRows[row])
                upperColumns[column].Add((row, value));
        _upperColumns = upperColumns.Select(values => values.ToArray()).ToArray();
        _coarseResidual = new double[count];
        _coarseSolution = new double[count];
    }

    internal void AddCorrection(ReadOnlySpan<double> residual, Span<double> result)
    {
        Array.Clear(_coarseResidual);
        for (int cell = 0; cell < residual.Length; cell++)
            _coarseResidual[_fineToCoarse[cell]] += residual[cell];
        for (int row = 0; row < _diagonal.Length; row++)
        {
            double value = _coarseResidual[row];
            foreach ((int column, double factor) in _lowerRows[row])
                value -= factor * _coarseSolution[column];
            _coarseSolution[row] = value / _diagonal[row];
        }
        for (int row = _diagonal.Length - 1; row >= 0; row--)
        {
            double value = _coarseSolution[row];
            foreach ((int upperRow, double factor) in _upperColumns[row])
                value -= factor * _coarseSolution[upperRow];
            _coarseSolution[row] = value / _diagonal[row];
        }
        for (int cell = 0; cell < result.Length; cell++)
            result[cell] += _coarseSolution[_fineToCoarse[cell]];
    }
}


internal readonly record struct CgResult(bool Converged, int Iterations, double RelativeResidual);

internal static class ConjugateGradientSolver
{
    internal static CgResult Solve(
        MatrixFreePressureSystem system,
        ReadOnlySpan<double> rightHandSide,
        double[] solution,
        double relativeTolerance,
        int maximumIterations,
        bool useSymmetricPreconditioner,
        CoarsePressurePreconditioner? coarsePreconditioner = null,
        NumericalWorkTracker? work = null)
    {
        int count = system.Size;
        var applied = new double[count];
        var residual = new double[count];
        var preconditioned = new double[count];
        var direction = new double[count];
        double[] diagonal = system.BuildJacobiDiagonal();
        system.Apply(solution, applied);
        for (int cell = 0; cell < count; cell++)
        {
            if (!(diagonal[cell] > 0) || !double.IsFinite(diagonal[cell]))
                throw new SimulationFailureException("Pressure matrix has a nonpositive or nonfinite Jacobi diagonal.");
            residual[cell] = rightHandSide[cell] - applied[cell];
            preconditioned[cell] = residual[cell];
        }

        long preconditionerStarted = Stopwatch.GetTimestamp();
        if (useSymmetricPreconditioner)
            system.ApplySymmetricJacobiSweep(residual, preconditioned, diagonal);
        else
            for (int cell = 0; cell < count; cell++) preconditioned[cell] = residual[cell] / diagonal[cell];
        coarsePreconditioner?.AddCorrection(residual, preconditioned);
        work?.AddPreconditioner(preconditionerStarted);
        preconditioned.CopyTo(direction, 0);
        double initialResidualNorm = Math.Sqrt(Dot(residual, residual));
        if (!double.IsFinite(initialResidualNorm))
            throw new SimulationFailureException("Pressure solve produced a nonfinite initial residual.");
        if (initialResidualNorm == 0)
            return new CgResult(true, 0, 0);
        double tolerance = relativeTolerance * initialResidualNorm;
        double residualNorm = initialResidualNorm;

        double residualProduct = Dot(residual, preconditioned);
        for (int iteration = 1; iteration <= maximumIterations; iteration++)
        {
            system.Apply(direction, applied);
            double curvature = Dot(direction, applied);
            if (!(curvature > 0) || !double.IsFinite(curvature))
                throw new SimulationFailureException("Pressure matrix lost positive definiteness during conjugate gradient.");

            double alpha = residualProduct / curvature;
            for (int cell = 0; cell < count; cell++)
            {
                solution[cell] += alpha * direction[cell];
                residual[cell] -= alpha * applied[cell];
            }

            residualNorm = Math.Sqrt(Dot(residual, residual));
            if (!double.IsFinite(residualNorm))
                throw new SimulationFailureException("Pressure solve produced a nonfinite residual.");
            if (residualNorm <= tolerance)
                return new CgResult(true, iteration, residualNorm / initialResidualNorm);

            preconditionerStarted = Stopwatch.GetTimestamp();
            if (useSymmetricPreconditioner)
                system.ApplySymmetricJacobiSweep(residual, preconditioned, diagonal);
            else
                for (int cell = 0; cell < count; cell++) preconditioned[cell] = residual[cell] / diagonal[cell];
            coarsePreconditioner?.AddCorrection(residual, preconditioned);
            work?.AddPreconditioner(preconditionerStarted);
            double nextProduct = Dot(residual, preconditioned);
            if (!(nextProduct > 0) || !double.IsFinite(nextProduct))
                throw new SimulationFailureException("Jacobi-preconditioned residual became invalid.");
            double beta = nextProduct / residualProduct;
            for (int cell = 0; cell < count; cell++)
                direction[cell] = preconditioned[cell] + beta * direction[cell];
            residualProduct = nextProduct;
        }

        return new CgResult(false, maximumIterations, residualNorm / initialResidualNorm);
    }

    private static double Dot(ReadOnlySpan<double> left, ReadOnlySpan<double> right)
    {
        double result = 0;
        for (int index = 0; index < left.Length; index++)
            result += left[index] * right[index];
        return result;
    }
}
