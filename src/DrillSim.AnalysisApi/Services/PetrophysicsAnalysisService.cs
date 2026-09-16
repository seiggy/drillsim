using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Services;

public interface IPetrophysicsAnalysisService
{
    AnalysisResult Analyze(AnalysisPackage package, string? reservoirName = null);
    AnalysisResult Analyze(AnalysisPackage package, string? reservoirName, AnalysisConfiguration configuration) =>
        configuration == AnalysisConfiguration.Default
            ? Analyze(package, reservoirName)
            : throw new NotSupportedException("This analysis implementation does not support configurable screening.");
    NetPayResult CalculateNetPay(string evidenceId, IEnumerable<PetrophysicsSample> samples);
}

public sealed class PetrophysicsAnalysisService : IPetrophysicsAnalysisService
{
    private const double EarthRadiusM = 6_378_137.0;
    public const double PorosityCutoff = 0.12;
    public const double PermeabilityCutoffM2 = 9.869233e-16;
    public const double WellExclusionRadiusM = 500.0;
    public const double QuantileZScore = 1.2816;
    public const string ConfiguredCandidatePrefix = "configured:";

    private readonly TimeProvider _timeProvider;

    public PetrophysicsAnalysisService(TimeProvider timeProvider) => _timeProvider = timeProvider;

    public NetPayResult CalculateNetPay(string evidenceId, IEnumerable<PetrophysicsSample> samples) =>
        CalculateNetPay(evidenceId, samples, AnalysisConfiguration.Default);

    public NetPayResult CalculateNetPay(
        string evidenceId,
        IEnumerable<PetrophysicsSample> samples,
        AnalysisConfiguration configuration)
    {
        AnalysisConfiguration.Validate(configuration);
        PetrophysicsSample[] ordered = samples
            .Where(IsFinite)
            .OrderBy(sample => sample.MeasuredDepth)
            .ToArray();
        PetrophysicsSample[] pay = ordered.Where(sample => Qualifies(sample, configuration)).ToArray();

        double thickness = 0;
        for (int index = 1; index < ordered.Length; index++)
        {
            if (Qualifies(ordered[index - 1], configuration) && Qualifies(ordered[index], configuration))
                thickness += Math.Max(0, ordered[index].MeasuredDepth - ordered[index - 1].MeasuredDepth);
        }

        return new NetPayResult(
            evidenceId,
            thickness,
            pay.Length == 0 ? 0 : pay.Average(sample => sample.Porosity),
            pay.Length == 0 ? 0 : pay.Average(sample => sample.PermeabilityM2),
            pay.Length);
    }

    public AnalysisResult Analyze(AnalysisPackage package, string? reservoirName = null) =>
        Analyze(package, reservoirName, AnalysisConfiguration.Default);

    public AnalysisResult Analyze(AnalysisPackage package, string? reservoirName, AnalysisConfiguration configuration)
    {
        AnalysisConfiguration.Validate(configuration);
        var gaps = new List<string>(package.DataGaps);
        GeographicOrigin? origin = ReadGeographicOrigin(package.Field);
        IReadOnlyList<WellPaySummary> summaries = BuildWellSummaries(package, gaps, origin, reservoirName, configuration);
        var (grid, bounds) = BuildCandidateGrid(package, summaries, gaps, origin, configuration);
        RankedCandidate[] ranking = grid
            .Where(point => point.Status == "eligible" && point.Prediction is not null)
            .Select(point => point.Prediction!)
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.CandidateId, StringComparer.Ordinal)
            .Take(5)
            .Select((candidate, index) => candidate with { Rank = index + 1 })
            .ToArray();
        Dictionary<string, RankedCandidate> rankedById = ranking.ToDictionary(candidate => candidate.CandidateId);
        grid = grid.Select(point => rankedById.TryGetValue(point.CandidateId, out RankedCandidate? ranked)
            ? point with { Prediction = ranked }
            : point).ToArray();

        var methodology = new AnalysisMethodology(
            "Deterministic expected paydirt screening; derived, not reserves",
            configuration.PorosityCutoff,
            configuration.PermeabilityCutoffM2,
            "mean must be greater than zero",
            "Sum sorted measured-depth deltas only where both adjacent samples satisfy all cutoffs",
            $"Bounded {configuration.GridPointsPerAxis} by {configuration.GridPointsPerAxis} grid over field reference, delineation, and cluster metric positions",
            configuration.IdwNeighborCount,
            configuration.WellExclusionRadiusM,
            QuantileZScore,
            configuration.IdwNeighborCount == 4
                ? "IDW nearest-four well disagreement plus a distance-proportional term"
                : $"IDW nearest-{configuration.IdwNeighborCount} well disagreement plus a distance-proportional term",
            "p50NetPay * porosity * log10(1 + permeabilityMd) / (1 + relativeUncertainty)");

        var result = new AnalysisResult(
            _timeProvider.GetUtcNow(),
            package.FieldId,
            reservoirName,
            package.Sha256,
            methodology,
            summaries,
            ranking,
            gaps.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray())
        {
            Configuration = configuration,
            ConfigurationSha256 = configuration.ComputeSha256(),
            CandidateGrid = grid,
            CandidateGridBounds = bounds
        };
        return result with
        {
            AnalysisSha256 = PredictionJson.ComputeSha256(new
            {
                result.ModelVersion, result.FieldId, result.PackageSha256, result.ReservoirName,
                result.ConfigurationSha256, result.Methodology, result.WellSummaries,
                result.CandidateGridBounds, result.CandidateGrid, result.Ranking, result.DataGaps
            })
        };
    }

    public static (double P90, double P50, double P10) BuildQuantiles(double p50, double sigma)
    {
        double spread = QuantileZScore * Math.Max(0, sigma);
        return (Math.Max(0, p50 - spread), p50, p50 + spread);
    }

    private IReadOnlyList<WellPaySummary> BuildWellSummaries(
        AnalysisPackage package,
        List<string> gaps,
        GeographicOrigin? origin,
        string? reservoirName,
        AnalysisConfiguration configuration)
    {
        var wellById = package.Wells
            .Select(well => (Id: JsonAccess.MetaId(well), Node: well))
            .Where(item => item.Id.HasValue)
            .ToDictionary(item => item.Id!.Value, item => item.Node);
        var wellBoreToWell = package.WellBores
            .Select(wellBore => (WellBoreId: JsonAccess.MetaId(wellBore), WellId: JsonAccess.Guid(wellBore, "WellID")))
            .Where(item => item.WellBoreId.HasValue && item.WellId.HasValue)
            .ToDictionary(item => item.WellBoreId!.Value, item => item.WellId!.Value);
        var clusterById = package.Clusters
            .Select(cluster => (Id: JsonAccess.MetaId(cluster), Node: cluster))
            .Where(item => item.Id.HasValue)
            .ToDictionary(item => item.Id!.Value, item => item.Node);
        Dictionary<Guid, Coordinate> trajectoryLocations = BuildTrajectoryLocations(package.Trajectories, origin);

        var calculationsByWell = new Dictionary<Guid, List<NetPayResult>>();
        int recordsWithoutSamples = 0;
        foreach (JsonNode geology in package.GeologicalProperties)
        {
            Guid? wellBoreId = JsonAccess.Guid(geology, "WellBoreID");
            if (wellBoreId is null || !wellBoreToWell.TryGetValue(wellBoreId.Value, out Guid wellId))
            {
                gaps.Add("A geological-properties record could not be linked through wellbore to well.");
                continue;
            }

            string evidenceId = $"geology:{JsonAccess.MetaId(geology)?.ToString() ?? wellBoreId.Value.ToString()}";
            PetrophysicsSample[] samples = ReadSamples(geology, reservoirName).ToArray();
            if (samples.Length < 2)
            {
                recordsWithoutSamples++;
                continue;
            }

            NetPayResult calculation = CalculateNetPay(evidenceId, samples, configuration);
            if (!calculationsByWell.TryGetValue(wellId, out List<NetPayResult>? calculations))
                calculationsByWell[wellId] = calculations = [];
            calculations.Add(calculation);
        }
        if (recordsWithoutSamples > 0)
            gaps.Add($"{recordsWithoutSamples} reservoir intersection(s) have formation geometry but no complete petrophysics samples.");

        var summaries = new List<WellPaySummary>();
        foreach (var (wellId, calculations) in calculationsByWell.OrderBy(item => item.Key))
        {
            if (!wellById.TryGetValue(wellId, out JsonNode? well))
                continue;

            Coordinate? location = trajectoryLocations.GetValueOrDefault(wellId);
            if (location is null)
            {
                Guid? clusterId = JsonAccess.Guid(well, "ClusterID");
                if (clusterId is Guid id && clusterById.TryGetValue(id, out JsonNode? cluster))
                    location = ReadPoint(JsonAccess.Get(cluster, "ReferencePoint"), origin);
            }

            if (location is null)
            {
                gaps.Add($"well:{wellId} has expected paydirt data but no usable metric location.");
                continue;
            }

            int totalSamples = calculations.Sum(item => item.QualifyingSampleCount);
            summaries.Add(new WellPaySummary(
                wellId,
                $"well:{wellId}",
                calculations.Select(item => item.EvidenceId).Order(StringComparer.Ordinal).ToArray(),
                calculations.OrderBy(item => item.EvidenceId, StringComparer.Ordinal).ToArray(),
                calculations.Average(item => item.NetPayThicknessM),
                totalSamples == 0 ? 0 : calculations.Sum(item => item.MeanPayPorosity * item.QualifyingSampleCount) / totalSamples,
                totalSamples == 0 ? 0 : calculations.Sum(item => item.MeanPayPermeabilityM2 * item.QualifyingSampleCount) / totalSamples,
                location.Value.Easting,
                location.Value.Northing));
        }

        if (summaries.Count < configuration.IdwNeighborCount)
            gaps.Add($"At least {configuration.IdwNeighborCount} located wells with qualifying expected paydirt intervals are required for IDW ranking.");
        return summaries;
    }

    private static (IReadOnlyList<CandidateGridPoint> Grid, CandidateGridBounds? Bounds) BuildCandidateGrid(
        AnalysisPackage package,
        IReadOnlyList<WellPaySummary> wells,
        List<string> gaps,
        GeographicOrigin? origin,
        AnalysisConfiguration configuration)
    {
        List<Coordinate> boundsPoints = ReadBoundsPoints(package, origin)
            .Where(point => double.IsFinite(point.Easting) && double.IsFinite(point.Northing)).ToList();
        if (boundsPoints.Count < 2)
        {
            gaps.Add("Field delineation, reference, and cluster positions do not provide a candidate-grid extent.");
            return ([], null);
        }

        double minEast = boundsPoints.Min(point => point.Easting);
        double maxEast = boundsPoints.Max(point => point.Easting);
        double minNorth = boundsPoints.Min(point => point.Northing);
        double maxNorth = boundsPoints.Max(point => point.Northing);
        if (!double.IsFinite(maxEast - minEast) || !double.IsFinite(maxNorth - minNorth) ||
            maxEast - minEast < 1 || maxNorth - minNorth < 1)
        {
            gaps.Add("Candidate-grid extent is degenerate in metric coordinates.");
            return ([], null);
        }

        double diagonal = Math.Sqrt(Math.Pow(maxEast - minEast, 2) + Math.Pow(maxNorth - minNorth, 2));
        if (!double.IsFinite(diagonal))
        {
            gaps.Add("Candidate-grid extent exceeds finite metric computation limits.");
            return ([], null);
        }
        var bounds = new CandidateGridBounds(minEast, maxEast, minNorth, maxNorth);
        string candidatePrefix = configuration == AnalysisConfiguration.Default
            ? string.Empty
            : $"{ConfiguredCandidatePrefix}{configuration.ComputeSha256()}:";
        var grid = new List<CandidateGridPoint>();
        for (int eastIndex = 0; eastIndex < configuration.GridPointsPerAxis; eastIndex++)
        {
            double east = minEast + (maxEast - minEast) * eastIndex / (configuration.GridPointsPerAxis - 1);
            for (int northIndex = 0; northIndex < configuration.GridPointsPerAxis; northIndex++)
            {
                double north = minNorth + (maxNorth - minNorth) * northIndex / (configuration.GridPointsPerAxis - 1);
                string candidateId = $"{candidatePrefix}candidate:{eastIndex:D2}:{northIndex:D2}";
                var geographic = origin is GeographicOrigin value
                    ? LocalMetricToWgs84(east, north, value.LongitudeRadians, value.LatitudeRadians)
                    : MetricToWgs84(east, north);
                var nearest = wells
                    .Select(well => (Well: well, Distance: Distance(east, north, well.EastingM, well.NorthingM)))
                    .OrderBy(item => item.Distance)
                    .Take(configuration.IdwNeighborCount)
                    .ToArray();
                double? nearestDistance = nearest.Length == 0 ? null : nearest[0].Distance;
                bool excluded = nearestDistance < configuration.WellExclusionRadiusM;
                bool unsupported = nearest.Length < configuration.IdwNeighborCount;
                if (excluded || unsupported)
                {
                    var reasons = new List<string>();
                    if (excluded)
                        reasons.Add("within-well-exclusion-radius");
                    if (unsupported)
                        reasons.Add("insufficient-located-well-controls");
                    grid.Add(new(candidateId, east, north, geographic.LongitudeDegrees, geographic.LatitudeDegrees,
                        excluded ? "excluded" : "unsupported", reasons, nearestDistance, null));
                    continue;
                }

                double[] weights = nearest.Select(item => 1.0 / Math.Pow(Math.Max(item.Distance, 1), 2)).ToArray();
                double weightSum = weights.Sum();
                double p50 = WeightedAverage(nearest.Select(item => item.Well.NetPayThicknessM).ToArray(), weights, weightSum);
                double porosity = WeightedAverage(nearest.Select(item => item.Well.MeanPayPorosity).ToArray(), weights, weightSum);
                double permeabilityM2 = WeightedAverage(nearest.Select(item => item.Well.MeanPayPermeabilityM2).ToArray(), weights, weightSum);
                double disagreementVariance = nearest.Select((item, index) => weights[index] * Math.Pow(item.Well.NetPayThicknessM - p50, 2)).Sum() / weightSum;
                double weightedDistance = nearest.Select((item, index) => weights[index] * item.Distance).Sum() / weightSum;
                double distanceSigma = p50 * 0.15 * Math.Min(1, weightedDistance / diagonal);
                double sigma = Math.Sqrt(disagreementVariance + distanceSigma * distanceSigma);
                double relativeUncertainty = sigma / Math.Max(p50, 1e-12);
                double permeabilityMd = permeabilityM2 / PermeabilityCutoffM2;
                double score = p50 * porosity * Math.Log10(1 + permeabilityMd) / (1 + relativeUncertainty);
                var quantiles = BuildQuantiles(p50, sigma);
                if (!new[] { p50, porosity, permeabilityM2, disagreementVariance, weightedDistance, sigma,
                        relativeUncertainty, permeabilityMd, score, quantiles.P90, quantiles.P10 }.All(double.IsFinite))
                {
                    grid.Add(new(candidateId, east, north, geographic.LongitudeDegrees, geographic.LatitudeDegrees,
                        "unsupported", ["nonfinite-interpolation"], nearestDistance, null));
                    continue;
                }
                var candidate = new RankedCandidate(
                    0,
                    candidateId,
                    east,
                    north,
                    geographic.LongitudeDegrees,
                    geographic.LatitudeDegrees,
                    nearest[0].Distance,
                    quantiles.P90,
                    quantiles.P50,
                    quantiles.P10,
                    sigma,
                    porosity,
                    permeabilityMd,
                    relativeUncertainty,
                    score,
                    nearest.Select(item => item.Well.WellEvidenceId).ToArray())
                {
                    ScoreComponents = new(p50, porosity, permeabilityMd, Math.Log10(1 + permeabilityMd),
                        p50 * porosity * Math.Log10(1 + permeabilityMd), 1 + relativeUncertainty),
                    UncertaintyComponents = new(disagreementVariance, weightedDistance, diagonal,
                        distanceSigma, sigma, QuantileZScore, false)
                };
                grid.Add(new(candidateId, east, north, geographic.LongitudeDegrees, geographic.LatitudeDegrees,
                    "eligible", [], nearestDistance, candidate));
            }
        }

        if (grid.All(point => point.Status == "excluded"))
        {
            gaps.Add(FormattableString.Invariant(
                $"No bounded grid point remained after the {configuration.WellExclusionRadiusM} m well-location exclusion."));
        }
        if (grid.Any(point => point.Reasons.Contains("nonfinite-interpolation")))
            gaps.Add("Some grid points have unsupported nonfinite interpolation results; no prediction is reported for those points.");

        return (grid, bounds);
    }

    private static IEnumerable<PetrophysicsSample> ReadSamples(JsonNode geology, string? reservoirName)
    {
        if (JsonAccess.Get(geology, "GeologicalPropertyTable") is not JsonArray table)
            yield break;

        (double Top, double Base)? interval = ReadFormationInterval(geology, reservoirName);
        foreach (JsonNode? entry in table)
        {
            double? depth = JsonAccess.GaussianMean(entry, "MeasuredDepth");
            double? porosity = JsonAccess.GaussianMean(entry, "Porosity");
            double? permeability = JsonAccess.GaussianMean(entry, "Permeability");
            double? pressure = JsonAccess.GaussianMean(entry, "PressureDifferential");
            if (depth.HasValue && porosity.HasValue && permeability.HasValue && pressure.HasValue &&
                (interval is null || depth.Value >= interval.Value.Top && depth.Value < interval.Value.Base))
                yield return new PetrophysicsSample(depth.Value, porosity.Value, permeability.Value, pressure.Value);
        }
    }

    private static (double Top, double Base)? ReadFormationInterval(JsonNode geology, string? reservoirName)
    {
        if (string.IsNullOrWhiteSpace(reservoirName))
            return null;

        JsonNode? petrophysics = JsonAccess.Get(geology, "Petrophysics");
        if (JsonAccess.Get(petrophysics, "FormationIntervals") is JsonArray intervalNodes)
        {
            JsonNode? interval = intervalNodes.FirstOrDefault(node =>
                string.Equals(JsonAccess.String(node, "FormationName"), reservoirName, StringComparison.OrdinalIgnoreCase));
            double? top = JsonAccess.Number(JsonAccess.Get(interval, "TopDepth"), "Value");
            double? @base = JsonAccess.Number(JsonAccess.Get(interval, "BaseDepth"), "Value");
            if (top.HasValue && @base.HasValue && @base > top)
                return (top.Value, @base.Value);
        }

        if (JsonAccess.Get(petrophysics, "FormationTops") is not JsonArray topNodes)
            return (double.PositiveInfinity, double.PositiveInfinity);

        var tops = topNodes
            .Select(node => (
                Name: JsonAccess.String(node, "FormationName"),
                Depth: JsonAccess.Get(node, "Depths") is JsonArray depths && depths.Count > 0
                    ? JsonAccess.Number(depths[0], "Value")
                    : null))
            .Where(item => item.Name is not null && item.Depth.HasValue)
            .OrderBy(item => item.Depth)
            .ToArray();
        int index = Array.FindIndex(tops, item =>
            string.Equals(item.Name, reservoirName, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            return (double.PositiveInfinity, double.PositiveInfinity);
        return (tops[index].Depth!.Value, index + 1 < tops.Length ? tops[index + 1].Depth!.Value : double.PositiveInfinity);
    }

    private static IEnumerable<Coordinate> ReadBoundsPoints(AnalysisPackage package, GeographicOrigin? origin)
    {
        Coordinate? reference = ReadPoint(JsonAccess.Get(package.Field, "ReferencePoint"), origin);
        if (reference.HasValue)
            yield return reference.Value;

        if (JsonAccess.Get(package.Field, "DelineationLines") is JsonArray lines)
        {
            foreach (JsonNode? line in lines)
            {
                if (JsonAccess.Get(line, "Points") is not JsonArray points)
                    continue;
                foreach (JsonNode? pointNode in points)
                    if (ReadPoint(pointNode, origin) is Coordinate point)
                        yield return point;
            }
        }

        foreach (JsonNode cluster in package.Clusters)
            if (ReadPoint(JsonAccess.Get(cluster, "ReferencePoint"), origin) is Coordinate point)
                yield return point;
    }

    private static Dictionary<Guid, Coordinate> BuildTrajectoryLocations(
        IReadOnlyList<JsonNode> trajectories,
        GeographicOrigin? origin)
    {
        var locations = new Dictionary<Guid, Coordinate>();
        foreach (JsonNode trajectory in trajectories)
        {
            Guid? wellId = JsonAccess.Guid(trajectory, "WellID");
            if (!wellId.HasValue)
                continue;

            Coordinate? point = ReadPoint(JsonAccess.Get(trajectory, "TieInPoint"), origin);
            if (point is null && JsonAccess.Get(trajectory, "SurveyStationList") is JsonArray stations && stations.Count > 0)
                point = ReadPoint(stations[0], origin);
            if (point.HasValue)
                locations.TryAdd(wellId.Value, point.Value);
        }
        return locations;
    }

    private static Coordinate? ReadPoint(JsonNode? node, GeographicOrigin? origin)
    {
        if (node is null)
            return null;

        double? longitude = JsonAccess.Number(node, "Longitude");
        double? latitude = JsonAccess.Number(node, "Latitude");
        if (origin is GeographicOrigin geographicOrigin && longitude.HasValue && latitude.HasValue)
        {
            return new Coordinate(
                EarthRadiusM * Math.Cos(geographicOrigin.LatitudeRadians) * (longitude.Value - geographicOrigin.LongitudeRadians),
                EarthRadiusM * (latitude.Value - geographicOrigin.LatitudeRadians));
        }

        double? east = JsonAccess.Number(node, "RiemannianEast") ?? JsonAccess.Number(node, "Y");
        double? north = JsonAccess.Number(node, "RiemannianNorth") ?? JsonAccess.Number(node, "X");
        if (east.HasValue && north.HasValue)
        {
            return origin is GeographicOrigin metricOrigin &&
                metricOrigin.RiemannianEast.HasValue &&
                metricOrigin.RiemannianNorth.HasValue
                ? new Coordinate(
                    east.Value - metricOrigin.RiemannianEast.Value,
                    north.Value - metricOrigin.RiemannianNorth.Value)
                : new Coordinate(east.Value, north.Value);
        }

        if (longitude.HasValue && latitude.HasValue)
        {
            double latitudeRadians = latitude.Value;
            return new Coordinate(
                EarthRadiusM * longitude.Value * Math.Cos(latitudeRadians),
                EarthRadiusM * latitudeRadians);
        }

        return ReadPoint(JsonAccess.Get(node, "Position"), origin) ??
            ReadPoint(JsonAccess.Get(node, "GlobalCoordinates"), origin);
    }

    private static GeographicOrigin? ReadGeographicOrigin(JsonNode field)
    {
        JsonNode? point = JsonAccess.Get(field, "ReferencePoint");
        double? longitude = JsonAccess.Number(point, "Longitude");
        double? latitude = JsonAccess.Number(point, "Latitude");
        return longitude.HasValue && latitude.HasValue
            ? new GeographicOrigin(
                longitude.Value,
                latitude.Value,
                JsonAccess.Number(point, "RiemannianEast") ?? JsonAccess.Number(point, "Y"),
                JsonAccess.Number(point, "RiemannianNorth") ?? JsonAccess.Number(point, "X"))
            : null;
    }

    private static bool Qualifies(PetrophysicsSample sample, AnalysisConfiguration configuration) =>
        sample.Porosity >= configuration.PorosityCutoff &&
        sample.PermeabilityM2 >= configuration.PermeabilityCutoffM2 &&
        sample.PressureDifferential > 0;

    private static bool IsFinite(PetrophysicsSample sample) =>
        double.IsFinite(sample.MeasuredDepth) && double.IsFinite(sample.Porosity) &&
        double.IsFinite(sample.PermeabilityM2) && double.IsFinite(sample.PressureDifferential);

    private static double WeightedAverage(double[] values, double[] weights, double weightSum) =>
        values.Select((value, index) => value * weights[index]).Sum() / weightSum;

    private static double Distance(double x1, double y1, double x2, double y2) =>
        Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));

    public static (double LongitudeDegrees, double LatitudeDegrees) MetricToWgs84(double easting, double northing)
    {
        double latitudeRadians = northing / EarthRadiusM;
        double longitudeRadians = easting / (EarthRadiusM * Math.Cos(latitudeRadians));
        return (longitudeRadians * 180 / Math.PI, latitudeRadians * 180 / Math.PI);
    }

    public static (double LongitudeDegrees, double LatitudeDegrees) LocalMetricToWgs84(
        double easting,
        double northing,
        double originLongitudeRadians,
        double originLatitudeRadians)
    {
        double latitudeRadians = originLatitudeRadians + northing / EarthRadiusM;
        double longitudeRadians = originLongitudeRadians +
            easting / (EarthRadiusM * Math.Cos(originLatitudeRadians));
        return (longitudeRadians * 180 / Math.PI, latitudeRadians * 180 / Math.PI);
    }

    private readonly record struct Coordinate(double Easting, double Northing);
    private readonly record struct GeographicOrigin(
        double LongitudeRadians,
        double LatitudeRadians,
        double? RiemannianEast,
        double? RiemannianNorth);
}
