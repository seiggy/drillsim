#!/usr/bin/env python3
import argparse
import json
import math
from pathlib import Path
import tempfile
import uuid

from seed_northstar import (
    McpClient,
    RestClient,
    discover_urls,
    gaussian,
    wait_for_calculation,
)


NAD27_CONUS_ID = "6aa37377-7f73-41e4-a4d5-d37e1d7b6907"
WGS84_ID = "a3f1d727-4e9b-4f19-8b06-1234567890ab"
EARTH_RADIUS_M = 6_378_137.0
SURVEY_INSTRUMENT_ID = "2af52fd1-84a9-4fe0-9ea3-3d4c6256b2b5"


def stable_id(*parts):
    return str(uuid.uuid5(uuid.NAMESPACE_URL, "drillsim:import:" + ":".join(str(part) for part in parts)))


def load_records(path, limit):
    with path.open(encoding="utf-8") as source:
        for index, line in enumerate(source):
            if limit is not None and index >= limit:
                break
            yield json.loads(line)


def ensure_mcp(client, list_tool, get_tool, create_tool, resource_id, body):
    ids = {str(value).lower() for value in client.call(list_tool).get("data", [])}
    if resource_id not in ids:
        client.call(create_tool, body)
    resource = client.call(get_tool, {"id": resource_id}).get("data")
    if not resource:
        raise RuntimeError(f"{create_tool}: {resource_id} was not persisted")
    return resource


def ensure_rest(client, collection, resource_id, body, replace=False):
    ids = {str(value).lower() for value in client.get(collection)}
    if resource_id in ids and replace:
        client.put(f"{collection}/{resource_id}", body)
    elif resource_id not in ids:
        client.post(collection, body)
    resource = client.get(f"{collection}/{resource_id}")
    if not resource:
        raise RuntimeError(f"POST {collection}: {resource_id} was not persisted")
    return resource


def convert_location(client, source_location):
    crs = source_location.get("Crs", "")
    if "WGS84" in crs.upper() or "WGS 84" in crs.upper():
        latitude = math.radians(source_location["Latitude"])
        longitude = math.radians(source_location["Longitude"])
        return {
            "X": EARTH_RADIUS_M * latitude,
            "Y": EARTH_RADIUS_M * math.cos(latitude) * longitude,
            "Z": 0.0,
            "RiemannianNorth": EARTH_RADIUS_M * latitude,
            "RiemannianEast": EARTH_RADIUS_M * math.cos(latitude) * longitude,
            "Latitude": latitude,
            "Longitude": longitude,
            "TVD": 0.0,
        }
    if "NAD27" not in crs.upper():
        raise ValueError(f"Unsupported source CRS: {crs}")
    result = client.call(
        "geodetic_datum_convert_coordinate",
        {
            "sourceDatumId": NAD27_CONUS_ID,
            "targetDatumId": WGS84_ID,
            "latitude": math.radians(source_location["Latitude"]),
            "longitude": math.radians(source_location["Longitude"]),
            "verticalDepth": 0.0,
        },
    )
    data = result["data"]
    latitude = data["latitude"]
    longitude = data["longitude"]
    return {
        "X": EARTH_RADIUS_M * latitude,
        "Y": EARTH_RADIUS_M * math.cos(latitude) * longitude,
        "Z": data["verticalDepth"],
        "RiemannianNorth": EARTH_RADIUS_M * latitude,
        "RiemannianEast": EARTH_RADIUS_M * math.cos(latitude) * longitude,
        "Latitude": data["latitude"],
        "Longitude": data["longitude"],
        "TVD": data["verticalDepth"],
    }


def field_name(dataset_key, record):
    if dataset_key.startswith("force-sodir-"):
        return record["FieldName"]
    if dataset_key.startswith("kgs"):
        return record.get("FieldName") or "KGS Ellis 11S 16W Unnamed"
    return "USGS Williston Basin Pilot"


def load(args):
    urls = discover_urls()
    geodetic = McpClient(urls["geodetic-datum"] + "/geodeticdatum/api/mcp")
    field = McpClient(urls["field"] + "/field/api/mcp")
    cluster = McpClient(urls["cluster"] + "/cluster/api/mcp")
    well = McpClient(urls["well"] + "/well/api/mcp")
    well_bore = RestClient(urls["well-bore"] + "/wellbore/api")
    trajectory = RestClient(urls["trajectory"] + "/trajectory/api")
    architecture = RestClient(urls["well-bore-architecture"] + "/wellborearchitecture/api")
    geology = RestClient(urls["geological-properties"] + "/geologicalproperties/api")
    dataset_key = args.path.parent.name
    counts = {
        "fields": set(),
        "clusters": set(),
        "wells": set(),
        "wellBores": set(),
        "trajectories": set(),
        "architectures": set(),
        "geology": set(),
    }

    for record in load_records(args.path, args.limit):
        well_body = record["Well"]
        well_id = well_body["MetaInfo"]["ID"].lower()
        well_bore_body = record["WellBore"]
        well_bore_id = well_bore_body["MetaInfo"]["ID"].lower()
        geological_body = record["GeologicalProperties"]
        geological_id = geological_body["MetaInfo"]["ID"].lower()

        if "SourceLocation" in record:
            dataset_key = record.get("DatasetKey", dataset_key)
            location = convert_location(geodetic, record["SourceLocation"])
            name = field_name(dataset_key, record)
            field_id = record.get("FieldID", stable_id(dataset_key, "field", name)).lower()
            cluster_id = stable_id(dataset_key, "cluster", well_id).lower()
            slot_id = stable_id(dataset_key, "slot", well_id).lower()
            counts["fields"].add(field_id)
            well_body.update({"ClusterID": cluster_id, "SlotID": slot_id, "IsSingleWell": True})
            if not args.dry_run:
                center = convert_location(
                    geodetic,
                    record.get("FieldCenter", record["SourceLocation"]),
                )
                boundary = [
                    convert_location(geodetic, point)
                    for point in record.get("FieldBoundary", [])
                ]
                field_body = {
                    "MetaInfo": {"ID": field_id},
                    "Name": name,
                    "Description": record.get(
                        "FieldDescription",
                        f"Imported {dataset_key} field grouping",
                    ),
                    "ReferencePoint": center,
                }
                if boundary:
                    field_body["DelineationLines"] = [
                        {
                            "ID": stable_id(dataset_key, "delineation", field_id),
                            "Name": f"{name} study boundary",
                            "Description": "Bounding envelope of the selected public well controls.",
                            "Margin": 0.0,
                            "TopDepth": 0.0,
                            "BottomDepth": 5000.0,
                            "Points": boundary,
                        }
                    ]
                stored_field = ensure_mcp(
                    field,
                    "field_get_all_ids",
                    "field_get_by_id",
                    "field_create",
                    field_id,
                    {
                        "field": field_body
                    },
                )
                if any(stored_field.get(key) != value for key, value in field_body.items()):
                    field.call(
                        "field_update_by_id",
                        {
                            "id": field_id,
                            "expectedModifiedUtc": stored_field["LastModificationDate"],
                            "field": {**stored_field, **field_body},
                        },
                    )
                ensure_mcp(
                    cluster,
                    "cluster_get_all_ids",
                    "cluster_get_by_id",
                    "cluster_create",
                    cluster_id,
                    {
                        "cluster": {
                            "MetaInfo": {"ID": cluster_id},
                            "Name": f"{well_body['Name']} location",
                            "Description": f"Imported {dataset_key} single-well location; source surface elevation unavailable",
                            "FieldID": field_id,
                            "IsSingleWell": True,
                            "IsFixedPlatform": False,
                            "ReferencePoint": location,
                            "GroundMudLineDepth": gaussian(record.get("WaterDepth") or location["TVD"], 2.0),
                            "Slots": {
                                slot_id: {
                                    "ID": slot_id,
                                    "Name": f"{well_body['Name']} slot",
                                    "Description": f"Imported {dataset_key} source location",
                                    "Latitude": gaussian(location["Latitude"], 1.6e-6),
                                    "Longitude": gaussian(location["Longitude"], 1.6e-6),
                                }
                            },
                        }
                    },
                )
            counts["clusters"].add(cluster_id)

        if not args.dry_run:
            stored_well = ensure_mcp(
                well,
                "well_get_all_ids",
                "well_get_by_id",
                "well_create",
                well_id,
                {"well": well_body},
            )
            if any(stored_well.get(key) != value for key, value in well_body.items()):
                well.call(
                    "well_update_by_id",
                    {"id": well_id, "well": {**stored_well, **well_body}},
                )
            ensure_rest(well_bore, "/WellBore", well_bore_id, well_bore_body, replace=True)
            if "SurveyRun" in record:
                survey = {
                    **record["SurveyRun"],
                    "fieldID": field_id,
                    "clusterID": cluster_id,
                    "wellID": well_id,
                    "wellBoreID": well_bore_id,
                    "surveyInstrumentID": SURVEY_INSTRUMENT_ID,
                    "surveyRunType": 0,
                    "calculationType": 0,
                }
                survey_id = survey["MetaInfo"]["ID"].lower()
                ensure_rest(trajectory, "/SurveyRun", survey_id, survey, replace=True)
                wait_for_calculation(trajectory, "/SurveyRun", survey_id, timeout_seconds=60)
                trajectory_body = {
                    **record["Trajectory"],
                    "fieldID": field_id,
                    "clusterID": cluster_id,
                    "wellID": well_id,
                    "wellBoreID": well_bore_id,
                    "trajectoryType": 0,
                    "isDefinitive": True,
                    "mdStep": 10.0,
                    "surveyRunSectionList": [
                        {"surveyRunID": survey_id, "startAbscissa": 0.0}
                    ],
                }
                trajectory_id = trajectory_body["MetaInfo"]["ID"].lower()
                ensure_rest(trajectory, "/Trajectory", trajectory_id, trajectory_body, replace=True)
                wait_for_calculation(trajectory, "/Trajectory", trajectory_id, timeout_seconds=60)
            if "WellBoreArchitecture" in record:
                architecture_body = record["WellBoreArchitecture"]
                architecture_id = architecture_body["MetaInfo"]["ID"].lower()
                ensure_rest(
                    architecture,
                    "/WellBoreArchitecture",
                    architecture_id,
                    architecture_body,
                    replace=True,
                )
            ensure_rest(
                geology,
                "/GeologicalProperties",
                geological_id,
                geological_body,
                replace=True,
            )
        counts["wells"].add(well_id)
        counts["wellBores"].add(well_bore_id)
        counts["geology"].add(geological_id)
        if "Trajectory" in record:
            counts["trajectories"].add(record["Trajectory"]["MetaInfo"]["ID"].lower())
        if "WellBoreArchitecture" in record:
            counts["architectures"].add(record["WellBoreArchitecture"]["MetaInfo"]["ID"].lower())

    counts = {name: len(ids) for name, ids in counts.items()}
    print(json.dumps({"dataset": dataset_key, "dryRun": args.dry_run, **counts}, indent=2))


def self_check():
    first = stable_id("dataset", "field", "example")
    assert first == stable_id("dataset", "field", "example")
    assert first != stable_id("dataset", "field", "other")
    wgs84 = convert_location(None, {"Latitude": 58.3, "Longitude": 1.9, "Crs": "WGS84"})
    assert abs(wgs84["Latitude"] - math.radians(58.3)) < 1e-12
    with tempfile.TemporaryDirectory() as directory:
        path = Path(directory) / "records.jsonl"
        path.write_text('{"Well":{"MetaInfo":{"ID":"example"}}}\n', encoding="utf-8")
        records = list(load_records(path, 1))
        assert len(records) == 1 and records[0]["Well"]["MetaInfo"]["ID"] == "example"
    print("self-check passed")


def main():
    parser = argparse.ArgumentParser(description="Load staged public records into running DrillSim services.")
    parser.add_argument("path", type=Path, nargs="?")
    parser.add_argument("--limit", type=int, default=10)
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument("--self-check", action="store_true")
    args = parser.parse_args()
    if args.self_check:
        self_check()
    elif args.path is None:
        parser.error("path is required unless --self-check is used")
    else:
        load(args)


if __name__ == "__main__":
    main()
