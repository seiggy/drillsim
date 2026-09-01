using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using GeographicLib;

namespace OSDC.Drilling.EarthMagneticField.Model;

/// <summary>Loads WMM2025 and IGRF14 once and performs stateless north-east-down evaluations.</summary>
public sealed class EarthMagneticFieldEvaluator
{
    private const double NanoteslaToTesla = 1e-9;
    private readonly IReadOnlyDictionary<EarthMagneticFieldModel, MagneticModel> models_;
    private readonly IReadOnlyDictionary<EarthMagneticFieldModel, EarthMagneticModelInfo> modelInfo_;

    public EarthMagneticFieldEvaluator(string? modelDirectory = null)
    {
        string directory = ResolveModelDirectory(modelDirectory);
        var models = new Dictionary<EarthMagneticFieldModel, MagneticModel>
        {
            [EarthMagneticFieldModel.WMM2025] = LoadModel("wmm2025", directory),
            [EarthMagneticFieldModel.IGRF14] = LoadModel("igrf14", directory)
        };
        models_ = models;
        modelInfo_ = new Dictionary<EarthMagneticFieldModel, EarthMagneticModelInfo>
        {
            [EarthMagneticFieldModel.WMM2025] = CreateModelInfo(
                EarthMagneticFieldModel.WMM2025, "WMM2025A", models[EarthMagneticFieldModel.WMM2025], directory),
            [EarthMagneticFieldModel.IGRF14] = CreateModelInfo(
                EarthMagneticFieldModel.IGRF14, "IGRF14-A", models[EarthMagneticFieldModel.IGRF14], directory)
        };
        ServiceInfo = new EarthMagneticFieldServiceInfo
        {
            Models = modelInfo_.Values.OrderBy(info => info.Model).ToList()
        };
    }

    public EarthMagneticFieldServiceInfo ServiceInfo { get; }

    public EvaluateEarthMagneticFieldResponse Evaluate(EvaluateEarthMagneticFieldRequest? request,
        int maximumSamples = 10_000, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EarthMagneticFieldValidationError> errors = Validate(request, maximumSamples);
        if (errors.Count != 0) throw new EarthMagneticFieldValidationException(errors);

        MagneticModel model = models_[request!.Model];
        var response = new EvaluateEarthMagneticFieldResponse { Model = modelInfo_[request.Model] };
        foreach (EarthMagneticFieldEvaluationPoint input in request.Samples)
        {
            cancellationToken.ThrowIfCancellationRequested();
            double decimalYear = ToDecimalYear(input.DateTimeUtc);
            double latitudeDegrees = input.Latitude * 180.0 / Math.PI;
            double longitudeDegrees = input.Longitude * 180.0 / Math.PI;
            double ellipsoidalHeight = -input.Depth;

            // GeographicLib returns east/north/up in nanotesla. Reordering, the vertical
            // sign change, and conversion to SI teslas occur only at this boundary.
            (double eastNanotesla, double northNanotesla, double upNanotesla) = model.Evaluate(
                decimalYear, latitudeDegrees, longitudeDegrees, ellipsoidalHeight);
            double north = northNanotesla * NanoteslaToTesla;
            double east = eastNanotesla * NanoteslaToTesla;
            double down = -upNanotesla * NanoteslaToTesla;
            double horizontal = Math.Sqrt(north * north + east * east);
            double total = Math.Sqrt(horizontal * horizontal + down * down);

            response.Samples.Add(new EarthMagneticFieldSample
            {
                Input = new EarthMagneticFieldEvaluationPoint
                {
                    Latitude = input.Latitude,
                    Longitude = input.Longitude,
                    Depth = input.Depth,
                    DateTimeUtc = input.DateTimeUtc.ToUniversalTime()
                },
                North = north,
                East = east,
                Down = down,
                HorizontalIntensity = horizontal,
                TotalIntensity = total,
                Declination = horizontal > 0 ? Math.Atan2(east, north) : null,
                Inclination = total > 0 ? Math.Atan2(down, horizontal) : null
            });
        }
        return response;
    }

    public EarthMagneticModelInfo GetModelInfo(EarthMagneticFieldModel model) =>
        modelInfo_.TryGetValue(model, out EarthMagneticModelInfo? info)
            ? info
            : throw new ArgumentOutOfRangeException(nameof(model), model, "Unsupported magnetic model.");

    internal static double ToDecimalYear(DateTimeOffset dateTimeUtc)
    {
        DateTimeOffset utc = dateTimeUtc.ToUniversalTime();
        var start = new DateTimeOffset(utc.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddYears(1);
        return utc.Year + (utc - start).TotalSeconds / (end - start).TotalSeconds;
    }

    private IReadOnlyList<EarthMagneticFieldValidationError> Validate(
        EvaluateEarthMagneticFieldRequest? request, int maximumSamples)
    {
        var errors = new List<EarthMagneticFieldValidationError>();
        if (request == null)
        {
            errors.Add(new(null, "Request", "required", "A request object is required."));
            return errors;
        }
        if (!models_.ContainsKey(request.Model))
            errors.Add(new(null, "Model", "unsupported", "Model must be WMM2025 or IGRF14."));
        if (request.Samples == null)
        {
            errors.Add(new(null, "Samples", "required", "Samples is required."));
            return errors;
        }
        if (request.Samples.Count == 0)
            errors.Add(new(null, "Samples", "empty", "At least one sample is required."));
        if (request.Samples.Count > maximumSamples)
            errors.Add(new(null, "Samples", "too_many",
                $"At most {maximumSamples.ToString(CultureInfo.InvariantCulture)} samples are allowed."));

        EarthMagneticModelInfo? info = modelInfo_.GetValueOrDefault(request.Model);
        for (int index = 0; index < request.Samples.Count; index++)
        {
            EarthMagneticFieldEvaluationPoint? sample = request.Samples[index];
            if (sample == null)
            {
                errors.Add(new(index, "Sample", "required", "Sample must not be null."));
                continue;
            }
            ValidateAngle(errors, index, "Latitude", sample.Latitude, -Math.PI / 2, Math.PI / 2);
            ValidateAngle(errors, index, "Longitude", sample.Longitude, -Math.PI, Math.PI);
            if (!double.IsFinite(sample.Depth))
                errors.Add(new(index, "Depth", "not_finite", "Depth must be finite and expressed in SI metres."));
            else if (info != null && (sample.Depth < info.MinimumDepth || sample.Depth > info.MaximumDepth))
                errors.Add(new(index, "Depth", "out_of_range",
                    $"Depth for {request.Model} must be between {info.MinimumDepth} and {info.MaximumDepth} SI metres."));

            if (sample.DateTimeUtc.Offset != TimeSpan.Zero)
                errors.Add(new(index, "DateTimeUtc", "not_utc",
                    "DateTimeUtc must use UTC with Z or the equivalent +00:00 offset."));
            if (info != null && (sample.DateTimeUtc < info.MinimumUtc || sample.DateTimeUtc > info.MaximumUtc))
                errors.Add(new(index, "DateTimeUtc", "out_of_range",
                    $"DateTimeUtc for {request.Model} must be between {info.MinimumUtc:O} and {info.MaximumUtc:O}, inclusive."));
        }
        return errors;
    }

    private static void ValidateAngle(List<EarthMagneticFieldValidationError> errors, int index,
        string property, double value, double minimum, double maximum)
    {
        if (!double.IsFinite(value))
            errors.Add(new(index, property, "not_finite", $"{property} must be finite."));
        else if (value < minimum || value > maximum)
            errors.Add(new(index, property, "out_of_range",
                $"{property} must be between {minimum} and {maximum} SI radians."));
    }

    private static MagneticModel LoadModel(string name, string directory)
    {
        string metadataPath = Path.Combine(directory, name + ".wmm");
        string coefficientPath = metadataPath + ".cof";
        if (!File.Exists(metadataPath) || !File.Exists(coefficientPath))
            throw new FileNotFoundException($"Required magnetic model files for '{name}' were not found in '{directory}'.");
        return new MagneticModel(name, directory);
    }

    private static EarthMagneticModelInfo CreateModelInfo(EarthMagneticFieldModel modelKind, string id,
        MagneticModel model, string directory)
    {
        string name = model.MagneticModelName ?? modelKind.ToString();
        string metadataPath = Path.Combine(directory, name + ".wmm");
        string coefficientPath = metadataPath + ".cof";
        return new EarthMagneticModelInfo
        {
            Model = modelKind,
            Name = name,
            ID = id,
            Description = model.Description,
            ReleaseDate = model.DateTime,
            MinimumUtc = YearBoundary(model.MinTime),
            MaximumUtc = YearBoundary(model.MaxTime),
            MinimumDepth = -model.MaxHeight,
            MaximumDepth = -model.MinHeight,
            Degree = model.Degree,
            Order = model.Order,
            GeographicLibVersion = typeof(MagneticModel).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? typeof(MagneticModel).Assembly.GetName().Version?.ToString() ?? "Unknown",
            MetadataSHA256 = Hash(metadataPath),
            CoefficientSHA256 = Hash(coefficientPath)
        };
    }

    private static DateTimeOffset YearBoundary(double decimalYear) =>
        new((int)Math.Round(decimalYear), 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static string Hash(string path) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static string ResolveModelDirectory(string? configured) => !string.IsNullOrWhiteSpace(configured)
        ? Path.GetFullPath(configured)
        : Path.Combine(AppContext.BaseDirectory, "MagneticFieldModelFiles");
}
