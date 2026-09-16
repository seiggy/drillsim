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
    well_bores, wells, clusters, trajectories = package_indexes(package)
    origin_easting, origin_northing = field_origin(package)
    points = []
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
        top_tvd = interpolate_tvd(stations, interval[0]) if stations else interval[0]
        base_tvd = interpolate_tvd(stations, interval[1]) if stations else interval[1]
        points.append(
            {
                "eastingM": location[0] - origin_easting,
                "northingM": location[1] - origin_northing,
                "reservoirTopDepthM": min(top_tvd, base_tvd),
                "reservoirBaseDepthM": max(top_tvd, base_tvd),
                "evidenceId": f"geology:{record_id(geology)}",
            }
        )
    return points


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
        porosity = finite_values(curves.get("PHIE", []), indices)
        permeability = finite_values(curves.get("PERM", []), indices)
        water = finite_values(curves.get("SW", []), indices)
        gas = finite_values(curves.get("SG", []), indices)
        if not porosity or not permeability or not water or not gas:
            continue
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
        easting = location[0] - origin_easting
        northing = location[1] - origin_northing
        top_tvd = interpolate_tvd(stations, interval[0])
        base_tvd = interpolate_tvd(stations, interval[1])
        pressure_differentials = [
            gaussian_mean(read(entry, "PressureDifferential"))
            for entry in read(geology, "GeologicalPropertyTable") or []
            if interval[0] <= (gaussian_mean(read(entry, "MeasuredDepth")) or -math.inf) < interval[1]
        ]
        pressure_differentials = [value for value in pressure_differentials if value is not None]
        lithology = finite_values(curves.get("FORCE_2020_LITHOFACIES_LITHOLOGY", []), indices)
        net_to_gross = (
            sum(round(value) in (30000, 65030) for value in lithology) / len(lithology)
            if lithology
            else 1.0
        )
        for contact_type, depth in fluid_contacts(depths, curves, indices, stations):
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
    gas_water = read(initialization, "GasWaterControlPoints") or []
    return {
        "transitionThicknessM": transition,
        "gasWater": [
            {key: value for key, value in point.items() if key != "evidenceId"}
            for point in gas_water
        ],
        "gasOil": [],
        "oilWater": [],
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
    structural_points = structural_conditioning_points(package, reservoir_name)
    points, detected_contacts = conditioning_points(package, reservoir_name)
    if len(points) < 4:
        raise ValueError(f"{reservoir_name}: at least four logged conditioning controls are required, found {len(points)}")
    if len(structural_points) < len(points):
        raise ValueError(
            f"{reservoir_name}: structural controls ({len(structural_points)}) cannot be fewer than property controls ({len(points)})"
        )
    contact_options = calibration_contacts(calibration)
    validate_detected_contacts(detected_contacts, contact_options)
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
            "shalePorosity": 0.05,
            "shalePermeabilityM2": 1e-20,
        },
        "structuralConditioningPoints": [
            {key: value for key, value in point.items() if key != "evidenceId"}
            for point in structural_points
        ],
        "conditioningPoints": [
            {key: value for key, value in point.items() if key != "evidenceId"}
            for point in points
        ],
        "fluidContacts": contact_options,
    }, {
        "structuralEvidenceIds": [point["evidenceId"] for point in structural_points],
        "propertyEvidenceIds": [point["evidenceId"] for point in points],
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
