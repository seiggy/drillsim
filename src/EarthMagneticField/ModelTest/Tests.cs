using GeographicLib;
using NUnit.Framework;
using OSDC.Drilling.EarthMagneticField.Model;

namespace OSDC.Drilling.EarthMagneticField.ModelTest;

public class Tests
{
    private EarthMagneticFieldEvaluator evaluator_ = null!;

    [SetUp]
    public void Setup() => evaluator_ = new EarthMagneticFieldEvaluator();

    [TestCase(EarthMagneticFieldModel.WMM2025, "wmm2025", 2026, 8, 24)]
    [TestCase(EarthMagneticFieldModel.IGRF14, "igrf14", 2020, 6, 30)]
    public void EvaluationMatchesGeographicLibWithExplicitNedAndSiBoundary(
        EarthMagneticFieldModel modelKind, string modelName, int year, int month, int day)
    {
        var time = new DateTimeOffset(year, month, day, 10, 30, 0, TimeSpan.Zero);
        const double latitude = 0.7;
        const double longitude = -1.2;
        const double depth = 500;
        EvaluateEarthMagneticFieldResponse response = evaluator_.Evaluate(Request(modelKind, latitude, longitude, depth, time));

        var model = new MagneticModel(modelName, Path.Combine(AppContext.BaseDirectory, "MagneticFieldModelFiles"));
        (double eastNt, double northNt, double upNt) = model.Evaluate(
            DecimalYear(time), latitude * 180 / Math.PI, longitude * 180 / Math.PI, -depth);
        EarthMagneticFieldSample result = response.Samples.Single();

        Assert.Multiple(() =>
        {
            Assert.That(result.North, Is.EqualTo(northNt * 1e-9).Within(1e-16));
            Assert.That(result.East, Is.EqualTo(eastNt * 1e-9).Within(1e-16));
            Assert.That(result.Down, Is.EqualTo(-upNt * 1e-9).Within(1e-16));
            Assert.That(result.HorizontalIntensity,
                Is.EqualTo(Math.Sqrt(result.North * result.North + result.East * result.East)).Within(1e-16));
            Assert.That(result.TotalIntensity,
                Is.EqualTo(Math.Sqrt(result.HorizontalIntensity * result.HorizontalIntensity + result.Down * result.Down)).Within(1e-16));
            Assert.That(result.Declination, Is.EqualTo(Math.Atan2(result.East, result.North)).Within(1e-15));
            Assert.That(result.Inclination, Is.EqualTo(Math.Atan2(result.Down, result.HorizontalIntensity)).Within(1e-15));
            Assert.That(result.Input.DateTimeUtc.Offset, Is.EqualTo(TimeSpan.Zero));
        });
    }

    [Test]
    public void ServiceInformationDescribesBothModelsAndConventions()
    {
        EarthMagneticModelInfo wmm = evaluator_.GetModelInfo(EarthMagneticFieldModel.WMM2025);
        EarthMagneticModelInfo igrf = evaluator_.GetModelInfo(EarthMagneticFieldModel.IGRF14);
        Assert.Multiple(() =>
        {
            Assert.That(evaluator_.ServiceInfo.Models, Has.Count.EqualTo(2));
            Assert.That(evaluator_.ServiceInfo.CoordinateFrame, Is.EqualTo("north-east-down"));
            Assert.That(evaluator_.ServiceInfo.TimeConvention, Is.EqualTo("UTC"));
            Assert.That(wmm.ID, Is.EqualTo("WMM2025A"));
            Assert.That(wmm.MinimumUtc, Is.EqualTo(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)));
            Assert.That(wmm.MaximumUtc, Is.EqualTo(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero)));
            Assert.That(wmm.MinimumDepth, Is.EqualTo(-850000));
            Assert.That(wmm.MaximumDepth, Is.EqualTo(1000));
            Assert.That(igrf.ID, Is.EqualTo("IGRF14-A"));
            Assert.That(igrf.MinimumUtc.Year, Is.EqualTo(1900));
            Assert.That(igrf.MinimumDepth, Is.EqualTo(-600000));
            Assert.That(wmm.MetadataSHA256, Has.Length.EqualTo(64));
            Assert.That(igrf.CoefficientSHA256, Has.Length.EqualTo(64));
        });
    }

    [Test]
    public void ConcurrentEvaluationProducesStableResults()
    {
        EvaluateEarthMagneticFieldResponse[] responses = Enumerable.Range(0, 64).AsParallel()
            .Select(_ => evaluator_.Evaluate(Request(EarthMagneticFieldModel.WMM2025, 0.5, 1.0, 100,
                new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))))
            .ToArray();
        Assert.That(responses.Select(response => response.Samples[0].TotalIntensity).Distinct().ToArray(), Has.Length.EqualTo(1));
    }

    [TestCase(Math.PI, 0, 0, "Latitude")]
    [TestCase(0, 4, 0, "Longitude")]
    [TestCase(0, 0, double.NaN, "Depth")]
    [TestCase(0, 0, 1001, "Depth")]
    public void InvalidSpatialInputRejectsCompleteBatch(double latitude, double longitude, double depth, string property)
    {
        EarthMagneticFieldValidationException exception = Assert.Throws<EarthMagneticFieldValidationException>(() =>
            evaluator_.Evaluate(Request(EarthMagneticFieldModel.WMM2025, latitude, longitude, depth,
                new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))))!;
        Assert.That(exception.Errors, Has.Some.Property("Property").EqualTo(property));
    }

    [Test]
    public void NonUtcTimeIsRejected()
    {
        EarthMagneticFieldValidationException exception = Assert.Throws<EarthMagneticFieldValidationException>(() =>
            evaluator_.Evaluate(Request(EarthMagneticFieldModel.WMM2025, 0, 0, 0,
                new DateTimeOffset(2026, 1, 1, 1, 0, 0, TimeSpan.FromHours(1)))))!;
        Assert.That(exception.Errors, Has.Some.Property("Code").EqualTo("not_utc"));
    }

    [Test]
    public void ModelDateRangeIsEnforced()
    {
        EarthMagneticFieldValidationException exception = Assert.Throws<EarthMagneticFieldValidationException>(() =>
            evaluator_.Evaluate(Request(EarthMagneticFieldModel.WMM2025, 0, 0, 0,
                new DateTimeOffset(2024, 12, 31, 23, 59, 59, TimeSpan.Zero))))!;
        Assert.That(exception.Errors, Has.Some.Property("Property").EqualTo("DateTimeUtc"));
    }

    [Test]
    public void MaximumBatchSizeIsEnforced()
    {
        EvaluateEarthMagneticFieldRequest request = Request(EarthMagneticFieldModel.WMM2025, 0, 0, 0,
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        request.Samples.Add(new EarthMagneticFieldEvaluationPoint
        {
            DateTimeUtc = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        });
        EarthMagneticFieldValidationException exception = Assert.Throws<EarthMagneticFieldValidationException>(
            () => evaluator_.Evaluate(request, 1))!;
        Assert.That(exception.Errors, Has.Some.Property("Code").EqualTo("too_many"));
    }

    private static EvaluateEarthMagneticFieldRequest Request(EarthMagneticFieldModel model, double latitude,
        double longitude, double depth, DateTimeOffset time) => new()
        {
            Model = model,
            Samples =
        [
            new EarthMagneticFieldEvaluationPoint
            {
                Latitude = latitude,
                Longitude = longitude,
                Depth = depth,
                DateTimeUtc = time
            }
        ]
        };

    private static double DecimalYear(DateTimeOffset time)
    {
        DateTimeOffset start = new(time.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset end = start.AddYears(1);
        return time.Year + (time - start).TotalSeconds / (end - start).TotalSeconds;
    }
}
