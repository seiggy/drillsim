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

public sealed partial class PetrophysicsAnalysisService : IPetrophysicsAnalysisService
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
        Func<double, double, double>? fluidFraction = TargetFluidFraction(package, reservoirName, configuration);
        if (configuration.TargetFluid is not null && fluidFraction is null)
            gaps.Add($"No reservoir model could be built, so {configuration.TargetFluid.ToLowerInvariant()} pay at candidates is not separated from other fluids.");
        var (grid, bounds) = BuildCandidateGrid(package, summaries, gaps, origin, configuration, fluidFraction);
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
            configuration.TargetFluid is null
                ? "Deterministic expected paydirt screening; derived, not reserves"
                : $"Deterministic expected {configuration.TargetFluid.ToLowerInvariant()} paydirt screening: well pay counts only good-rock log readings dominated by {configuration.TargetFluid.ToLowerInvariant()}, and candidate pay is rock pay times the reservoir model's {configuration.TargetFluid.ToLowerInvariant()} share at the point; derived, not reserves",
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
        int recordsWithoutSamples = 0, recordsWithoutFluid = 0;
        var rockPay = new Dictionary<string, double>(StringComparer.Ordinal);
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
            rockPay[evidenceId] = calculation.NetPayThicknessM;
            if (configuration.TargetFluid is string fluid)
            {
                (double Top, double Base) interval = ReadFormationInterval(geology, reservoirName) ?? (double.NegativeInfinity, double.PositiveInfinity);
                string[] fluids = ReadModelSamples(geology, reservoirName ?? "", interval.Top, interval.Base, configuration)
                    .Where(sample => sample.Fluid is not null).Select(sample => sample.Fluid!).ToArray();
                if (fluids.Length == 0)
                    recordsWithoutFluid++;
                else
                    calculation = calculation with
                    {
                        NetPayThicknessM = calculation.NetPayThicknessM *
                            fluids.Count(item => item.Equals(fluid, StringComparison.OrdinalIgnoreCase)) / fluids.Length
                    };
            }
            if (!calculationsByWell.TryGetValue(wellId, out List<NetPayResult>? calculations))
                calculationsByWell[wellId] = calculations = [];
            calculations.Add(calculation);
        }
        if (recordsWithoutSamples > 0)
            gaps.Add($"{recordsWithoutSamples} reservoir intersection(s) have formation geometry but no complete petrophysics samples.");
        if (recordsWithoutFluid > 0)
            gaps.Add($"{recordsWithoutFluid} well record(s) have no fluid-saturation log; their full rock pay is counted as {configuration.TargetFluid?.ToLowerInvariant()} pay.");

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
                location.Value.Northing)
            {
                RockNetPayThicknessM = configuration.TargetFluid is null ? null : calculations.Average(item => rockPay[item.EvidenceId])
            });
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
        AnalysisConfiguration configuration,
        Func<double, double, double>? fluidFraction = null)
    {
        // With a model share, interpolate rock pay and apply the share once; otherwise use the wells' (fluid) pay.
        double Pay(WellPaySummary well) => fluidFraction is null ? well.NetPayThicknessM : well.RockNetPayThicknessM ?? well.NetPayThicknessM;
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
                double p50 = WeightedAverage(nearest.Select(item => Pay(item.Well)).ToArray(), weights, weightSum);
                double porosity = WeightedAverage(nearest.Select(item => item.Well.MeanPayPorosity).ToArray(), weights, weightSum);
                double permeabilityM2 = WeightedAverage(nearest.Select(item => item.Well.MeanPayPermeabilityM2).ToArray(), weights, weightSum);
                double disagreementVariance = nearest.Select((item, index) => weights[index] * Math.Pow(Pay(item.Well) - p50, 2)).Sum() / weightSum;
                double weightedDistance = nearest.Select((item, index) => weights[index] * item.Distance).Sum() / weightSum;
                double distanceSigma = p50 * 0.15 * Math.Min(1, weightedDistance / diagonal);
                double sigma = Math.Sqrt(disagreementVariance + distanceSigma * distanceSigma);
                double? fraction = fluidFraction?.Invoke(east, north);
                p50 *= fraction ?? 1;
                sigma *= fraction ?? 1;
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
                        distanceSigma, sigma, QuantileZScore, false),
                    TargetFluidFraction = fraction
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

    public const string StructureEstimateMethod = "formation-structure-idw-v1";
    internal const double StructuralSlopeSigma = 0.02;

    /// <summary>
    /// Inverse-distance (power 2) estimate of visible formation top/base TVD at a target, in the candidate-grid frame.
    /// Controls are selected by <see cref="ReadStructuralControls"/> (survey TVD, or inlier assumed-vertical picks).
    /// </summary>
    public static FormationStructureEstimate EstimateFormationStructure(
        AnalysisPackage package, string reservoirName, double targetEasting, double targetNorthing, int neighborCount)
    {
        (IReadOnlyList<FormationStructureControl> located, int unsurveyed, int excluded) = ReadStructuralControls(package, reservoirName);
        List<FormationStructureControl> controls = located.Select(c => c with
        {
            DistanceM = Distance(targetEasting, targetNorthing, c.EastingM, c.NorthingM)
        }).ToList();

        var limitations = new List<string>
        {
            "Inverse-distance interpolation of visible picks only; no faults, dip model, seismic or hidden truth.",
            FormattableString.Invariant($"P90/P10 spread = neighbour disagreement plus {StructuralSlopeSigma:0.##} x weighted distance; uncalibrated.")
        };
        if (unsurveyed > 0)
            limitations.Add($"{unsurveyed} control(s) lack survey TVD; their reported depths are treated as TVD.");
        if (excluded > 0)
            limitations.Add($"{excluded} unsurveyed control(s) excluded under {StructuralControlRule}: a surveyed pick exists for the same well, or the reported MD top/thickness is an outlier versus surveyed picks.");
        // Sidetracks at the same location would otherwise be over-weighted.
        FormationStructureControl[] nearest = controls
            .GroupBy(c => (Math.Round(c.EastingM), Math.Round(c.NorthingM)))
            .Select(group => group.OrderBy(c => c.GeologyEvidenceId, StringComparer.Ordinal).First() with
            {
                TopTrueVerticalDepthM = group.Average(c => c.TopTrueVerticalDepthM),
                BaseTrueVerticalDepthM = group.Average(c => c.BaseTrueVerticalDepthM)
            })
            .OrderBy(c => c.DistanceM).ThenBy(c => c.GeologyEvidenceId, StringComparer.Ordinal)
            .Take(Math.Max(1, neighborCount))
            .ToArray();
        if (nearest.Length < neighborCount)
            limitations.Add($"Only {nearest.Length} located control(s) carry the selected formation; {neighborCount} were requested.");
        if (nearest.Length == 0)
            return new(StructureEstimateMethod, targetEasting, targetNorthing, 0, null, null, null, [], limitations);

        double[] weights = nearest.Select(c => 1.0 / Math.Pow(Math.Max(c.DistanceM, 1), 2)).ToArray();
        double weightSum = weights.Sum();
        double topP50 = WeightedAverage(nearest.Select(c => c.TopTrueVerticalDepthM).ToArray(), weights, weightSum);
        double thickness = WeightedAverage(nearest.Select(c => c.BaseTrueVerticalDepthM - c.TopTrueVerticalDepthM).ToArray(), weights, weightSum);
        double variance = nearest.Select((c, i) => weights[i] * Math.Pow(c.TopTrueVerticalDepthM - topP50, 2)).Sum() / weightSum;
        double weightedDistance = nearest.Select((c, i) => weights[i] * c.DistanceM).Sum() / weightSum;
        double sigma = Math.Sqrt(variance + Math.Pow(StructuralSlopeSigma * weightedDistance, 2));
        var (p90, p50, p10) = BuildQuantiles(topP50, sigma);
        return new(StructureEstimateMethod, targetEasting, targetNorthing, nearest.Length,
            new(p90, p50, p10), new(p90 + thickness, p50 + thickness, p10 + thickness), thickness, nearest, limitations);
    }

    public const string SimulatorCoverageMethod = "simulator-coverage-v1";
    // scripts/seed_reservoir_world.py pads the hidden world's grid by horizontalPaddingM = 2000 around the same
    // structural controls; 500 m is held back for coordinate-frame differences between the seed and this grid.
    internal const double SeedHorizontalPaddingM = 2000;
    internal const double CoverageFrameSafetyM = 500;
    internal const double CoverageMarginM = SeedHorizontalPaddingM - CoverageFrameSafetyM;
    // RestrictedTruthSamplingService accepts station TVD up to world base + max(1000, base - top).
    internal const double SimulatorMinimumVerticalToleranceM = 1000;
    internal const double CoverageDepthSafetyM = 100;

    /// <summary>
    /// Conservative estimate of the blind simulator's hidden-world extent, in the candidate-grid frame, from the
    /// structural controls the world seed is conditioned on. Returns null when no located control carries the formation.
    /// </summary>
    public static SimulatorCoverage? EstimateSimulatorCoverage(AnalysisPackage package, string reservoirName)
    {
        IReadOnlyList<FormationStructureControl> controls = ReadStructuralControls(package, reservoirName).Controls;
        if (controls.Count == 0) return null;
        double minTop = controls.Min(c => c.TopTrueVerticalDepthM);
        double maxBase = controls.Max(c => c.BaseTrueVerticalDepthM);
        return new(SimulatorCoverageMethod,
            controls.Min(c => c.EastingM) - CoverageMarginM, controls.Max(c => c.EastingM) + CoverageMarginM,
            controls.Min(c => c.NorthingM) - CoverageMarginM, controls.Max(c => c.NorthingM) + CoverageMarginM,
            maxBase + Math.Max(SimulatorMinimumVerticalToleranceM, maxBase - minTop) - CoverageDepthSafetyM,
            controls.Count, CoverageMarginM,
            FormattableString.Invariant($"Approximate blind-simulator model extent: the bounding box of {controls.Count} visible formation control(s) plus {CoverageMarginM:0} m, and its deep TVD envelope. Targets or well depths outside it cannot be drilled in the simulator. Not a geological boundary."));
    }

    /// <summary>
    /// Located visible picks of the formation (DistanceM is zero). Interval MD is converted with the bore's own
    /// survey-station TVD when it covers the interval. Unsurveyed picks follow <see cref="StructuralControlRule"/>
    /// (mirrored by scripts/seed_reservoir_world.py): with at least four surveyed picks, an unsurveyed pick is kept as
    /// assumed-vertical only if its well has no surveyed pick and its MD top and thickness lie inside the Tukey
    /// far-out fences of the surveyed picks; otherwise it is excluded. With fewer surveyed picks all are kept.
    /// </summary>
    internal static (IReadOnlyList<FormationStructureControl> Controls, int Unsurveyed, int Excluded) ReadStructuralControls(
        AnalysisPackage package, string reservoirName)
    {
        var (picks, unsurveyed, excluded) = ReadStructuralPicks(package, reservoirName);
        return (picks.Select(pick => pick.Control).ToArray(), unsurveyed, excluded);
    }

    private sealed record StructuralPick(
        FormationStructureControl Control, bool Surveyed, Guid? WellId, JsonNode Geology,
        (double Md, double Tvd)[] TvdStations, double TopMd, double BaseMd);

    private static (IReadOnlyList<StructuralPick> Picks, int Unsurveyed, int Excluded) ReadStructuralPicks(
        AnalysisPackage package, string reservoirName)
    {
        GeographicOrigin? origin = ReadGeographicOrigin(package.Field);
        var boreToWell = package.WellBores
            .Select(bore => (Id: JsonAccess.MetaId(bore), WellId: JsonAccess.Guid(bore, "WellID")))
            .Where(item => item.Id.HasValue && item.WellId.HasValue)
            .ToDictionary(item => item.Id!.Value, item => item.WellId!.Value);
        var wellToCluster = package.Wells
            .Select(well => (Id: JsonAccess.MetaId(well), ClusterId: JsonAccess.Guid(well, "ClusterID")))
            .Where(item => item.Id.HasValue && item.ClusterId.HasValue)
            .ToDictionary(item => item.Id!.Value, item => item.ClusterId!.Value);
        var clusters = package.Clusters
            .Select(cluster => (Id: JsonAccess.MetaId(cluster), Node: cluster))
            .Where(item => item.Id.HasValue)
            .ToDictionary(item => item.Id!.Value, item => item.Node);
        var trajectoryByBore = new Dictionary<Guid, JsonNode>();
        var trajectoryByWell = new Dictionary<Guid, JsonNode>();
        foreach (JsonNode trajectory in package.Trajectories)
        {
            if (JsonAccess.Guid(trajectory, "WellBoreID") is Guid boreId) trajectoryByBore.TryAdd(boreId, trajectory);
            if (JsonAccess.Guid(trajectory, "WellID") is Guid wellId) trajectoryByWell.TryAdd(wellId, trajectory);
        }

        var candidates = new List<StructuralPick>();
        foreach (JsonNode geology in package.GeologicalProperties)
        {
            if (ReadFormationInterval(geology, reservoirName) is not (double top, double @base) ||
                !double.IsFinite(top) || !double.IsFinite(@base) || JsonAccess.Guid(geology, "WellBoreID") is not Guid boreId)
                continue;
            Guid? wellId = boreToWell.TryGetValue(boreId, out Guid linked) ? linked : null;
            JsonNode? trajectory = trajectoryByBore.GetValueOrDefault(boreId) ??
                (wellId is Guid id ? trajectoryByWell.GetValueOrDefault(id) : null);
            var stations = (JsonAccess.Get(trajectory, "SurveyStationList") as JsonArray ?? new JsonArray())
                .Select(station => (Md: JsonAccess.Number(station, "MD"), Tvd: JsonAccess.Number(station, "TVD"),
                    Point: ReadPoint(station, origin)))
                .Where(station => station.Md is double md && double.IsFinite(md))
                .OrderBy(station => station.Md)
                .ToArray();
            var tvdStations = stations.Where(s => s.Tvd is double v && double.IsFinite(v)).Select(s => (s.Md!.Value, s.Tvd!.Value)).ToArray();
            var pointStations = stations.Where(s => s.Point.HasValue).ToArray();
            double middle = (top + @base) / 2;
            Coordinate? location = pointStations.Length > 0
                ? new Coordinate(
                    Interpolate(pointStations.Select(s => (s.Md!.Value, s.Point!.Value.Easting)).ToArray(), middle),
                    Interpolate(pointStations.Select(s => (s.Md!.Value, s.Point!.Value.Northing)).ToArray(), middle))
                : ReadPoint(JsonAccess.Get(trajectory, "TieInPoint"), origin);
            if (location is null && wellId is Guid well && wellToCluster.TryGetValue(well, out Guid clusterId) &&
                clusters.TryGetValue(clusterId, out JsonNode? cluster))
                location = ReadPoint(JsonAccess.Get(cluster, "ReferencePoint"), origin);
            if (location is not Coordinate point || !double.IsFinite(point.Easting) || !double.IsFinite(point.Northing))
                continue;
            bool ownSurvey = JsonAccess.Guid(trajectory, "WellBoreID") is not Guid surveyBore || surveyBore == boreId;
            bool surveyed = ownSurvey && tvdStations.Length > 0 &&
                tvdStations[0].Item1 <= Math.Min(top, @base) && Math.Max(top, @base) <= tvdStations[^1].Item1;
            double topTvd = surveyed ? Interpolate(tvdStations, top) : top;
            double baseTvd = surveyed ? Interpolate(tvdStations, @base) : @base;
            candidates.Add(new(new(
                $"geology:{JsonAccess.MetaId(geology)?.ToString() ?? boreId.ToString()}",
                wellId is Guid w ? $"well:{w}" : null,
                point.Easting, point.Northing, 0,
                Math.Min(topTvd, baseTvd), Math.Max(topTvd, baseTvd),
                surveyed ? "survey-interpolated TVD" : "reported interval depth; no survey, assumed vertical"),
                surveyed, wellId, geology, surveyed ? tvdStations : [], Math.Min(top, @base), Math.Max(top, @base)));
        }
        var surveyedControls = candidates.Where(c => c.Surveyed).Select(c => c.Control).ToArray();
        if (surveyedControls.Length < MinimumSurveyedStructuralControls)
            return (candidates, candidates.Count(c => !c.Surveyed), 0);
        (double Low, double High) topFences = FarOutFences(surveyedControls.Select(c => c.TopTrueVerticalDepthM));
        (double Low, double High) thicknessFences = FarOutFences(
            surveyedControls.Select(c => c.BaseTrueVerticalDepthM - c.TopTrueVerticalDepthM));
        var surveyedWells = candidates.Where(c => c.Surveyed && c.WellId.HasValue).Select(c => c.WellId!.Value).ToHashSet();
        StructuralPick[] kept = candidates.Where(c => c.Surveyed ||
                !(c.WellId is Guid parent && surveyedWells.Contains(parent)) &&
                c.Control.TopTrueVerticalDepthM >= topFences.Low && c.Control.TopTrueVerticalDepthM <= topFences.High &&
                c.Control.BaseTrueVerticalDepthM - c.Control.TopTrueVerticalDepthM >= thicknessFences.Low &&
                c.Control.BaseTrueVerticalDepthM - c.Control.TopTrueVerticalDepthM <= thicknessFences.High)
            .ToArray();
        return (kept, kept.Length - surveyedControls.Length, candidates.Count - kept.Length);
    }

    public const string StructuralControlRule = "structural-controls-surveyed-or-inlier-v1";
    internal const int MinimumSurveyedStructuralControls = 4;
    internal const double StructuralInlierFenceIqr = 3;

    private static (double Low, double High) FarOutFences(IEnumerable<double> values)
    {
        double[] ordered = values.Order().ToArray();
        double q1 = LinearQuantile(ordered, .25), q3 = LinearQuantile(ordered, .75);
        double spread = StructuralInlierFenceIqr * (q3 - q1);
        return (q1 - spread, q3 + spread);

        static double LinearQuantile(double[] sorted, double fraction)
        {
            double position = (sorted.Length - 1) * fraction;
            int lower = (int)Math.Floor(position), upper = Math.Min(lower + 1, sorted.Length - 1);
            return sorted[lower] + (position - lower) * (sorted[upper] - sorted[lower]);
        }
    }

    private static double Interpolate(IReadOnlyList<(double Md, double Value)> stations, double md)
    {
        if (md <= stations[0].Md) return stations[0].Value;
        for (int i = 1; i < stations.Count; i++)
            if (md <= stations[i].Md)
                return stations[i - 1].Value + (md - stations[i - 1].Md) /
                    Math.Max(stations[i].Md - stations[i - 1].Md, 1e-12) * (stations[i].Value - stations[i - 1].Value);
        return stations[^1].Value;
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
