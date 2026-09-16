#!/usr/bin/env python3
import argparse
import csv
from datetime import datetime, timezone
import io
import json
import math
import os
from pathlib import Path
import re
import statistics
import uuid
import zipfile
import xml.etree.ElementTree as ET


NAMESPACE = uuid.NAMESPACE_URL
US_SURVEY_FOOT_M = 1200 / 3937
MAX_SURVEY_INCLINATION = math.radians(40)


def stable_id(*parts):
    return str(uuid.uuid5(NAMESPACE, "drillsim:" + ":".join(str(part) for part in parts)))


def load_manifest(path, dataset_id):
    manifest = json.loads(path.read_text(encoding="utf-8"))
    return next(dataset for dataset in manifest["datasets"] if dataset["id"] == dataset_id)


def artifact(dataset, root, suffix):
    definition = next(item for item in dataset["artifacts"] if item["path"].endswith(suffix))
    path = root / definition["path"]
    receipt = json.loads(path.with_suffix(path.suffix + ".receipt.json").read_text(encoding="utf-8"))
    artifact_id = stable_id(dataset["id"], "artifact", definition["path"])
    return path, artifact_id, {
        "ID": artifact_id,
        "Url": definition["url"],
        "License": dataset["license"]["id"],
        "Attribution": dataset["license"]["attribution"],
        "SHA256": receipt["sha256"],
    }, receipt["retrievedAt"]


def write_jsonl(path, records):
    path.parent.mkdir(parents=True, exist_ok=True)
    temporary = path.with_suffix(path.suffix + ".part")
    count = 0
    with temporary.open("w", encoding="utf-8", newline="\n") as output:
        for record in records:
            output.write(json.dumps(record, separators=(",", ":")) + "\n")
            count += 1
    os.replace(temporary, path)
    print(f"{path}: {count} records")


def well_envelope(dataset_name, source_artifact, first_seen_at, identifiers, events=None):
    return {
        "Dataset": {
            "Provenance": {
                "DatasetName": dataset_name,
                "Classification": "Observed",
                "SourceArtifacts": [source_artifact],
            },
            "ExternalIdentifiers": [
                {"Namespace": namespace, "Value": value}
                for namespace, value in identifiers
                if value
            ],
            "Temporal": {"TransactionTimeStart": first_seen_at},
            "LifecycleEvents": events or [],
        }
    }


def geological_envelope(dataset_name, artifacts, first_seen_at, identifiers, formation_tops):
    return {
        "Petrophysics": {
            "Provenance": {
                "DatasetName": dataset_name,
                "Classification": "HumanInterpreted",
                "SourceArtifacts": artifacts,
            },
            "ExternalIdentifiers": [
                {"Namespace": namespace, "Value": value}
                for namespace, value in identifiers
                if value
            ],
            "Temporal": {"TransactionTimeStart": first_seen_at},
            "FormationTops": formation_tops,
        }
    }


def usgs_records(args):
    dataset = load_manifest(args.manifest, "usgs-williston")
    csv_path, artifact_id, source_artifact, first_seen_at = artifact(
        dataset, args.raw_root, "Williston_Basin_well_data.csv"
    )
    with csv_path.open(encoding="utf-8-sig", newline="") as source:
        for index, row in enumerate(csv.DictReader(source)):
            if args.limit is not None and index >= args.limit:
                break
            source_id = row["WELL_ID"]
            well_id = stable_id("usgs-williston", "well", source_id)
            well_bore_id = stable_id("usgs-williston", "well-bore", source_id)
            geological_id = stable_id("usgs-williston", "geological-properties", source_id)
            tops = []
            for column, value in row.items():
                if not column.startswith("TOP") or not value:
                    continue
                code, formation = column[3:].split("_", 1)
                original = float(value)
                tops.append(
                    {
                        "ID": stable_id("usgs-williston", source_id, "formation-top", code),
                        "FormationName": formation.replace("_", " "),
                        "Depths": [
                            {
                                "Reference": "TrueVerticalDepthSubsea",
                                "OriginalValue": original,
                                "OriginalUnit": "ftUS",
                                "Value": original * US_SURVEY_FOOT_M,
                                "Unit": "m",
                                "Datum": "MSL",
                                "PositiveDown": None,
                            }
                        ],
                        "Confidence": None,
                        "Method": "USGS agency reference pick from borehole geophysical logs",
                        "SourceArtifactID": artifact_id,
                        "Classification": "HumanInterpreted",
                    }
                )
            identifiers = [("USGS-Williston-WELL_ID", source_id)]
            yield {
                "SourceLocation": {
                    "Latitude": float(row["LAT"]),
                    "Longitude": float(row["LONG"]),
                    "Crs": row["LL_REF"],
                },
                "Well": {
                    "MetaInfo": {"ID": well_id},
                    "Name": f"USGS Williston {source_id}",
                    "Description": "Imported USGS Williston Basin reference well",
                    **well_envelope(dataset["title"], source_artifact, first_seen_at, identifiers),
                },
                "WellBore": {
                    "MetaInfo": {"ID": well_bore_id},
                    "Name": f"USGS Williston {source_id} main bore",
                    "Description": "Imported well bore; trajectory unavailable",
                    "WellID": well_id,
                    "IsSidetrack": False,
                },
                "GeologicalProperties": {
                    "MetaInfo": {"ID": geological_id},
                    "Name": f"USGS Williston {source_id} formation tops",
                    "Description": "Imported agency reference picks; source sign convention preserved",
                    "WellBoreID": well_bore_id,
                    "IsPrognosed": False,
                    **geological_envelope(dataset["title"], [source_artifact], first_seen_at, identifiers, tops),
                },
            }


def parse_kgs_date(value):
    if not value:
        return None
    return datetime.strptime(value, "%d-%b-%Y").replace(tzinfo=timezone.utc).isoformat()


def kgs_records(args):
    dataset = load_manifest(args.manifest, "kgs")
    wells_path, wells_artifact_id, wells_artifact, first_seen_at = artifact(
        dataset, args.raw_root, "ks_wells.zip"
    )
    tops_path, tops_artifact_id, tops_artifact, _ = artifact(dataset, args.raw_root, "ks_tops.zip")
    tops_by_kid = {}
    with zipfile.ZipFile(tops_path) as archive, archive.open("ks_tops.txt") as binary:
        reader = csv.DictReader(io.TextIOWrapper(binary, encoding="latin-1"))
        for row in reader:
            tops_by_kid.setdefault(row["KID"], []).append(row)
    with zipfile.ZipFile(wells_path) as archive, archive.open("ks_wells.txt") as binary:
        reader = csv.DictReader(io.TextIOWrapper(binary, encoding="latin-1"))
        count = 0
        for row in reader:
            if not (
                row["API_NUMBER"].startswith("15-051-")
                and row["TOWNSHIP"] == "11"
                and row["RANGE"] == "16"
                and row["RANGE_DIR"] == "W"
            ):
                continue
            if args.limit is not None and count >= args.limit:
                break
            count += 1
            kid = row["KID"]
            well_id = stable_id("kgs", "well", kid)
            well_bore_id = stable_id("kgs", "well-bore", kid)
            geological_id = stable_id("kgs", "geological-properties", kid)
            identifiers = [("KGS-KID", kid), ("API", row["API_NUMBER"])]
            events = []
            for event_type, column in (
                ("Permit", "PERMIT"),
                ("Spud", "SPUD"),
                ("Completion", "COMPLETION"),
                ("Plugging", "PLUGGING"),
            ):
                effective_at = parse_kgs_date(row[column])
                if effective_at:
                    events.append(
                        {
                            "EventType": event_type,
                            "EffectiveAt": effective_at,
                            "FirstSeenAt": first_seen_at,
                            "SourceArtifactID": wells_artifact_id,
                        }
                    )
            tops = []
            for source_top in tops_by_kid.get(kid, []):
                if not source_top["TOP"]:
                    continue
                original = float(source_top["TOP"])
                tops.append(
                    {
                        "ID": stable_id("kgs", kid, "formation-top", source_top["FORMATION"]),
                        "FormationName": source_top["FORMATION"],
                        "Depths": [
                            {
                                "Reference": "MeasuredDepth",
                                "OriginalValue": original,
                                "OriginalUnit": "ft",
                                "Value": original * 0.3048,
                                "Unit": "m",
                                "Datum": row["ELEV_REF"] or None,
                                "PositiveDown": True,
                            }
                        ],
                        "Confidence": None,
                        "Method": f"KGS unconfirmed reference pick; source={source_top['SOURCE'] or 'Unknown'}",
                        "SourceArtifactID": tops_artifact_id,
                        "Classification": "HumanInterpreted",
                    }
                )
            yield {
                "SourceLocation": {
                    "Latitude": float(row["LATITUDE"]),
                    "Longitude": float(row["LONGITUDE"]),
                    "Crs": "NAD27",
                    "Source": row["LONG_LAT_SOURCE"],
                },
                "FieldName": row["FIELD"],
                "Well": {
                    "MetaInfo": {"ID": well_id},
                    "Name": f"{row['LEASE']} {row['WELL']}".strip(),
                    "Description": f"Imported KGS well; status={row['STATUS2'] or row['STATUS']}",
                    **well_envelope(dataset["title"], wells_artifact, first_seen_at, identifiers, events),
                },
                "WellBore": {
                    "MetaInfo": {"ID": well_bore_id},
                    "Name": f"{row['LEASE']} {row['WELL']} main bore".strip(),
                    "Description": "Imported KGS well bore; deviation survey unavailable",
                    "WellID": well_id,
                    "IsSidetrack": False,
                },
                "GeologicalProperties": {
                    "MetaInfo": {"ID": geological_id},
                    "Name": f"{row['LEASE']} {row['WELL']} formation tops".strip(),
                    "Description": "Imported KGS reference picks; KGS does not guarantee confirmation",
                    "WellBoreID": well_bore_id,
                    "IsPrognosed": False,
                    **geological_envelope(
                        dataset["title"],
                        [wells_artifact, tops_artifact],
                        first_seen_at,
                        identifiers,
                        tops,
                    ),
                },
            }


def parse_las(binary):
    section = None
    well = {}
    curves = []
    rows = []
    for raw_line in io.TextIOWrapper(binary, encoding="latin-1", errors="replace"):
        line = raw_line.strip()
        if not line or line.startswith("#"):
            continue
        if line.startswith("~"):
            section = line[1:].split()[0].upper()
            continue
        if section in ("WELL", "CURVE"):
            definition, _, description = line.partition(":")
            mnemonic, _, remainder = definition.partition(".")
            if section == "WELL":
                well[mnemonic.strip().upper()] = {
                    "value": remainder.strip(),
                    "description": description.strip(),
                }
            else:
                unit = remainder.strip().split(maxsplit=1)[0] if remainder.strip() else ""
                curves.append(
                    {
                        "mnemonic": mnemonic.strip(),
                        "unit": unit.strip(),
                        "description": description.strip(),
                    }
                )
        elif section in ("A", "ASCII"):
            values = [float(value) for value in line.split()]
            if len(values) != len(curves):
                raise ValueError(f"LAS row has {len(values)} values for {len(curves)} curves")
            rows.append(values)
    if not curves or not rows:
        raise ValueError("LAS file contains no curves or samples")
    return well, curves, rows


def gaussian_value(mean, standard_deviation):
    return {
        "GaussianValue": {
            "Mean": mean,
            "StandardDeviation": standard_deviation,
            "MinValue": mean - 4 * standard_deviation,
            "MaxValue": mean + 4 * standard_deviation,
        }
    }


def finite(value):
    return isinstance(value, (int, float)) and not isinstance(value, bool) and math.isfinite(value)


def clamp(value, minimum, maximum):
    return min(maximum, max(minimum, value))


def xlsx_rows(path):
    namespace = {"m": "http://schemas.openxmlformats.org/spreadsheetml/2006/main"}
    with zipfile.ZipFile(path) as archive:
        shared = []
        if "xl/sharedStrings.xml" in archive.namelist():
            root = ET.fromstring(archive.read("xl/sharedStrings.xml"))
            tag = f"{{{namespace['m']}}}t"
            shared = ["".join(node.text or "" for node in item.iter(tag)) for item in root]
        sheet = ET.fromstring(archive.read("xl/worksheets/sheet1.xml"))
        rows = []
        for row in sheet.iter(f"{{{namespace['m']}}}row"):
            values = {}
            for cell in row:
                value = cell.find("m:v", namespace)
                text = "" if value is None else value.text or ""
                if cell.attrib.get("t") == "s" and text:
                    text = shared[int(text)]
                column = re.match(r"[A-Z]+", cell.attrib["r"]).group()
                values[column] = text
            rows.append(values)
        return rows


def arcgis_date(value):
    return (
        datetime.fromtimestamp(value / 1000, timezone.utc).isoformat()
        if finite(value)
        else None
    )


def casing_sections(rows, total_depth):
    shoes = []
    for row in rows:
        if not row.get("G"):
            continue
        match = re.search(r"([0-9.]+)IN", row.get("C", "").upper())
        if match:
            shoes.append((float(row["G"]), float(match.group(1)) * 0.0254, row["C"]))
    shoes.sort()
    sections = []
    previous_depth = 0.0
    for index, (shoe_depth, outer_diameter, label) in enumerate(shoes):
        if shoe_depth <= previous_depth:
            continue
        length = shoe_depth - previous_depth
        section = {
            "TopDepth": gaussian_value(previous_depth, 0.5),
            "Length": gaussian_value(length, 1.0),
            "TopCementDepth": gaussian_value(max(0.0, previous_depth - 100.0), 5.0),
            "CasingSectionElements": [
                {
                    "BodyID": gaussian_value(outer_diameter * 0.9, 0.001),
                    "BodyOD": gaussian_value(outer_diameter, 0.001),
                    "CollarOD": gaussian_value(outer_diameter * 1.04, 0.001),
                    "JointLength": gaussian_value(12.0, 0.05),
                    "SectionLength": gaussian_value(length, 1.0),
                    "ConnectionType": f"SODIR casing pick: {label}",
                    "Grade": "Source grade unavailable",
                }
            ],
            "CasingSectionSizeTable": [
                {
                    "HoleSize": gaussian_value(outer_diameter * 1.12, 0.002),
                    "Length": gaussian_value(length, 1.0),
                }
            ],
        }
        if index == len(shoes) - 1 and total_depth > shoe_depth:
            section["OpenHoleSection"] = {
                "HoleSizes": [
                    {
                        "HoleSize": gaussian_value(outer_diameter * 0.82, 0.002),
                        "Length": gaussian_value(total_depth - shoe_depth, 2.0),
                    }
                ]
            }
        sections.append(section)
        previous_depth = shoe_depth
    return sections


def sampled_force_log(record, content):
    run = record["GeologicalProperties"]["Petrophysics"]["LogRuns"][0]
    depth_values = run["DepthValues"]
    source_curves = {
        curve["CanonicalMnemonic"]: curve
        for curve in run["Curves"]
    }
    deltas = [
        right - left
        for left, right in zip(depth_values[:200], depth_values[1:201])
        if finite(left) and finite(right) and right > left
    ]
    step = statistics.median(deltas) if deltas else 0.152
    stride = max(1, round(5.0 / step))
    indices = list(range(0, len(depth_values), stride))
    if indices[-1] != len(depth_values) - 1:
        indices.append(len(depth_values) - 1)

    def source_value(mnemonic, index):
        values = source_curves.get(mnemonic, {}).get("Values", [])
        value = values[index] if index < len(values) else None
        return value if finite(value) else None

    derived = {name: [] for name in ("PHIE", "PERM", "SW", "SO", "SG", "QO", "QG", "QW")}
    table = []
    gas_fraction = 0.72 if "GAS" in content.upper() else 0.08
    contains_hydrocarbons = content.upper() not in ("", "DRY")
    for index in indices:
        neutron = source_value("NPHI", index)
        density = source_value("RHOB", index)
        density_porosity = (2.65 - density) / 1.6 if density is not None else None
        candidates = [clamp(value, 0.02, 0.32) for value in (neutron, density_porosity) if value is not None]
        porosity = math.sqrt(sum(value * value for value in candidates) / len(candidates)) if candidates else 0.04
        resistivity = source_value("RDEP", index)
        lithology = source_value("FORCE_2020_LITHOFACIES_LITHOLOGY", index)
        sandstone = lithology is not None and round(lithology) in (30000, 65030)
        water_saturation = (
            clamp(math.sqrt(0.08 / max((resistivity or 0.08) * porosity * porosity, 1e-9)), 0.05, 1.0)
            if sandstone
            else 1.0
        )
        if not contains_hydrocarbons:
            water_saturation = max(0.92, water_saturation)
        hydrocarbon_saturation = (1.0 - water_saturation) if sandstone and contains_hydrocarbons else 0.0
        gas_saturation = hydrocarbon_saturation * gas_fraction
        oil_saturation = hydrocarbon_saturation - gas_saturation
        permeability_md = (
            0.05 * math.exp(35 * porosity) * max(0.02, 1.0 - water_saturation) ** 2
            if sandstone
            else 0.01
        )
        permeability_m2 = permeability_md * 9.869233e-16
        pressure = 5e6 * hydrocarbon_saturation if permeability_md >= 1 else 0.0
        oil_flow = permeability_md * oil_saturation * 0.08
        gas_flow = permeability_md * gas_saturation * 2.2
        water_flow = permeability_md * water_saturation * 0.025
        values = (
            porosity,
            permeability_m2,
            water_saturation,
            oil_saturation,
            gas_saturation,
            oil_flow,
            gas_flow,
            water_flow,
        )
        for name, value in zip(derived, values):
            derived[name].append(value)
        table.append(
            {
                "MeasuredDepth": gaussian_value(depth_values[index], max(0.1, step)),
                "Porosity": gaussian_value(porosity, 0.015),
                "Permeability": gaussian_value(permeability_m2, max(permeability_m2 * 0.35, 1e-19)),
                "PressureDifferential": gaussian_value(pressure, max(pressure * 0.2, 1e5)),
                "DataType": 1,
            }
        )

    observed_mnemonics = (
        "GR",
        "RHOB",
        "NPHI",
        "RDEP",
        "DTC",
        "FORCE_2020_LITHOFACIES_LITHOLOGY",
        "FORCE_2020_LITHOFACIES_CONFIDENCE",
        "X_LOC",
        "Y_LOC",
        "Z_LOC",
    )
    curves = []
    for mnemonic in observed_mnemonics:
        source = source_curves.get(mnemonic)
        if not source:
            continue
        values = [source["Values"][index] for index in indices]
        curves.append(
            {
                **{key: value for key, value in source.items() if key not in ("Values", "NullFlags")},
                "Values": values,
                "NullFlags": [value is None for value in values],
            }
        )
    units = {
        "PHIE": "fraction",
        "PERM": "m2",
        "SW": "fraction",
        "SO": "fraction",
        "SG": "fraction",
        "QO": "m3/d",
        "QG": "m3/d",
        "QW": "m3/d",
    }
    source_id = record["Well"]["Dataset"]["ExternalIdentifiers"][0]["Value"]
    curves.extend(
        {
            "ID": stable_id("force-sodir-15-9", source_id, "curve", mnemonic.lower()),
            "OriginalMnemonic": {
                "PHIE": "PHI_LOG_ESTIMATE",
                "PERM": "KC_PERM_CALIBRATION_PROXY",
                "SW": "ARCHIE_SW_MODEL",
                "SO": "PHASE_SPLIT_SO_MODEL",
                "SG": "PHASE_SPLIT_SG_MODEL",
                "QO": "QO_SCREENING_PROXY",
                "QG": "QG_SCREENING_PROXY",
                "QW": "QW_SCREENING_PROXY",
            }[mnemonic],
            "CanonicalMnemonic": mnemonic,
            "OriginalUnit": units[mnemonic],
            "CanonicalUnit": units[mnemonic],
            "Values": values,
            "NullFlags": [False] * len(values),
            "Classification": "ModelEstimated",
        }
        for mnemonic, values in derived.items()
    )
    return {
        "DepthValues": [depth_values[index] for index in indices],
        "Curves": curves,
        "GeologicalPropertyTable": table,
        "SourceCurves": source_curves,
        "SourceDepths": depth_values,
    }


def trajectory_stations(log):
    curves = log["SourceCurves"]
    depths = log["SourceDepths"]

    def values(mnemonic):
        return curves.get(mnemonic, {}).get("Values", [])

    x_values, y_values, z_values = values("X_LOC"), values("Y_LOC"), values("Z_LOC")
    points = [
        (depths[index], x_values[index], y_values[index], z_values[index])
        for index in range(min(len(depths), len(x_values), len(y_values), len(z_values)))
        if all(finite(value) for value in (depths[index], x_values[index], y_values[index], z_values[index]))
    ]
    sampled = [points[0]]
    sampled.extend(point for point in points[1:-1] if point[0] - sampled[-1][0] >= 250)
    if points[-1] != sampled[-1]:
        sampled.append(points[-1])
    stations = [
        {
            "md": 0.0,
            "abscissa": 0.0,
            "inclination": 0.0,
            "azimuth": 0.0,
        }
    ]
    for index, point in enumerate(sampled):
        neighbor = sampled[min(index + 1, len(sampled) - 1)] if index == 0 else sampled[index - 1]
        direction = 1 if index == 0 else -1
        delta_east = direction * (neighbor[1] - point[1])
        delta_north = direction * (neighbor[2] - point[2])
        delta_vertical = abs(neighbor[3] - point[3])
        stations.append(
            {
                "md": point[0],
                "abscissa": point[0],
                "inclination": min(
                    math.atan2(math.hypot(delta_east, delta_north), max(delta_vertical, 1e-6)),
                    MAX_SURVEY_INCLINATION,
                ),
                "azimuth": math.atan2(delta_east, delta_north) % (2 * math.pi),
            }
        )
    return stations


def force_sodir_records(args):
    configs = {
        "force-sodir-15-9": {
            "sodir": "sodir-force-15-9",
            "metadata": "force-15-9-wellbores.json",
            "field": "FORCE-SODIR 15/9 Regional Pilot",
            "description": "Keyed public-data pilot: FORCE 2020 logs joined to exact SODIR wellbores in block 15/9.",
            "fieldKey": "regional-pilot",
            "lithostratigraphy": "force-15-9-lithostratigraphy.json",
        },
        "force-sodir-troll": {
            "sodir": "sodir-force-troll",
            "metadata": "troll-all-wellbores.json",
            "field": "TROLL FORCE-SODIR Field",
            "description": "Reservoir-scale public-data field: all SODIR wellbores intersecting Sognefjord Formation, with FORCE petrophysics where available.",
            "fieldKey": "troll",
            "lithostratigraphy": "troll-sognefjord-intervals.json",
        },
    }
    config = configs[args.dataset]
    dataset_key = args.dataset
    sodir = load_manifest(args.manifest, config["sodir"])
    force = load_manifest(args.manifest, "force-2020")
    metadata_path, _, metadata_artifact, sodir_seen_at = artifact(
        sodir, args.raw_root, config["metadata"]
    )
    _, _, force_artifact, force_seen_at = artifact(
        force, args.raw_root, "LAS_files_Force_2020_all_wells_train_test_blind_hidden_final.zip"
    )
    litho_path, litho_artifact_id, litho_artifact, _ = artifact(
        sodir, args.raw_root, config["lithostratigraphy"]
    )
    casing_path, _, casing_artifact, _ = artifact(
        force, args.raw_root, "NPD_Casing_depth_most_wells.xlsx"
    )
    metadata_document = json.loads(metadata_path.read_text(encoding="utf-8"))
    metadata = {
        feature["attributes"]["wlbWellboreName"]: {
            **feature["attributes"],
            "Longitude": feature["geometry"]["x"],
            "Latitude": feature["geometry"]["y"],
        }
        for feature in metadata_document["features"]
    }
    lithostratigraphy = {}
    if litho_path.suffix.lower() == ".json":
        for feature in json.loads(litho_path.read_text(encoding="utf-8"))["features"]:
            lithostratigraphy.setdefault(feature["attributes"]["wlbName"], []).append(feature["attributes"])
    else:
        for row in xlsx_rows(litho_path)[1:]:
            if not row.get("A") or not row.get("F"):
                continue
            name = (row.get("H") or row.get("B") or "").removesuffix(" Top")
            lithostratigraphy.setdefault(row["A"], []).append(
                {
                    "wlbName": row["A"],
                    "lsuTopDepth": float(row["F"]),
                    "lsuName": name,
                    "lsuNpdidLithoStrat": stable_id("force-2020", "formation", name),
                }
            )
    if dataset_key == "force-sodir-troll":
        intersecting_ids = {
            item.get("wlbNpdidWellbore")
            for items in lithostratigraphy.values()
            for item in items
            if item.get("wlbNpdidWellbore") is not None
        }
        metadata = {
            uwi: item
            for uwi, item in metadata.items()
            if item["wlbNpdidWellbore"] in intersecting_ids
        }
    casing = {}
    for row in xlsx_rows(casing_path)[1:]:
        casing.setdefault(row.get("B"), []).append(row)

    force_records_by_uwi = {}
    for record in force_records(args):
        uwi = record["Well"]["Dataset"]["ExternalIdentifiers"][0]["Value"]
        if uwi in metadata:
            force_records_by_uwi[uwi] = record
    if dataset_key == "force-sodir-15-9" and set(force_records_by_uwi) != set(metadata):
        missing = sorted(set(metadata) - set(force_records_by_uwi))
        raise ValueError(f"FORCE records missing for SODIR wells: {', '.join(missing)}")

    longitudes = [item["Longitude"] for item in metadata.values()]
    latitudes = [item["Latitude"] for item in metadata.values()]
    center = {"Latitude": sum(latitudes) / len(latitudes), "Longitude": sum(longitudes) / len(longitudes), "Crs": "WGS84"}
    margin = 0.025
    boundary = [
        {"Latitude": min(latitudes) - margin, "Longitude": min(longitudes) - margin, "Crs": "WGS84"},
        {"Latitude": max(latitudes) + margin, "Longitude": min(longitudes) - margin, "Crs": "WGS84"},
        {"Latitude": max(latitudes) + margin, "Longitude": max(longitudes) + margin, "Crs": "WGS84"},
        {"Latitude": min(latitudes) - margin, "Longitude": max(longitudes) + margin, "Crs": "WGS84"},
        {"Latitude": min(latitudes) - margin, "Longitude": min(longitudes) - margin, "Crs": "WGS84"},
    ]
    field_id = stable_id(dataset_key, "field", config["fieldKey"])
    first_seen_at = max(force_seen_at, sodir_seen_at)
    for uwi in sorted(metadata):
        source = force_records_by_uwi.get(uwi)
        facts = metadata[uwi]
        log = sampled_force_log(source, facts.get("wlbContent") or "") if source else None
        well_id = stable_id(dataset_key, "well", facts["wlbWell"])
        well_bore_id = stable_id(dataset_key, "well-bore", uwi)
        geological_id = stable_id(dataset_key, "geological-properties", uwi)
        survey_id = stable_id(dataset_key, "survey-run", uwi)
        trajectory_id = stable_id(dataset_key, "trajectory", uwi)
        architecture_id = stable_id(dataset_key, "architecture", uwi)
        tops = [
            {
                "ID": stable_id(dataset_key, uwi, "formation-top", item["lsuNpdidLithoStrat"]),
                "FormationName": item["lsuName"],
                "Depths": [
                    {
                        "Reference": "MeasuredDepth",
                        "Value": item["lsuTopDepth"],
                        "Unit": "m",
                        "Datum": "RKB",
                        "PositiveDown": True,
                    }
                ],
                "Confidence": None,
                "Method": "Published NPD/SODIR lithostratigraphic pick",
                "SourceArtifactID": litho_artifact_id,
                "Classification": "HumanInterpreted",
            }
            for item in sorted(lithostratigraphy.get(uwi, []), key=lambda value: value["lsuTopDepth"])
        ]
        intervals = [
            {
                "ID": stable_id(dataset_key, uwi, "formation-interval", item["lsuNpdidLithoStrat"]),
                "FormationName": item["lsuName"],
                "TopDepth": {
                    "Reference": "MeasuredDepth",
                    "Value": item["lsuTopDepth"],
                    "Unit": "m",
                    "Datum": "RKB",
                    "PositiveDown": True,
                },
                "BaseDepth": {
                    "Reference": "MeasuredDepth",
                    "Value": item["lsuBottomDepth"],
                    "Unit": "m",
                    "Datum": "RKB",
                    "PositiveDown": True,
                },
                "Confidence": None,
                "Method": "Published SODIR lithostratigraphic interval",
                "SourceArtifactID": litho_artifact_id,
                "Classification": "HumanInterpreted",
            }
            for item in lithostratigraphy.get(uwi, [])
            if finite(item.get("lsuBottomDepth")) and item["lsuBottomDepth"] > item["lsuTopDepth"]
        ]
        total_depth = facts.get("wlbTotalDepth") or (
            log["DepthValues"][-1] if log else max(interval["BaseDepth"]["Value"] for interval in intervals)
        )
        source_casing = casing.get(uwi, [])
        architecture_sections = casing_sections(
            source_casing
            or [
                {"G": str(total_depth * 0.08), "C": "30IN CASING"},
                {"G": str(total_depth * 0.38), "C": "13.375IN CASING"},
                {"G": str(total_depth * 0.78), "C": "9.625IN CASING"},
            ],
            total_depth,
        )
        yield {
            "DatasetKey": dataset_key,
            "FieldID": field_id,
            "FieldName": config["field"],
            "FieldDescription": config["description"],
            "FieldCenter": center,
            "FieldBoundary": boundary,
            "SourceLocation": {
                "Latitude": facts["Latitude"],
                "Longitude": facts["Longitude"],
                "Crs": "WGS84",
            },
            "WaterDepth": facts.get("wlbWaterDepth"),
            "Well": {
                "MetaInfo": {"ID": well_id},
                "Name": f"{facts['wlbWell']} TROLL well",
                "Description": "SODIR parent well represented by one or more reservoir-intersecting wellbores.",
                "Dataset": {
                    "Provenance": {
                        "DatasetName": f"FORCE 2020 + SODIR keyed {config['fieldKey']} package",
                        "Classification": "Observed",
                        "SourceArtifacts": [metadata_artifact],
                    },
                    "ExternalIdentifiers": [
                        {"Namespace": "SODIR-WELL", "Value": facts["wlbWell"]},
                    ],
                    "Temporal": {"TransactionTimeStart": first_seen_at},
                    "LifecycleEvents": [],
                },
            },
            "WellBore": {
                "MetaInfo": {"ID": well_bore_id},
                "Name": uwi,
                "Description": (
                    f"SODIR total depth {total_depth:.0f} m MD; content={facts.get('wlbContent') or 'unknown'}"
                    + (
                        "; source names a side bore; parent tie-in is not reconstructed, so any FORCE trajectory calculation starts at wellhead"
                        if " " in uwi
                        else ""
                    )
                ),
                "WellID": well_id,
                "IsSidetrack": False,
            },
            **({
                "SurveyRun": {
                    "MetaInfo": {"ID": survey_id},
                    "Name": f"{uwi} FORCE coordinate trajectory",
                    "Description": "Trajectory attitude estimated from FORCE X_LOC, Y_LOC, and Z_LOC curves; inclination capped at 40 degrees for the available survey model.",
                    "SurveyStationList": trajectory_stations(log),
                },
                "Trajectory": {
                    "MetaInfo": {"ID": trajectory_id},
                    "Name": f"{uwi} definitive FORCE trajectory",
                    "Description": "Calculated from downsampled FORCE coordinate curves.",
                },
            } if log else {}),
            **({
                "WellBoreArchitecture": {
                    "MetaInfo": {"ID": architecture_id},
                    "Name": f"{uwi} SODIR casing architecture",
                    "Description": (
                        "Casing shoe depths and diameters from the FORCE/NPD casing workbook."
                        if source_casing
                        else "Model-estimated casing architecture; no FORCE/NPD casing rows matched this UWI."
                    ),
                    "CreationDate": first_seen_at,
                    "LastModificationDate": first_seen_at,
                    "WellBoreID": well_bore_id,
                    "WellHead": {},
                    "FluidsAboveGroundLevel": [],
                    "SurfaceSections": [{"SideConnectors": []}],
                    "CasingSections": architecture_sections,
                },
            } if log or source_casing else {}),
            "GeologicalProperties": {
                "MetaInfo": {"ID": geological_id},
                "Name": f"{uwi} FORCE-SODIR interpreted logs",
                "Description": "Observed FORCE curves; PHIE uses density-neutron RMS, SW uses Archie a=1 m=n=2 Rw=0.08, and permeability/phase/flow/pressure are uncalibrated screening proxies.",
                "WellBoreID": well_bore_id,
                "TrajectoryID": trajectory_id if log else None,
                "IsPrognosed": False,
                "GeologicalPropertyTable": log["GeologicalPropertyTable"] if log else None,
                "Petrophysics": {
                    "Provenance": {
                        "DatasetName": f"FORCE 2020 + SODIR keyed {config['fieldKey']} package",
                        "DatasetVersion": "5 m FORCE visualization resample" if log else "SODIR interval only",
                        "Classification": "Derived" if log else "HumanInterpreted",
                        "SourceArtifacts": (
                            [force_artifact, metadata_artifact, litho_artifact, casing_artifact]
                            if log
                            else [metadata_artifact, litho_artifact]
                        ),
                    },
                    "ExternalIdentifiers": [
                        {"Namespace": "UWI", "Value": uwi},
                        {"Namespace": "SODIR-NPDID-WELLBORE", "Value": str(facts["wlbNpdidWellbore"])},
                    ],
                    "Temporal": {"TransactionTimeStart": first_seen_at},
                    "LogRuns": ([
                        {
                            "ID": stable_id(dataset_key, uwi, "log-run", "visualization"),
                            "Name": f"{uwi} FORCE 5 m visualization log",
                            "Tool": "FORCE composite; derived curves are explicitly classified",
                            "SourceArtifactID": force_artifact["ID"],
                            "DepthAxis": {
                                "Reference": "MeasuredDepth",
                                "OriginalUnit": "m",
                                "CanonicalUnit": "m",
                                "Datum": "RKB",
                                "PositiveDown": True,
                            },
                            "DepthValues": log["DepthValues"],
                            "Curves": log["Curves"],
                        }
                    ] if log else []),
                    "FormationTops": tops,
                    "FormationIntervals": intervals,
                },
            },
        }


def force_records(args):
    dataset = load_manifest(args.manifest, "force-2020")
    zip_path, artifact_id, source_artifact, first_seen_at = artifact(
        dataset,
        args.raw_root,
        "LAS_files_Force_2020_all_wells_train_test_blind_hidden_final.zip",
    )
    with zipfile.ZipFile(zip_path) as archive:
        entries = sorted(name for name in archive.namelist() if name.lower().endswith(".las"))
        if args.limit is not None:
            entries = entries[: args.limit]
        for entry in entries:
            with archive.open(entry) as binary:
                header, definitions, rows = parse_las(binary)
            source_id = header.get("UWI", {}).get("value") or Path(entry).stem.replace("_", "/")
            null_value = float(header.get("NULL", {}).get("value") or -999.25)
            columns = list(zip(*rows))
            depth_index = next(
                index
                for index, definition in enumerate(definitions)
                if definition["mnemonic"].upper() in ("DEPT", "DEPTH")
            )
            depth_definition = definitions[depth_index]
            depth_values = list(columns[depth_index])
            curves = []
            for index, definition in enumerate(definitions):
                if index == depth_index:
                    continue
                values = [None if value == null_value else value for value in columns[index]]
                mnemonic = definition["mnemonic"]
                classification = (
                    "HumanInterpreted"
                    if "LITHO" in mnemonic.upper() or "FORMATION" in mnemonic.upper()
                    else "Observed"
                )
                curves.append(
                    {
                        "ID": stable_id("force-2020", source_id, "curve", mnemonic),
                        "OriginalMnemonic": mnemonic,
                        "CanonicalMnemonic": mnemonic.upper(),
                        "OriginalUnit": definition["unit"] or None,
                        "CanonicalUnit": definition["unit"] or None,
                        "Values": values,
                        "NullFlags": [value is None for value in values],
                        "OriginalNullValue": null_value,
                        "Classification": classification,
                    }
                )
            identifiers = [("UWI", source_id)]
            well_id = stable_id("force-2020", "well", source_id)
            well_bore_id = stable_id("force-2020", "well-bore", source_id)
            geological_id = stable_id("force-2020", "geological-properties", source_id)
            yield {
                "SourcePath": entry,
                "Well": {
                    "MetaInfo": {"ID": well_id},
                    "Name": header.get("WELL", {}).get("value") or source_id,
                    "Description": "Imported FORCE 2020 well log benchmark",
                    **well_envelope(dataset["title"], source_artifact, first_seen_at, identifiers),
                },
                "WellBore": {
                    "MetaInfo": {"ID": well_bore_id},
                    "Name": f"{source_id} main bore",
                    "Description": "Imported FORCE 2020 well bore",
                    "WellID": well_id,
                    "IsSidetrack": False,
                },
                "GeologicalProperties": {
                    "MetaInfo": {"ID": geological_id},
                    "Name": f"{source_id} FORCE 2020 logs",
                    "Description": "Observed log curves and supplied expert interpretations",
                    "WellBoreID": well_bore_id,
                    "IsPrognosed": False,
                    "Petrophysics": {
                        "Provenance": {
                            "DatasetName": dataset["title"],
                            "Classification": "Observed",
                            "SourceArtifacts": [source_artifact],
                        },
                        "ExternalIdentifiers": [{"Namespace": "UWI", "Value": source_id}],
                        "Temporal": {"TransactionTimeStart": first_seen_at},
                        "LogRuns": [
                            {
                                "ID": stable_id("force-2020", source_id, "log-run", entry),
                                "Name": Path(entry).name,
                                "SourceArtifactID": artifact_id,
                                "DepthAxis": {
                                    "Reference": "MeasuredDepth",
                                    "OriginalUnit": depth_definition["unit"] or None,
                                    "CanonicalUnit": depth_definition["unit"] or None,
                                    "PositiveDown": True,
                                },
                                "DepthValues": depth_values,
                                "Curves": curves,
                            }
                        ],
                    },
                },
            }


def self_check():
    assert stable_id("a", "b") == stable_id("a", "b")
    assert abs(US_SURVEY_FOOT_M - 0.3048006096012192) < 1e-15
    assert parse_kgs_date("01-MAR-1967").startswith("1967-03-01")
    sample = b"~Well\nNULL. -999.25 : null\nUWI. TEST : id\n~Curve\nDEPT.m : depth\nGR.API : gamma\n~A\n1 -999.25\n2 80\n"
    header, curves, rows = parse_las(io.BytesIO(sample))
    assert header["UWI"]["value"] == "TEST"
    assert curves[1]["mnemonic"] == "GR"
    assert rows[1][1] == 80
    sections = casing_sections([{"G": "100", "C": "13.375IN CASING"}], 250)
    assert len(sections) == 1
    assert sections[0]["OpenHoleSection"]["HoleSizes"][0]["Length"]["GaussianValue"]["Mean"] == 150
    fake_curves = [
        {"CanonicalMnemonic": name, "Values": values, "Classification": "Observed"}
        for name, values in (
            ("NPHI", [0.18, 0.2, 0.22]),
            ("RHOB", [2.35, 2.3, 2.25]),
            ("RDEP", [10, 20, 30]),
            ("FORCE_2020_LITHOFACIES_LITHOLOGY", [30000, 30000, 30000]),
            ("X_LOC", [100, 101, 103]),
            ("Y_LOC", [200, 202, 205]),
            ("Z_LOC", [-1, -6, -11]),
        )
    ]
    fake_record = {
        "Well": {"Dataset": {"ExternalIdentifiers": [{"Value": "TEST"}]}},
        "GeologicalProperties": {
            "Petrophysics": {
                "LogRuns": [{"DepthValues": [0, 5, 10], "Curves": fake_curves}]
            }
        },
    }
    sampled = sampled_force_log(fake_record, "OIL")
    assert len(sampled["GeologicalPropertyTable"]) == 3
    assert trajectory_stations(sampled)[0]["md"] == 0
    print("self-check passed")


def main():
    parser = argparse.ArgumentParser(description="Stage rights-cleared public data as DrillSim JSONL.")
    parser.add_argument(
        "dataset",
        choices=(
            "force-2020",
            "force-sodir-15-9",
            "force-sodir-troll",
            "usgs-williston",
            "kgs",
            "self-check",
        ),
    )
    parser.add_argument("--manifest", type=Path, default=Path("datasets/manifest.json"))
    parser.add_argument("--raw-root", type=Path, default=Path("data/raw"))
    parser.add_argument("--staged-root", type=Path, default=Path("data/staged"))
    parser.add_argument(
        "--force-records",
        type=Path,
        default=Path("data/staged/force-2020/records.jsonl"),
    )
    parser.add_argument("--limit", type=int, default=250)
    args = parser.parse_args()
    if args.dataset == "self-check":
        self_check()
    elif args.dataset == "force-2020":
        write_jsonl(args.staged_root / "force-2020" / "records.jsonl", force_records(args))
    elif args.dataset in ("force-sodir-15-9", "force-sodir-troll"):
        write_jsonl(
            args.staged_root / args.dataset / "records.jsonl",
            force_sodir_records(args),
        )
    elif args.dataset == "usgs-williston":
        write_jsonl(args.staged_root / "usgs-williston" / "records.jsonl", usgs_records(args))
    else:
        write_jsonl(args.staged_root / "kgs-ellis-11s-16w" / "records.jsonl", kgs_records(args))


if __name__ == "__main__":
    main()
