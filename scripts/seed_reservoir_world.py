#!/usr/bin/env python3
import argparse
import hashlib
import json
import math
import os
from pathlib import Path
import urllib.error
import urllib.request

from seed_northstar import discover_urls


DEFAULT_FIELD_ID = "502fea8b-4d85-5d40-86a7-f5b7655d89d6"
DEFAULT_RESERVOIR = "SOGNEFJORD FM"
PERMEABILITY_M2_PER_MD = 9.869233e-16
DEFAULT_CALIBRATION = Path(__file__).resolve().parent.parent / "datasets" / "reservoir-fluid-calibration.json"


def read(record, *names):
    if not isinstance(record, dict):
        return None
    lowered = {key.lower(): value for key, value in record.items()}
    return next((lowered[name.lower()] for name in names if name.lower() in lowered), None)


def gaussian_mean(value):
    gaussian = read(value, "GaussianValue")
    candidate = read(gaussian, "Mean") if gaussian else read(value, "Mean")
    return candidate if isinstance(candidate, (int, float)) and math.isfinite(candidate) else None


def record_id(record):
    return str(read(read(record, "MetaInfo"), "ID") or "")


def request_json(url, method="GET", body=None, headers=None):
    payload = None if body is None else json.dumps(body, allow_nan=False).encode()
    request = urllib.request.Request(
        url,
        payload,
        {"Accept": "application/json", "Content-Type": "application/json", **(headers or {})},
        method=method,
    )
    try:
        with urllib.request.urlopen(request, timeout=300) as response:
            return json.load(response)
    except urllib.error.HTTPError as error:
        detail = error.read().decode()
        raise RuntimeError(f"{method} {url}: HTTP {error.code} {detail}") from error


def formation_interval(geology, reservoir_name):
    petrophysics = read(geology, "Petrophysics")
    for interval in read(petrophysics, "FormationIntervals") or []:
        if str(read(interval, "FormationName") or "").casefold() != reservoir_name.casefold():
            continue
        top = read(read(interval, "TopDepth"), "Value")
        base = read(read(interval, "BaseDepth"), "Value")
        if all(isinstance(value, (int, float)) and math.isfinite(value) for value in (top, base)) and base > top:
            return float(top), float(base)
    return None


def interpolate_station(stations, measured_depth, *names):
    ordered = []
    for station in stations or []:
        depth = read(station, "MD")
        value = read(station, *names)
        if all(isinstance(item, (int, float)) and math.isfinite(item) for item in (depth, value)):
            ordered.append((depth, value))
    ordered.sort(key=lambda item: item[0])
    if not ordered:
        return measured_depth
    if measured_depth <= ordered[0][0]:
        return ordered[0][1]
    for left, right in zip(ordered, ordered[1:]):
        if measured_depth <= right[0]:
            ratio = (measured_depth - left[0]) / max(right[0] - left[0], 1e-12)
            return left[1] + ratio * (right[1] - left[1])
    return ordered[-1][1]


def interpolate_tvd(stations, measured_depth):
    return interpolate_station(stations, measured_depth, "TVD")


# P9.6 structural-control rule (mirrored by PetrophysicsAnalysisService.ReadStructuralControls):
# an MD formation interval is converted to TVD only with the wellbore's own survey stations covering both ends.
# When at least MINIMUM_SURVEYED_CONTROLS surveyed controls exist, an unsurveyed control is kept (as an assumed-
# vertical pick) only if its parent well has no surveyed control and its MD top and MD thickness both lie inside
# the Tukey far-out fences (Q1 - 3 IQR, Q3 + 3 IQR; linear quantiles) of the surveyed TVD tops and thicknesses.
# Deviated or sidetracked bores without surveys (e.g. a 982 m MD "thickness") are excluded instead of being
# treated as vertical. With fewer surveyed controls the legacy assumed-vertical behaviour is kept and reported.
STRUCTURAL_CONTROL_RULE = "structural-controls-surveyed-or-inlier-v1"
MINIMUM_SURVEYED_CONTROLS = 4
INLIER_FENCE_IQR = 3.0


def covering_survey_tvd(stations, top_md, base_md):
    ordered = sorted(
        (read(station, "MD"), read(station, "TVD"))
        for station in stations or []
        if all(
            isinstance(value, (int, float)) and math.isfinite(value)
            for value in (read(station, "MD"), read(station, "TVD"))
        )
    )
    if not ordered or top_md < ordered[0][0] or base_md > ordered[-1][0]:
        return None
    return interpolate_tvd(stations, top_md), interpolate_tvd(stations, base_md)


def linear_quantile(values, fraction):
    ordered = sorted(values)
    position = (len(ordered) - 1) * fraction
    lower = math.floor(position)
    upper = min(lower + 1, len(ordered) - 1)
    return ordered[lower] + (position - lower) * (ordered[upper] - ordered[lower])


def far_out_fences(values):
    q1, q3 = linear_quantile(values, 0.25), linear_quantile(values, 0.75)
    spread = INLIER_FENCE_IQR * (q3 - q1)
    return q1 - spread, q3 + spread


def select_structural_controls(candidates):
    """Return (kept, excluded) candidates under STRUCTURAL_CONTROL_RULE."""
    surveyed = [candidate for candidate in candidates if candidate["surveyed"]]
    unsurveyed = [candidate for candidate in candidates if not candidate["surveyed"]]
    if len(surveyed) < MINIMUM_SURVEYED_CONTROLS:
        return candidates, []
    top_fences = far_out_fences([candidate["top"] for candidate in surveyed])
    thickness_fences = far_out_fences([candidate["base"] - candidate["top"] for candidate in surveyed])
    surveyed_wells = {candidate["wellId"] for candidate in surveyed if candidate["wellId"]}
    excluded = {}
    for candidate in unsurveyed:
        thickness = candidate["base"] - candidate["top"]
        if candidate["wellId"] and candidate["wellId"] in surveyed_wells:
            excluded[id(candidate)] = "parent well has a surveyed control"
        elif not top_fences[0] <= candidate["top"] <= top_fences[1]:
            excluded[id(candidate)] = (
                f"unsurveyed MD top {candidate['top']:.1f} m outside surveyed fences "
                f"{top_fences[0]:.1f}-{top_fences[1]:.1f} m"
            )
        elif not thickness_fences[0] <= thickness <= thickness_fences[1]:
            excluded[id(candidate)] = (
                f"unsurveyed MD thickness {thickness:.1f} m outside surveyed fences "
                f"{thickness_fences[0]:.1f}-{thickness_fences[1]:.1f} m"
            )
    return (
        [candidate for candidate in candidates if id(candidate) not in excluded],
        [{**candidate, "reason": excluded[id(candidate)]} for candidate in candidates if id(candidate) in excluded],
    )


def finite_values(values, indices):
    return [
        values[index]
        for index in indices
        if index < len(values) and isinstance(values[index], (int, float)) and math.isfinite(values[index])
    ]


def package_indexes(package):
    well_bores = {record_id(item): item for item in package["wellBores"]}
    wells = {record_id(item): item for item in package["wells"]}
    clusters = {record_id(item): item for item in package["clusters"]}
    trajectories = {
        str(read(trajectory, "WellBoreID") or ""): trajectory
        for trajectory in package["trajectories"]
    }
    return well_bores, wells, clusters, trajectories


def field_origin(package):
    point = read(package["field"], "ReferencePoint")
    easting = read(point, "RiemannianEast", "Y")
    northing = read(point, "RiemannianNorth", "X")
    if not all(isinstance(value, (int, float)) and math.isfinite(value) for value in (easting, northing)):
        raise ValueError("Field ReferencePoint requires finite RiemannianEast and RiemannianNorth values")
    return easting, northing


def bore_location(well_bore_id, well_bores, wells, clusters, trajectory, measured_depth):
    stations = read(trajectory, "SurveyStationList") or []
    if stations:
        easting = interpolate_station(stations, measured_depth, "RiemannianEast", "Y")
        northing = interpolate_station(stations, measured_depth, "RiemannianNorth", "X")
        if all(isinstance(value, (int, float)) and math.isfinite(value) for value in (easting, northing)):
            return easting, northing
    well_bore = well_bores.get(well_bore_id)
    well = wells.get(str(read(well_bore, "WellID") or ""))
    cluster = clusters.get(str(read(well, "ClusterID") or ""))
    point = read(cluster, "ReferencePoint")
    easting = read(point, "RiemannianEast", "Y")
    northing = read(point, "RiemannianNorth", "X")
    return (easting, northing) if all(
        isinstance(value, (int, float)) and math.isfinite(value)
        for value in (easting, northing)
    ) else None


def structural_conditioning_points(package, reservoir_name):
    """Return (points, excluded) structural controls selected by STRUCTURAL_CONTROL_RULE."""
    well_bores, wells, clusters, trajectories = package_indexes(package)
    origin_easting, origin_northing = field_origin(package)
    candidates = []
    for geology in package["geologicalProperties"]:
        well_bore_id = str(read(geology, "WellBoreID") or "")
        interval = formation_interval(geology, reservoir_name)
        if not interval:
            continue
        trajectory = trajectories.get(well_bore_id)
        stations = read(trajectory, "SurveyStationList") or []
        location = bore_location(
            well_bore_id,
            well_bores,
            wells,
            clusters,
            trajectory,
            sum(interval) / 2,
        )
        if not location:
            continue
        surveyed_tvd = covering_survey_tvd(stations, *interval)
        top_tvd, base_tvd = surveyed_tvd or interval
        candidates.append(
            {
                "eastingM": location[0] - origin_easting,
                "northingM": location[1] - origin_northing,
                "top": min(top_tvd, base_tvd),
                "base": max(top_tvd, base_tvd),
                "surveyed": surveyed_tvd is not None,
                "wellId": str(read(well_bores.get(well_bore_id), "WellID") or ""),
                "evidenceId": f"geology:{record_id(geology)}",
            }
        )
    kept, excluded = select_structural_controls(candidates)
    points = [
        {
            "eastingM": candidate["eastingM"],
            "northingM": candidate["northingM"],
            "reservoirTopDepthM": candidate["top"],
            "reservoirBaseDepthM": candidate["base"],
            "evidenceId": candidate["evidenceId"],
            "depthBasis": "survey TVD" if candidate["surveyed"] else "reported MD; no survey, assumed vertical",
        }
        for candidate in kept
    ]
    return points, [
        {"evidenceId": candidate["evidenceId"], "reason": candidate["reason"]}
        for candidate in excluded
    ]


def fluid_contacts(depths, curves, indices, stations):
    classes = []
    for index in indices:
        if any(index >= len(curves.get(name, [])) for name in ("PHIE", "PERM", "SW", "SO", "SG")):
            classes.append((depths[index], "tight"))
            continue
        porosity = curves.get("PHIE", [])[index]
        permeability = curves.get("PERM", [])[index] / PERMEABILITY_M2_PER_MD
        water = curves.get("SW", [])[index]
        oil = curves.get("SO", [])[index]
        gas = curves.get("SG", [])[index]
        values = (porosity, permeability, water, oil, gas)
        if not all(isinstance(value, (int, float)) and math.isfinite(value) for value in values):
            classes.append((depths[index], "tight"))
        elif porosity < 0.12 or permeability < 1:
            classes.append((depths[index], "tight"))
        elif water >= 0.55:
            classes.append((depths[index], "water"))
        elif gas >= 0.15 and gas >= oil:
            classes.append((depths[index], "gas"))
        elif oil >= 0.15:
            classes.append((depths[index], "oil"))
        else:
            classes.append((depths[index], "water"))

    smoothed = []
    phase_order = ("water", "oil", "gas")
    for index, (depth, center_phase) in enumerate(classes):
        neighborhood = [
            item[1]
            for item in classes[max(0, index - 1):index + 2]
            if item[1] != "tight"
        ]
        phase = max(
            phase_order,
            key=lambda candidate: (
                neighborhood.count(candidate),
                candidate == center_phase,
                -phase_order.index(candidate),
            ),
        ) if neighborhood else "tight"
        smoothed.append((depth, phase))
    transitions = []
    previous = None
    for depth, phase in smoothed:
        if phase == "tight":
            continue
        if previous and phase != previous[1]:
            contact_type = {
                ("gas", "water"): "gasWater",
                ("gas", "oil"): "gasOil",
                ("oil", "water"): "oilWater",
            }.get((previous[1], phase))
            if contact_type:
                measured_depth = (previous[0] + depth) / 2
                transitions.append((contact_type, interpolate_tvd(stations, measured_depth)))
        previous = (depth, phase)
    return transitions


NET_LITHOLOGY_CODES = (30000, 65030)
LEGACY_SHALE_POROSITY = 0.05
LEGACY_SHALE_PERMEABILITY_M2 = 1e-20


def lithology_split(curves, indices):
    """Split interval samples into (net, shale) by FORCE lithofacies; without lithology every sample is net."""
    lithology = curves.get("FORCE_2020_LITHOFACIES_LITHOLOGY", [])
    coded = [
        (index, round(lithology[index]))
        for index in indices
        if index < len(lithology) and isinstance(lithology[index], (int, float)) and math.isfinite(lithology[index])
    ]
    if not coded:
        return list(indices), []
    return (
        [index for index, code in coded if code in NET_LITHOLOGY_CODES],
        [index for index, code in coded if code not in NET_LITHOLOGY_CODES],
    )


def shale_properties(points):
    """Observed non-net (shale) log porosity mean and permeability geometric mean pooled over property controls.

    The factory mixes these with the net-sand means by net-to-gross, so at a control the world reproduces the
    well's own gross geometric-mean permeability. Falls back to the legacy constants without shale samples."""
    porosity = [value for point in points for value in point["_shalePorosity"]]
    permeability = [value for point in points for value in point["_shalePermeabilityM2"]]
    if not porosity or not permeability:
        return LEGACY_SHALE_POROSITY, LEGACY_SHALE_PERMEABILITY_M2, 0
    mean_porosity = min(max(sum(porosity) / len(porosity), 1e-3), 0.999)
    geometric_permeability = math.exp(sum(math.log(max(value, 1e-22)) for value in permeability) / len(permeability))
    return mean_porosity, min(max(geometric_permeability, 1e-22), 1e-14), len(permeability)


def conditioning_points(package, reservoir_name):
    well_bores, wells, clusters, trajectories = package_indexes(package)
    origin_easting, origin_northing = field_origin(package)
    points = []
    contacts = {"gasWater": [], "gasOil": [], "oilWater": []}
    for geology in package["geologicalProperties"]:
        well_bore_id = str(read(geology, "WellBoreID") or "")
        trajectory = trajectories.get(well_bore_id)
        interval = formation_interval(geology, reservoir_name)
        runs = read(read(geology, "Petrophysics"), "LogRuns") or []
        if not trajectory or not interval or not runs:
            continue
        run = runs[0]
        depths = read(run, "DepthValues") or []
        indices = [
            index
            for index, depth in enumerate(depths)
            if isinstance(depth, (int, float)) and interval[0] <= depth < interval[1]
        ]
        curves = {
            str(read(curve, "CanonicalMnemonic") or "").upper(): read(curve, "Values") or []
            for curve in read(run, "Curves") or []
        }
        net_indices, shale_indices = lithology_split(curves, indices)
        # The world factory treats control porosity/permeability as net-sand values and mixes shale back in by
        # net-to-gross, so gross-interval averages would count the shale twice (P9.11).
        porosity = finite_values(curves.get("PHIE", []), net_indices) or finite_values(curves.get("PHIE", []), indices)
        permeability = finite_values(curves.get("PERM", []), net_indices) or finite_values(curves.get("PERM", []), indices)
        water = finite_values(curves.get("SW", []), indices)
        gas = finite_values(curves.get("SG", []), indices)
        if not porosity or not permeability or not water or not gas:
            continue
        stations = read(trajectory, "SurveyStationList") or []
        surveyed_tvd = covering_survey_tvd(stations, *interval)
        if not surveyed_tvd:
            continue
        location = bore_location(
            well_bore_id,
            well_bores,
            wells,
            clusters,
            trajectory,
            sum(interval) / 2,
        )
        if not location:
            continue
        easting = location[0] - origin_easting
        northing = location[1] - origin_northing
        top_tvd, base_tvd = surveyed_tvd
        pressure_differentials = [
            gaussian_mean(read(entry, "PressureDifferential"))
            for entry in read(geology, "GeologicalPropertyTable") or []
            if interval[0] <= (gaussian_mean(read(entry, "MeasuredDepth")) or -math.inf) < interval[1]
        ]
        pressure_differentials = [value for value in pressure_differentials if value is not None]
        net_to_gross = (
            len(net_indices) / (len(net_indices) + len(shale_indices))
            if net_indices or shale_indices
            else 1.0
        )
        # A well can cross a contact more than once (tight or shaly streaks); its contact is the deepest crossing.
        deepest = {}
        for contact_type, depth in fluid_contacts(depths, curves, indices, stations):
            deepest[contact_type] = max(depth, deepest.get(contact_type, depth))
        for contact_type, depth in deepest.items():
            contacts[contact_type].append(
                {
                    "eastingM": easting,
                    "northingM": northing,
                    "contactDepthTvdM": depth,
                }
            )
        points.append(
            {
                "eastingM": easting,
                "northingM": northing,
                "reservoirTopDepthM": min(top_tvd, base_tvd),
                "reservoirBaseDepthM": max(top_tvd, base_tvd),
                "porosity": sum(porosity) / len(porosity),
                "permeabilityM2": math.exp(sum(math.log(max(value, 1e-22)) for value in permeability) / len(permeability)),
                "pressurePa": 10_000_000 + 9_500 * (top_tvd + base_tvd) / 2 +
                    (sum(pressure_differentials) / len(pressure_differentials) if pressure_differentials else 0),
                "waterSaturation": sum(water) / len(water),
                "gasSaturation": sum(gas) / len(gas),
                "netToGross": net_to_gross,
                "_shalePorosity": finite_values(curves.get("PHIE", []), shale_indices),
                "_shalePermeabilityM2": finite_values(curves.get("PERM", []), shale_indices),
                "evidenceId": f"geology:{record_id(geology)}",
            }
        )
    return points, contacts


def load_calibration(path, package, reservoir_name):
    content = Path(path).read_bytes()
    calibration = json.loads(content)
    field_id = package["fieldId"]
    if calibration.get("fieldId") != field_id:
        raise ValueError(f"Calibration field {calibration.get('fieldId')} does not match package field {field_id}")
    if str(calibration.get("reservoirName") or "").casefold() != reservoir_name.casefold():
        raise ValueError(f"Calibration reservoir {calibration.get('reservoirName')} does not match {reservoir_name}")
    if calibration.get("classification") != "ModelEstimated":
        raise ValueError("Calibration classification must remain ModelEstimated")
    if read(calibration.get("contactInitialization"), "Status") != "Sealed":
        raise ValueError("Calibration contact initialization must be sealed")
    origin_easting, origin_northing = field_origin(package)
    reference = calibration.get("coordinateReference") or {}
    if reference.get("frame") != "Field-local Riemannian east/north" or any(
        not isinstance(reference.get(name), (int, float)) or abs(reference[name] - expected) > 1e-6
        for name, expected in (
            ("originEastingM", origin_easting),
            ("originNorthingM", origin_northing),
        )
    ):
        raise ValueError("Calibration coordinate reference does not match Field.ReferencePoint")
    return calibration, hashlib.sha256(content).hexdigest()


def calibration_contacts(calibration):
    initialization = calibration["contactInitialization"]
    transition = read(read(initialization, "TransitionThickness"), "Value")

    def control_points(name):
        return [
            {key: value for key, value in point.items() if key != "evidenceId"}
            for point in read(initialization, name) or []
        ]

    return {
        "transitionThicknessM": transition,
        "gasWater": control_points("GasWaterControlPoints"),
        "gasOil": control_points("GasOilControlPoints"),
        "oilWater": control_points("OilWaterControlPoints"),
    }


def validate_detected_contacts(detected, sealed, tolerance_m=0.001):
    for contact_type in ("gasWater", "gasOil", "oilWater"):
        detected_depths = sorted(point["contactDepthTvdM"] for point in detected[contact_type])
        sealed_depths = sorted(point["contactDepthTvdM"] for point in sealed[contact_type])
        if len(detected_depths) != len(sealed_depths) or any(
            abs(left - right) > tolerance_m
            for left, right in zip(detected_depths, sealed_depths)
        ):
            raise ValueError(
                f"Detected {contact_type} controls no longer match the sealed calibration artifact"
            )


def world_request(package, reservoir_name, seed, count_x, count_y, count_z, calibration, calibration_sha256):
    structural_points, excluded_structural = structural_conditioning_points(package, reservoir_name)
    points, detected_contacts = conditioning_points(package, reservoir_name)
    if len(points) < 4:
        raise ValueError(f"{reservoir_name}: at least four logged conditioning controls are required, found {len(points)}")
    if len(structural_points) < len(points):
        raise ValueError(
            f"{reservoir_name}: structural controls ({len(structural_points)}) cannot be fewer than property controls ({len(points)})"
        )
    contact_options = calibration_contacts(calibration)
    validate_detected_contacts(detected_contacts, contact_options)
    shale_porosity, shale_permeability, shale_samples = shale_properties(points)
    return {
        "fieldId": package["fieldId"],
        "reservoirName": reservoir_name,
        "seed": seed,
        "calibrationArtifact": {
            "id": calibration["id"],
            "sha256": calibration_sha256,
        },
        "grid": {
            "countX": count_x,
            "countY": count_y,
            "countZ": count_z,
            "horizontalPaddingM": 2_000,
        },
        "heterogeneity": {
            "idwPower": 2,
            "spectralModeCount": 32,
            "correlationLengthXM": 1_400,
            "correlationLengthYM": 1_400,
            "correlationLengthZM": 12,
            "controlFadeDistanceM": 1_200,
            "topDepthStdDevM": 6,
            "baseDepthStdDevM": 8,
            "porosityStdDev": 0.025,
            "logPermeabilityStdDev": 0.8,
            "pressureStdDevPa": 750_000,
            "waterSaturationStdDev": 0.04,
            "gasSaturationStdDev": 0.03,
            "netToGrossStdDev": 0.12,
            "shalePorosity": shale_porosity,
            "shalePermeabilityM2": shale_permeability,
        },
        "structuralConditioningPoints": [
            {key: value for key, value in point.items() if key not in ("evidenceId", "depthBasis")}
            for point in structural_points
        ],
        "conditioningPoints": [
            {key: value for key, value in point.items() if key != "evidenceId" and not key.startswith("_")}
            for point in points
        ],
        "fluidContacts": contact_options,
    }, {
        "structuralControlRule": STRUCTURAL_CONTROL_RULE,
        "structuralEvidenceIds": [point["evidenceId"] for point in structural_points],
        "assumedVerticalStructuralEvidenceIds": [
            point["evidenceId"] for point in structural_points if point["depthBasis"] != "survey TVD"
        ],
        "excludedStructuralControls": excluded_structural,
        "propertyEvidenceIds": [point["evidenceId"] for point in points],
        "propertyConditioning": "net-sand porosity mean and permeability geometric mean; shale from observed non-net samples",
        "shaleSampleCount": shale_samples,
        "contactControlCounts": {key: len(value) for key, value in contact_options.items() if isinstance(value, list)},
    }


def main():
    parser = argparse.ArgumentParser(description="Condition a hidden reservoir world from a live DrillSim field package.")
    parser.add_argument("--field-id", default=DEFAULT_FIELD_ID)
    parser.add_argument("--reservoir", default=DEFAULT_RESERVOIR)
    parser.add_argument("--seed", type=int, default=20260903)
    parser.add_argument("--grid", type=int, nargs=3, default=(64, 64, 20), metavar=("NX", "NY", "NZ"))
    parser.add_argument("--calibration", default=str(DEFAULT_CALIBRATION))
    parser.add_argument("--self-check", action="store_true")
    args = parser.parse_args()
    if args.self_check:
        assert read({"MetaInfo": {"ID": "x"}}, "metaInfo") == {"ID": "x"}
        assert abs(interpolate_tvd([{"MD": 0, "TVD": 0}, {"MD": 100, "TVD": 90}], 50) - 45) < 1e-12
        assert covering_survey_tvd([], 10, 20) is None
        assert covering_survey_tvd([{"MD": 0, "TVD": 0}, {"MD": 100, "TVD": 90}], 10, 120) is None
        surveyed = [
            {"top": 1500 + 10 * index, "base": 1600 + 15 * index, "surveyed": True, "wellId": f"w{index}"}
            for index in range(5)
        ]
        inlier = {"top": 1520, "base": 1640, "surveyed": False, "wellId": "vertical"}
        thick = {"top": 1621, "base": 2603, "surveyed": False, "wellId": "deviated"}
        sidetrack = {"top": 1500, "base": 1600, "surveyed": False, "wellId": "w0"}
        kept, excluded = select_structural_controls(surveyed + [inlier, thick, sidetrack])
        assert inlier in kept and thick not in kept and sidetrack not in kept and len(excluded) == 2
        assert select_structural_controls(surveyed[:3] + [thick])[0][-1] is thick
        lith = {"FORCE_2020_LITHOFACIES_LITHOLOGY": [30000, 65000, 65030, None, 70000]}
        assert lithology_split(lith, range(5)) == ([0, 2], [1, 4])
        assert lithology_split({}, range(3)) == ([0, 1, 2], [])
        controls = [{"_shalePorosity": [0.2, 0.22], "_shalePermeabilityM2": [1e-18, 1e-16]}]
        porosity, permeability, count = shale_properties(controls)
        assert abs(porosity - 0.21) < 1e-12 and abs(permeability - 1e-17) < 1e-27 and count == 2
        assert shale_properties([{"_shalePorosity": [], "_shalePermeabilityM2": []}])[:2] == (0.05, 1e-20)
        print("self-check passed")
        return

    operator_key = os.environ.get("RESERVOIR_SIMULATION_OPERATOR_KEY")
    if not operator_key:
        raise ValueError("RESERVOIR_SIMULATION_OPERATOR_KEY is required")
    urls = discover_urls()
    package = request_json(f"{urls['analysis-api']}/api/fields/{args.field_id}/package")
    calibration, calibration_sha256 = load_calibration(args.calibration, package, args.reservoir)
    request, evidence = world_request(
        package,
        args.reservoir,
        args.seed,
        *args.grid,
        calibration,
        calibration_sha256,
    )
    summary = request_json(
        f"{urls['reservoir-simulation']}/reservoirsimulation/api/worlds",
        "POST",
        request,
        {"X-DrillSim-Operator-Key": operator_key},
    )
    print(json.dumps({"world": summary, "conditioning": evidence}, indent=2))


if __name__ == "__main__":
    main()
