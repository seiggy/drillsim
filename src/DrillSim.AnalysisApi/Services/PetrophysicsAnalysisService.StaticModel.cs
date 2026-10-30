using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Services;

public sealed partial class PetrophysicsAnalysisService
{
    public const string StaticModelMethod = "contact-static-model-v1";
    internal const double StaticModelPaddingM = 1000;
    internal const double StaticModelMinimumCellM = 100;
    internal const int StaticModelMaximumNodesPerAxis = 120;
    internal const double StaticModelTargetLayerM = 5;
    private const int StaticModelMinimumLayers = 8, StaticModelMaximumLayers = 60, StaticModelMaximumPathPoints = 80;
    private static readonly ConcurrentDictionary<string, (StaticModelCore Core, StaticReservoirModel Model)> StaticModels = new();

    /// <summary>
    /// Builds (and caches per package, reservoir and configuration) the visible-evidence static model, then evaluates
    /// the requested targets exactly. Returns null when no located visible pick carries the formation.
    /// </summary>
    public static StaticReservoirModel? BuildStaticModel(AnalysisPackage package, string reservoirName,
        AnalysisConfiguration configuration, IEnumerable<ReservoirModelTarget>? targets = null)
    {
        if (CachedModel(package, reservoirName, configuration) is not var (core, model))
            return null;
        return model with { Targets = (targets ?? []).Select(t => core.Column(t.Id, t.EastingM, t.NorthingM)).ToArray() };
    }

    /// <summary>
    /// Share of the model's good rock that holds <see cref="AnalysisConfiguration.TargetFluid"/> at a point, or null when
    /// no fluid is targeted or no model can be built.
    /// </summary>
    internal static Func<double, double, double>? TargetFluidFraction(AnalysisPackage package, string? reservoirName,
        AnalysisConfiguration configuration)
    {
        if (configuration.TargetFluid is null || string.IsNullOrWhiteSpace(reservoirName) ||
            CachedModel(package, reservoirName, configuration) is not var (core, _))
            return null;
        bool oil = configuration.TargetFluid == "Oil";
        return (east, north) =>
        {
            ReservoirModelColumn column = core.Column("", east, north);
            return column.NetRockM <= 0 ? 0 : Math.Clamp((oil ? column.OilM : column.GasM) / column.NetRockM, 0, 1);
        };
    }

    private static (StaticModelCore Core, StaticReservoirModel Model)? CachedModel(AnalysisPackage package, string reservoirName,
        AnalysisConfiguration configuration)
    {
        // The model does not depend on the target fluid; share one cache entry across Oil/Gas/any.
        AnalysisConfiguration modelConfiguration = configuration with { TargetFluid = null };
        string key = $"{package.Sha256}|{package.FieldId}|{reservoirName.ToUpperInvariant()}|{modelConfiguration.ComputeSha256()}";
        if (StaticModels.TryGetValue(key, out var cached))
            return cached;
        if (StaticModelCore.Create(package, reservoirName, modelConfiguration) is not StaticModelCore core)
            return null;
        if (StaticModels.Count >= 8) StaticModels.Clear();
        return StaticModels.GetOrAdd(key, (core, core.Build(package, reservoirName, modelConfiguration)));
    }

    private readonly record struct ModelSample(double Md, bool RockKnown, bool Qualifies, double Porosity, double PermeabilityMd, string? Fluid);
    private sealed record PropertyWell(string EvidenceId, double Easting, double Northing, double[] NetToGross,
        double[] Porosity, double[] LogPermeabilityMd, bool[] Known);
    private readonly record struct ColumnValues(double Top, double Base, double Net, double Gas, double Oil, double Water,
        double Porosity, double LogPermeabilityMd, double HydrocarbonPoreM);

    private sealed partial class StaticModelCore
    {
        private (double E, double N, double Top, double Base, string Id)[] _controls = [];
        private PropertyWell[] _wells = [];
        private int _layers, _neighbours, _fluidWells, _classified;
        private double? _gasOil, _lower;
        private string _hydrocarbon = "none";
        private double _medianGross, _minE, _maxE, _minN, _maxN;
        private readonly List<FluidContactEstimate> _contacts = [];
        private readonly HashSet<Guid> _loggedBores = [];
        private IReadOnlyList<StructuralPick> _picks = [];

        public static StaticModelCore? Create(AnalysisPackage package, string reservoirName, AnalysisConfiguration configuration)
        {
            var (picks, _, _) = ReadStructuralPicks(package, reservoirName);
            if (picks.Count == 0) return null;
            var core = new StaticModelCore { _picks = picks, _neighbours = configuration.IdwNeighborCount };
            // Sidetracks at the same location would otherwise be over-weighted.
            core._controls = picks.Select(p => p.Control)
                .GroupBy(c => (Math.Round(c.EastingM), Math.Round(c.NorthingM)))
                .Select(g => (g.Average(c => c.EastingM), g.Average(c => c.NorthingM), g.Average(c => c.TopTrueVerticalDepthM),
                    g.Average(c => c.BaseTrueVerticalDepthM), g.OrderBy(c => c.GeologyEvidenceId, StringComparer.Ordinal).First().GeologyEvidenceId))
                .OrderBy(c => c.Item5, StringComparer.Ordinal).ToArray();
            double[] gross = core._controls.Select(c => c.Base - c.Top).Order().ToArray();
            core._medianGross = gross[gross.Length / 2];
            core._layers = Math.Clamp((int)Math.Round(core._medianGross / StaticModelTargetLayerM), StaticModelMinimumLayers, StaticModelMaximumLayers);
            core._minE = core._controls.Min(c => c.E) - StaticModelPaddingM;
            core._maxE = core._controls.Max(c => c.E) + StaticModelPaddingM;
            core._minN = core._controls.Min(c => c.N) - StaticModelPaddingM;
            core._maxN = core._controls.Max(c => c.N) + StaticModelPaddingM;

            var fluidSamples = new List<(double Tvd, string Fluid, int Well)>();
            var wells = new List<PropertyWell>();
            foreach (StructuralPick pick in picks.OrderBy(p => p.Control.GeologyEvidenceId, StringComparer.Ordinal))
            {
                double span = Math.Max(pick.BaseMd - pick.TopMd, 1e-6);
                ModelSample[] samples = ReadModelSamples(pick.Geology, reservoirName, pick.TopMd, pick.BaseMd, configuration)
                    .Where(s => s.RockKnown).ToArray();
                if (samples.Length == 0) continue;
                int L = core._layers;
                double[] count = new double[L], net = new double[L], phi = new double[L], logK = new double[L];
                foreach (ModelSample s in samples)
                {
                    int layer = Math.Clamp((int)((s.Md - pick.TopMd) / span * L), 0, L - 1);
                    count[layer]++;
                    if (!s.Qualifies) continue;
                    net[layer]++;
                    phi[layer] += s.Porosity;
                    logK[layer] += Math.Log10(Math.Max(s.PermeabilityMd, 1e-6));
                    if (s.Fluid is string fluid)
                        fluidSamples.Add((pick.Surveyed ? Interpolate(pick.TvdStations, s.Md) : s.Md, fluid, wells.Count));
                }
                if (samples.Any(s => s.Fluid is not null) && JsonAccess.Guid(pick.Geology, "WellBoreID") is Guid bore)
                    core._loggedBores.Add(bore);
                // Sparse sampling leaves empty thin layers; borrow the nearest sampled layer of the same well.
                int[] source = Enumerable.Range(0, L).Select(i => Enumerable.Range(0, L)
                    .Where(j => count[j] > 0 && Math.Abs(j - i) <= 3).OrderBy(j => Math.Abs(j - i)).DefaultIfEmpty(-1).First()).ToArray();
                wells.Add(new(pick.Control.GeologyEvidenceId, pick.Control.EastingM, pick.Control.NorthingM,
                    source.Select(j => j < 0 ? 0 : net[j] / count[j]).ToArray(),
                    source.Select(j => j < 0 || net[j] == 0 ? double.NaN : phi[j] / net[j]).ToArray(),
                    source.Select(j => j < 0 || net[j] == 0 ? double.NaN : logK[j] / net[j]).ToArray(),
                    source.Select(j => j >= 0).ToArray()));
            }
            core._wells = [.. wells];
            core._classified = fluidSamples.Count;
            core._fluidWells = fluidSamples.Select(s => s.Well).Distinct().Count();
            core.EstimateContacts(fluidSamples);
            return core;
        }

        private void EstimateContacts(List<(double Tvd, string Fluid, int Well)> samples)
        {
            bool gas = samples.Any(s => s.Fluid == "gas"), oil = samples.Any(s => s.Fluid == "oil");
            _hydrocarbon = gas && oil ? "gas and oil" : gas ? "gas" : oil ? "oil" : "none";
            if (_hydrocarbon == "none") return;
            var lower = Split(samples.Select(s => (s.Tvd, s.Fluid != "water", s.Well)).ToArray());
            var hydrocarbons = samples.Where(s => s.Fluid != "water" && (lower is null || s.Tvd < lower.Value.Depth)).ToArray();
            if (lower is { } water)
            {
                bool oilAbove = hydrocarbons.Any(s => s.Fluid == "oil");
                _lower = water.Depth;
                _contacts.Add(water.ToContact(oilAbove ? "OilWaterContact" : "GasWaterContact"));
            }
            if (gas && oil && Split(hydrocarbons.Select(s => (s.Tvd, s.Fluid == "gas", s.Well)).ToArray()) is { } gasOil)
            {
                _gasOil = gasOil.Depth;
                _contacts.Insert(0, gasOil.ToContact("GasOilContact"));
            }
            if (gas && oil && _gasOil is null)
                _hydrocarbon = hydrocarbons.Count(s => s.Fluid == "gas") >= hydrocarbons.Count(s => s.Fluid == "oil") ? "gas" : "oil";
        }

        private readonly record struct ContactSplit(double Depth, double Above, double Below, int Supporting, int Misclassified, int Wells)
        {
            public FluidContactEstimate ToContact(string type) => new(type, Math.Round(Depth, 1), Math.Round(Above, 1),
                Math.Round(Below, 1), Supporting, Misclassified, Wells);
        }

        /// <summary>Flat contact that misclassifies the fewest samples (upper fluid above, lower fluid below).</summary>
        private static ContactSplit? Split((double Tvd, bool Upper, int Well)[] samples)
        {
            var s = samples.OrderBy(x => x.Tvd).ToArray();
            int upperTotal = s.Count(x => x.Upper), n = s.Length;
            if (upperTotal == 0 || upperTotal == n) return null;
            int lowerAbove = 0, upperBelow = upperTotal, best = int.MaxValue, first = -1, last = -1;
            for (int k = 0; k <= n; k++)
            {
                int errors = lowerAbove + upperBelow;
                if (errors < best) (best, first, last) = (errors, k, k);
                else if (errors == best) last = k;
                if (k < n) { if (s[k].Upper) upperBelow--; else lowerAbove++; }
            }
            if (first == 0 || last == n) return null;
            double depth = (s[first - 1].Tvd + s[last].Tvd) / 2;
            double above = s.Where(x => x.Upper && x.Tvd < depth).Select(x => x.Tvd).DefaultIfEmpty(depth).Max();
            double below = s.Where(x => !x.Upper && x.Tvd >= depth).Select(x => x.Tvd).DefaultIfEmpty(depth).Min();
            return new(depth, above, below, n - best, best, s.Select(x => x.Well).Distinct().Count());
        }

        private (double Gas, double Oil, double Water) FluidFractions(double top, double @base)
        {
            if (_hydrocarbon == "none") return (0, 0, 1);
            double Frac(double contact) => @base <= top ? (contact > top ? 1 : 0) : Math.Clamp((contact - top) / (@base - top), 0, 1);
            double hydrocarbon = _lower is double lower ? Frac(lower) : 1;
            double gas = _gasOil is double gasOil ? Math.Min(Frac(gasOil), hydrocarbon) : _hydrocarbon == "oil" ? 0 : hydrocarbon;
            return (gas, hydrocarbon - gas, 1 - hydrocarbon);
        }

        private ColumnValues Evaluate(double east, double north, out (PropertyWell Well, double Distance)[] nearest, out double nearestControl)
        {
            double weightSum = 0, top = 0, @base = 0;
            nearestControl = double.PositiveInfinity;
            foreach (var c in _controls)
            {
                double d = Distance(east, north, c.E, c.N);
                nearestControl = Math.Min(nearestControl, d);
                double w = 1 / Math.Pow(Math.Max(d, 1), 2);
                weightSum += w; top += w * c.Top; @base += w * c.Base;
            }
            top /= weightSum; @base = Math.Max(@base / weightSum, top + 1);
            nearest = _wells.Select(w => (w, Distance(east, north, w.Easting, w.Northing))).OrderBy(x => x.Item2)
                .ThenBy(x => x.w.EvidenceId, StringComparer.Ordinal).ToArray();
            double dz = (@base - top) / _layers, net = 0, gas = 0, oil = 0, water = 0, phi = 0, logK = 0, pore = 0;
            for (int layer = 0; layer < _layers; layer++)
            {
                double ntgSum = 0, ntgWeight = 0, phiSum = 0, kSum = 0, rockWeight = 0;
                int used = 0;
                foreach (var (well, d) in nearest)
                {
                    if (!well.Known[layer]) continue;
                    double w = 1 / Math.Pow(Math.Max(d, 1), 2);
                    ntgSum += w * well.NetToGross[layer]; ntgWeight += w;
                    if (double.IsFinite(well.Porosity[layer]))
                    {
                        phiSum += w * well.Porosity[layer]; kSum += w * well.LogPermeabilityMd[layer]; rockWeight += w;
                    }
                    if (++used == _neighbours) break;
                }
                if (ntgWeight == 0) continue;
                double cellNet = dz * ntgSum / ntgWeight;
                if (cellNet <= 0 || rockWeight == 0) continue;
                double layerTop = top + layer * dz;
                var (g, o, wtr) = FluidFractions(layerTop, layerTop + dz);
                double layerPhi = phiSum / rockWeight;
                net += cellNet; gas += cellNet * g; oil += cellNet * o; water += cellNet * wtr;
                phi += cellNet * layerPhi; logK += cellNet * kSum / rockWeight; pore += cellNet * layerPhi * (g + o);
            }
            return new(top, @base, net, gas, oil, water, net > 0 ? phi / net : 0, net > 0 ? logK / net : 0, pore);
        }

        public ReservoirModelColumn Column(string id, double east, double north)
        {
            ColumnValues v = Evaluate(east, north, out var nearest, out double nearestControl);
            var (g, o, _) = FluidFractions(v.Top, v.Top + 0.01);
            return new(id, east, north,
                east >= _minE && east <= _maxE && north >= _minN && north <= _maxN,
                Math.Round(v.Top, 1), Math.Round(v.Base, 1), Math.Round(v.Base - v.Top, 1), Math.Round(v.Net, 1),
                Math.Round(v.Gas, 1), Math.Round(v.Oil, 1), Math.Round(v.Water, 1),
                Math.Round(v.Net / Math.Max(v.Base - v.Top, 1e-9), 3), Math.Round(v.Porosity, 3),
                Math.Round(v.Net > 0 ? Math.Pow(10, v.LogPermeabilityMd) : 0, 1),
                g > 0.5 ? "gas" : o > 0.5 ? "oil" : "water",
                Math.Round(nearestControl, 1), Math.Round(nearest.Length > 0 ? nearest[0].Distance : double.PositiveInfinity, 1),
                nearest.Take(_neighbours).Select(x => x.Well.EvidenceId).ToArray());
        }

        public StaticReservoirModel Build(AnalysisPackage package, string reservoirName, AnalysisConfiguration configuration)
        {
            double width = _maxE - _minE, height = _maxN - _minN;
            double cell = Math.Max(StaticModelMinimumCellM, Math.Max(width, height) / (StaticModelMaximumNodesPerAxis - 1));
            int columns = (int)Math.Floor(width / cell) + 1, rows = (int)Math.Floor(height / cell) + 1, n = columns * rows;
            double[] top = new double[n], @base = new double[n], net = new double[n], gas = new double[n], oil = new double[n],
                water = new double[n], porosity = new double[n];
            double gross = 0, netVolume = 0, gasVolume = 0, oilVolume = 0, pore = 0, area = cell * cell;
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                {
                    int i = row * columns + column;
                    ColumnValues v = Evaluate(_minE + column * cell, _minN + row * cell, out _, out _);
                    (top[i], @base[i], net[i], gas[i], oil[i], water[i], porosity[i]) = (Math.Round(v.Top, 1), Math.Round(v.Base, 1),
                        Math.Round(v.Net, 1), Math.Round(v.Gas, 1), Math.Round(v.Oil, 1), Math.Round(v.Water, 1), Math.Round(v.Porosity, 3));
                    gross += area * (v.Base - v.Top); netVolume += area * v.Net; gasVolume += area * v.Gas;
                    oilVolume += area * v.Oil; pore += area * v.HydrocarbonPoreM;
                }

            var limitations = new List<string>
            {
                FormattableString.Invariant($"Structure: inverse-distance surfaces through {_controls.Length} located visible picks of the formation; no faults, seismic or dip model."),
                FormattableString.Invariant($"Rock: {_layers} proportional layers (~{_medianGross / _layers:0.#} m); net-to-gross, porosity and permeability per layer from the nearest {_neighbours} of {_wells.Length} sampled wells."),
                "Fluids: one flat, field-wide contact set fitted to classified log samples in rock passing the cutoffs; no compartments, tilted contacts or transition zones.",
                FormattableString.Invariant($"Volumes cover the model box (picks plus {StaticModelPaddingM:0} m) and are geometric rock and pore volumes, not reserves.")
            };
            if (_hydrocarbon == "none")
                limitations.Add("No classified hydrocarbon samples: the whole formation is modelled as water-bearing.");
            else if (_lower is null)
                limitations.Add("No water-bearing samples below the hydrocarbons: hydrocarbons are assumed down to the formation base; the real contact is unknown.");

            return new(StaticModelMethod, package.FieldId, reservoirName, package.Sha256, configuration.ComputeSha256(),
                new(Math.Round(_minE, 1), Math.Round(_minN, 1), Math.Round(cell, 3), columns, rows, _layers, StaticModelPaddingM,
                    top, @base, net, gas, oil, water, porosity),
                _contacts.ToArray(),
                new(Math.Round(gross), Math.Round(netVolume), Math.Round(gasVolume), Math.Round(oilVolume), Math.Round(pore)),
                new(_controls.Length, _wells.Length, _fluidWells, _classified, Math.Round(_medianGross, 1), _hydrocarbon),
                ReadModelWells(package), [], limitations);
        }

        private IReadOnlyList<ReservoirModelWell> ReadModelWells(AnalysisPackage package)
        {
            GeographicOrigin? origin = ReadGeographicOrigin(package.Field);
            var picksByBore = _picks.Where(p => JsonAccess.Guid(p.Geology, "WellBoreID").HasValue)
                .GroupBy(p => JsonAccess.Guid(p.Geology, "WellBoreID")!.Value).ToDictionary(g => g.Key, g => g.First());
            var trajectories = package.Trajectories
                .Where(t => JsonAccess.Guid(t, "WellBoreID").HasValue)
                .GroupBy(t => JsonAccess.Guid(t, "WellBoreID")!.Value)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(t => JsonAccess.Get(t, "IsDefinitive") is JsonValue v && v.TryGetValue<bool>(out bool d) && d)
                    .ThenBy(t => JsonAccess.MetaId(t)).First());
            var result = new List<ReservoirModelWell>();
            foreach (JsonNode bore in package.WellBores)
            {
                if (JsonAccess.MetaId(bore) is not Guid boreId) continue;
                double[][] path = (JsonAccess.Get(trajectories.GetValueOrDefault(boreId), "SurveyStationList") as JsonArray ?? new JsonArray())
                    .Select(s => (Md: JsonAccess.Number(s, "MD"), Tvd: JsonAccess.Number(s, "TVD"), Point: ReadPoint(s, origin)))
                    .Where(s => s.Md.HasValue && s.Tvd.HasValue && s.Point.HasValue)
                    .OrderBy(s => s.Md)
                    .Select(s => new[] { Math.Round(s.Point!.Value.Easting, 1), Math.Round(s.Point.Value.Northing, 1), Math.Round(s.Tvd!.Value, 1) })
                    .ToArray();
                if (path.Length > StaticModelMaximumPathPoints)
                {
                    int step = (int)Math.Ceiling(path.Length / (double)StaticModelMaximumPathPoints);
                    path = path.Where((_, i) => i % step == 0).Append(path[^1]).Distinct().ToArray();
                }
                StructuralPick? pick = picksByBore.GetValueOrDefault(boreId);
                if (path.Length < 2 && pick is null) continue;
                Guid? wellId = JsonAccess.Guid(bore, "WellID");
                result.Add(new(boreId.ToString(), wellId?.ToString(),
                    SynthPrefix().Replace(JsonAccess.String(bore, "Name") ?? boreId.ToString(), ""),
                    pick is not null, _loggedBores.Contains(boreId), path,
                    pick?.Control.TopTrueVerticalDepthM is double t ? Math.Round(t, 1) : null,
                    pick?.Control.BaseTrueVerticalDepthM is double b ? Math.Round(b, 1) : null,
                    pick is null ? null : Math.Round(pick.Control.EastingM, 1),
                    pick is null ? null : Math.Round(pick.Control.NorthingM, 1)));
            }
            return result;
        }

        [GeneratedRegex(@"^SYNTH-\S+\s+", RegexOptions.IgnoreCase)]
        private static partial Regex SynthPrefix();
    }

    private static readonly Regex BadQuality = new("missing|bad.?hole|invalid", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>Formation samples from the first log run (PHIE, PERM, SW/SO/SG), or rock-only property-table samples.</summary>
    private static IEnumerable<ModelSample> ReadModelSamples(JsonNode geology, string reservoirName, double topMd, double baseMd,
        AnalysisConfiguration configuration)
    {
        JsonNode? run = (JsonAccess.Get(JsonAccess.Get(geology, "Petrophysics"), "LogRuns") as JsonArray)?.FirstOrDefault();
        var curves = new Dictionary<string, double?[]>(StringComparer.OrdinalIgnoreCase);
        foreach (JsonNode? curve in JsonAccess.Get(run, "Curves") as JsonArray ?? new JsonArray())
        {
            string mnemonic = JsonAccess.String(curve, "CanonicalMnemonic")?.ToUpperInvariant() ?? "";
            string unit = JsonAccess.String(curve, "CanonicalUnit") ?? "";
            if (mnemonic is "PHIE" or "SW" or "SO" or "SG" ? unit is not ("fraction" or "m3/m3") : mnemonic != "PERM" || unit != "m2")
                continue;
            JsonArray values = JsonAccess.Get(curve, "Values") as JsonArray ?? new JsonArray();
            JsonArray? nulls = JsonAccess.Get(curve, "NullFlags") as JsonArray;
            JsonArray? quality = JsonAccess.Get(curve, "QualityFlags") as JsonArray;
            curves[mnemonic] = values.Select((value, i) =>
                Value(value) is double v && !(nulls?.ElementAtOrDefault(i) is JsonValue flag && flag.TryGetValue<bool>(out bool isNull) && isNull) &&
                !(quality?.ElementAtOrDefault(i) is JsonValue q && q.TryGetValue<string>(out string? text) && BadQuality.IsMatch(text)) ? v : (double?)null).ToArray();
        }
        if (JsonAccess.Get(run, "DepthValues") is JsonArray depths && curves.ContainsKey("PHIE") && curves.ContainsKey("PERM"))
        {
            for (int i = 0; i < depths.Count; i++)
            {
                if (Value(depths[i]) is not double md || md < topMd || md > baseMd) continue;
                double? phi = At("PHIE", i), perm = At("PERM", i), sw = At("SW", i), so = At("SO", i), sg = At("SG", i);
                bool rock = phi is >= 0 and <= 1 && perm >= 0;
                string? fluid = null;
                if (sw is >= 0 and <= 1 && so is >= 0 and <= 1 && sg is >= 0 and <= 1 && Math.Abs(sw.Value + so.Value + sg.Value - 1) <= .05)
                    fluid = sw >= .55 ? "water" : sg >= .15 && sg >= so ? "gas" : so >= .15 ? "oil" : null;
                bool qualifies = rock && phi >= configuration.PorosityCutoff && perm >= configuration.PermeabilityCutoffM2;
                yield return new(md, rock, qualifies, phi ?? 0, (perm ?? 0) / PermeabilityCutoffM2, qualifies ? fluid : null);
            }
            yield break;
        }
        foreach (PetrophysicsSample sample in ReadSamples(geology, reservoirName).Where(IsFinite))
            yield return new(sample.MeasuredDepth, true, Qualifies(sample, configuration), sample.Porosity,
                sample.PermeabilityM2 / PermeabilityCutoffM2, null);

        double? At(string mnemonic, int index) =>
            curves.TryGetValue(mnemonic, out double?[]? values) && index < values.Length ? values[index] : null;
        static double? Value(JsonNode? node) =>
            node is JsonValue value && value.TryGetValue<double>(out double number) && double.IsFinite(number) ? number : null;
    }
}
