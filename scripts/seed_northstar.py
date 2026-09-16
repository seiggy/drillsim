#!/usr/bin/env python3
import argparse
import json
import math
import subprocess
import time
import urllib.error
import urllib.request
import uuid


SLUG = "northstar"
MARKER = f"SYNTH-{SLUG}"
CENTER_LAT = math.radians(29.075)
CENTER_LON = math.radians(-90.15)
EARTH_RADIUS_M = 6_378_137.0

MCP_PATHS = {
    "cartographic-projection": "/cartographicprojection/api/mcp",
    "cluster": "/cluster/api/mcp",
    "field": "/field/api/mcp",
    "rig": "/rig/api/mcp",
    "survey-instrument": "/surveyinstrument/api/mcp",
    "well": "/well/api/mcp",
}

REST_PATHS = {
    "drill-string": "/drillstring/api",
    "geological-properties": "/geologicalproperties/api",
    "geothermal-properties": "/geothermalproperties/api",
    "trajectory": "/trajectory/api",
    "well-bore": "/wellbore/api",
    "well-bore-architecture": "/wellborearchitecture/api",
}

WELLS = (
    ("Aster", -3500.0, -2200.0),
    ("Birch", -2100.0, 1800.0),
    ("Cedar", -200.0, -2800.0),
    ("Dogwood", 900.0, 2600.0),
    ("Elm", 3300.0, -900.0),
    ("Fir", 3900.0, 2300.0),
)


def scenario_id(service, entity, name):
    return str(uuid.uuid5(uuid.NAMESPACE_URL, f"drillsim:{SLUG}:{service}:{entity}:{name}"))


def global_position(east_m, north_m):
    latitude = CENTER_LAT + north_m / EARTH_RADIUS_M
    longitude = CENTER_LON + east_m / (EARTH_RADIUS_M * math.cos(CENTER_LAT))
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


def gaussian(mean, standard_deviation):
    return {
        "GaussianValue": {
            "MinValue": mean - 4 * standard_deviation,
            "MaxValue": mean + 4 * standard_deviation,
            "Mean": mean,
            "StandardDeviation": standard_deviation,
        }
    }


def discover_urls():
    process = subprocess.run(
        ["aspire", "describe", "--non-interactive", "--format", "Json"],
        check=True,
        capture_output=True,
        text=True,
    )
    description = json.loads(process.stdout[process.stdout.index("{") :])
    urls = {}
    for resource in description["resources"]:
        for endpoint in resource.get("urls", []):
            if endpoint["name"] == "http":
                urls[resource["displayName"]] = endpoint["url"]
    missing = set(MCP_PATHS) - set(urls)
    if missing:
        raise RuntimeError(f"Missing running Aspire resources: {', '.join(sorted(missing))}")
    return urls


class McpClient:
    def __init__(self, endpoint):
        self.endpoint = endpoint
        self.request_id = 0

    def call(self, tool, arguments=None):
        self.request_id += 1
        message = {
            "jsonrpc": "2.0",
            "id": self.request_id,
            "method": "tools/call",
            "params": {"name": tool, "arguments": arguments or {}},
        }
        request = urllib.request.Request(
            self.endpoint,
            json.dumps(message).encode(),
            {
                "Content-Type": "application/json",
                "Accept": "application/json, text/event-stream",
            },
        )
        with urllib.request.urlopen(request, timeout=60) as response:
            text = response.read().decode()
        payload = json.loads(next(line[6:] for line in text.splitlines() if line.startswith("data: ")))
        if "error" in payload:
            raise RuntimeError(f"{tool}: {payload['error']}")
        result = payload["result"]
        if result.get("isError"):
            detail = result.get("content", [{"text": "unknown MCP failure"}])[0]["text"]
            raise RuntimeError(f"{tool}: {detail}")
        return result.get("structuredContent", {})


class RestClient:
    def __init__(self, endpoint):
        self.endpoint = endpoint

    def request(self, method, path, body=None):
        data = None if body is None else json.dumps(body).encode()
        request = urllib.request.Request(
            self.endpoint + path,
            data,
            {"Content-Type": "application/json", "Accept": "application/json"},
            method=method,
        )
        try:
            with urllib.request.urlopen(request, timeout=60) as response:
                text = response.read().decode()
        except urllib.error.HTTPError as error:
            detail = error.read().decode()
            raise RuntimeError(f"{method} {path}: HTTP {error.code} {detail}") from error
        return json.loads(text) if text else None

    def get(self, path):
        return self.request("GET", path)

    def post(self, path, body):
        return self.request("POST", path, body)

    def put(self, path, body):
        return self.request("PUT", path, body)


def ensure_resource(client, list_tool, get_tool, create_tool, resource_id, body, update_tool=None):
    ids = client.call(list_tool).get("data", [])
    created = resource_id not in ids
    if created:
        client.call(create_tool, body)
    resource = client.call(get_tool, {"id": resource_id}).get("data")
    meta = (resource or {}).get("MetaInfo") or (resource or {}).get("metaInfo") or {}
    persisted_id = meta.get("ID") or meta.get("id") or ""
    if not resource or persisted_id.lower() != resource_id:
        raise RuntimeError(f"{create_tool}: resource {resource_id} was not persisted")
    text = " ".join(
        str(resource.get(key, ""))
        for key in ("Name", "name", "Description", "description")
    )
    if MARKER not in text:
        raise RuntimeError(f"{create_tool}: deterministic ID collides with non-synthetic data")
    if not created and update_tool:
        body_name, desired = next(iter(body.items()))
        if any(resource.get(key) != value for key, value in desired.items()):
            expected = resource.get("LastModificationDate") or resource.get("lastModificationDate")
            if not expected:
                raise RuntimeError(f"{update_tool}: resource {resource_id} has no modification token")
            resource = client.call(
                update_tool,
                {
                    "id": resource_id,
                    "expectedModifiedUtc": expected,
                    body_name: {**resource, **desired},
                },
            ).get("data")
    return resource


def ensure_rest(client, collection, resource_id, body, replace=False):
    ids = {str(value).lower() for value in client.get(collection)}
    if resource_id in ids and replace:
        client.put(f"{collection}/{resource_id}", body)
    elif resource_id not in ids:
        client.post(collection, body)
    resource = client.get(f"{collection}/{resource_id}")
    meta = resource.get("metaInfo") or resource.get("MetaInfo") or {}
    persisted_id = meta.get("id") or meta.get("ID") or ""
    if persisted_id.lower() != resource_id:
        raise RuntimeError(f"POST {collection}: resource {resource_id} was not persisted")
    text = " ".join(
        str(resource.get(key, ""))
        for key in ("Name", "name", "Description", "description")
    )
    if MARKER not in text:
        raise RuntimeError(f"POST {collection}: deterministic ID collides with non-synthetic data")
    return resource


def wait_for_calculation(client, collection, resource_id, timeout_seconds=30):
    deadline = time.monotonic() + timeout_seconds
    while time.monotonic() < deadline:
        resource = client.get(f"{collection}/{resource_id}")
        state = resource.get("CalculationState") or resource.get("calculationState")
        if state == "Completed":
            return resource
        if state == "Failed":
            message = resource.get("CalculationMessage") or resource.get("calculationMessage")
            raise RuntimeError(f"{collection}/{resource_id}: {message or 'calculation failed'}")
        time.sleep(0.25)
    raise RuntimeError(f"{collection}/{resource_id}: calculation timed out")


def geology_value(mean, standard_deviation):
    return {
        "gaussianValue": {
            "mean": mean,
            "standardDeviation": standard_deviation,
            "minValue": mean - 4 * standard_deviation,
            "maxValue": mean + 4 * standard_deviation,
        }
    }


def reservoir_profile(east_m, north_m):
    angle = math.radians(25)
    u = (east_m - 1800) * math.cos(angle) + (north_m - 1200) * math.sin(angle)
    v = -(east_m - 1800) * math.sin(angle) + (north_m - 1200) * math.cos(angle)
    quality = math.exp(-((u / 3500) ** 2) - ((v / 1400) ** 2))
    return {
        "quality": quality,
        "top": 2450 + 0.018 * east_m - 0.010 * north_m,
        "gross": 28 + 32 * quality,
        "porosity": 0.13 + 0.11 * quality,
        "permeability": 9.869233e-16 * 5 * 10 ** (2 * quality),
        "pressure": 4.0e6 + 3.0e6 * quality,
    }


def seed_downhole(urls, ledger):
    clients = {
        name: RestClient(urls[name] + path)
        for name, path in REST_PATHS.items()
    }
    for well in ledger["wells"]:
        name = well["name"]
        key = name.lower()
        profile = reservoir_profile(well["eastM"], well["northM"])
        well_bore_id = scenario_id("well-bore", "well-bore", key)
        ensure_rest(
            clients["well-bore"],
            "/WellBore",
            well_bore_id,
            {
                "metaInfo": {"id": well_bore_id},
                "name": f"{MARKER} {name} Well Bore",
                "description": f"{MARKER} fictional main well bore",
                "wellID": well["wellId"],
                "rigID": ledger["rigId"],
                "isSidetrack": False,
                "sidetrackType": 0,
            },
            replace=True,
        )

        target_east, target_north = 1800.0, 1200.0
        azimuth = math.atan2(target_east - well["eastM"], target_north - well["northM"]) % (2 * math.pi)
        trajectory_id = scenario_id("trajectory", "trajectory", key)
        stations = [
            {"md": 0.0, "abscissa": 0.0, "inclination": 0.0, "azimuth": azimuth},
            {"md": 1200.0, "abscissa": 1200.0, "inclination": 0.08, "azimuth": azimuth},
            {"md": 2600.0, "abscissa": 2600.0, "inclination": 0.12, "azimuth": azimuth},
            {"md": 3000.0, "abscissa": 3000.0, "inclination": 0.12, "azimuth": azimuth},
        ]
        survey_run_id = scenario_id("trajectory", "survey-run", key)
        ensure_rest(
            clients["trajectory"],
            "/SurveyRun",
            survey_run_id,
            {
                "metaInfo": {"id": survey_run_id},
                "name": f"{MARKER} {name} Survey Run",
                "description": f"{MARKER} fictional MWD survey run",
                "fieldID": ledger["fieldId"],
                "clusterID": well["clusterId"],
                "wellID": well["wellId"],
                "wellBoreID": well_bore_id,
                "surveyInstrumentID": ledger["surveyInstrumentId"],
                "surveyRunType": 0,
                "calculationType": 0,
                "surveyStationList": stations,
            },
            replace=True,
        )
        wait_for_calculation(clients["trajectory"], "/SurveyRun", survey_run_id)
        trajectory = ensure_rest(
            clients["trajectory"],
            "/Trajectory",
            trajectory_id,
            {
                "metaInfo": {"id": trajectory_id},
                "name": f"{MARKER} {name} Actual Trajectory",
                "description": f"{MARKER} fictional definitive trajectory",
                "fieldID": ledger["fieldId"],
                "clusterID": well["clusterId"],
                "wellID": well["wellId"],
                "wellBoreID": well_bore_id,
                "trajectoryType": 0,
                "isDefinitive": True,
                "mdStep": 10.0,
                "surveyRunSectionList": [
                    {"surveyRunID": survey_run_id, "startAbscissa": 0.0}
                ],
            },
            replace=True,
        )
        trajectory = wait_for_calculation(clients["trajectory"], "/Trajectory", trajectory_id)

        architecture_id = scenario_id("well-bore-architecture", "architecture", key)
        casing_length = 10 * math.floor(profile["top"] / 10)
        completion_length = 10 * math.ceil(profile["gross"] / 10)
        ensure_rest(
            clients["well-bore-architecture"],
            "/WellBoreArchitecture",
            architecture_id,
            {
                "metaInfo": {"id": architecture_id},
                "name": f"{MARKER} {name} Architecture",
                "description": f"{MARKER} fictional exploration well architecture",
                "creationDate": "2026-07-01T00:00:00Z",
                "lastModificationDate": "2026-07-01T00:00:00Z",
                "wellBoreID": well_bore_id,
                "wellHead": {},
                "fluidsAboveGroundLevel": [],
                "surfaceSections": [{"sideConnectors": []}],
                "casingSections": [
                    {
                        "topDepth": gaussian(0.0, 0.5),
                        "length": gaussian(casing_length, 1.0),
                        "topCementDepth": gaussian(0.0, 2.0),
                        "casingSectionElements": [
                            {
                                "bodyID": gaussian(0.178, 0.001),
                                "bodyOD": gaussian(0.194, 0.001),
                                "collarOD": gaussian(0.205, 0.001),
                                "jointLength": gaussian(12.0, 0.05),
                                "sectionLength": gaussian(casing_length, 1.0),
                                "connectionType": "Synthetic premium connection",
                                "grade": "P110",
                            }
                        ],
                        "casingSectionSizeTable": [
                            {
                                "holeSize": gaussian(0.216, 0.001),
                                "length": gaussian(casing_length, 1.0),
                            }
                        ],
                        "openHoleSection": {
                            "holeSizes": [
                                {
                                    "holeSize": gaussian(0.156, 0.001),
                                    "length": gaussian(completion_length, 1.0),
                                }
                            ]
                        },
                    }
                ],
            },
            replace=True,
        )

        drill_string_id = scenario_id("drill-string", "drill-string", key)
        ensure_rest(
            clients["drill-string"],
            "/DrillString",
            drill_string_id,
            {
                "metaInfo": {"id": drill_string_id},
                "name": f"{MARKER} {name} Drill String",
                "description": f"{MARKER} fictional exploration drill string",
                "wellBoreID": well_bore_id,
                "drillStringSectionList": [],
                "sensorsList": [],
            },
        )

        geothermal_id = scenario_id("geothermal-properties", "geothermal", key)
        ensure_rest(
            clients["geothermal-properties"],
            "/GeothermalProperties",
            geothermal_id,
            {
                "metaInfo": {"id": geothermal_id},
                "name": f"{MARKER} {name} Geothermal Profile",
                "description": f"{MARKER} fictional 0.030 K/m temperature gradient",
                "wellBoreID": well_bore_id,
                "trajectoryID": trajectory_id,
                "tableType": 0,
                "geothermalDataList": [
                    {
                        "regionType": 2,
                        "verticalDepth": depth,
                        "temperature": 288.15 + 0.030 * depth,
                        "temperatureGradient": 0.030,
                    }
                    for depth in range(0, 3001, 500)
                ],
            },
        )

        quality = profile["quality"]
        top = profile["top"]
        gross = profile["gross"]
        porosity = profile["porosity"]
        permeability = profile["permeability"]
        pressure = profile["pressure"]
        table = []
        depths = []
        curves = {
            "PHIE": [],
            "PERM": [],
            "SW": [],
            "SO": [],
            "SG": [],
            "QO": [],
            "QG": [],
            "QW": [],
        }
        start = 10 * math.floor((top - 20) / 10)
        end = 10 * math.ceil((top + gross + 20) / 10)
        for depth in range(start, end + 1, 10):
            in_reservoir = top <= depth <= top + gross
            water_saturation = 0.95 if not in_reservoir else 0.28 - 0.12 * quality
            gas_saturation = 0.01 if not in_reservoir else 0.08 + 0.10 * quality
            oil_saturation = 1.0 - water_saturation - gas_saturation
            oil_flow = (4.0 + 18.0 * quality) if in_reservoir else 0.0
            gas_flow = oil_flow * (18.0 + 12.0 * quality)
            water_flow = (1.0 + 5.0 * (1.0 - quality)) if in_reservoir else 0.0
            depths.append(depth)
            curves["PHIE"].append(porosity if in_reservoir else 0.07)
            curves["PERM"].append(permeability if in_reservoir else 1e-18)
            curves["SW"].append(water_saturation)
            curves["SO"].append(oil_saturation)
            curves["SG"].append(gas_saturation)
            curves["QO"].append(oil_flow)
            curves["QG"].append(gas_flow)
            curves["QW"].append(water_flow)
            table.append(
                {
                    "measuredDepth": geology_value(depth, 0.5),
                    "porosity": geology_value(porosity if in_reservoir else 0.07, 0.008 + 0.012 * (1 - quality)),
                    "permeability": geology_value(permeability if in_reservoir else 1e-18, max(permeability * 0.15, 1e-19)),
                    "pressureDifferential": geology_value(pressure if in_reservoir else 1.0e6, 2.0e5),
                    "internalFrictionAngle": geology_value(0.52 if in_reservoir else 0.61, 0.02),
                    "unconfinedCompressiveStrength": geology_value(35e6 if in_reservoir else 70e6, 3e6),
                    "confinedCompressiveStrength": geology_value(65e6 if in_reservoir else 110e6, 5e6),
                    "dataType": 0,
                }
            )
        geological_id = scenario_id("geological-properties", "geology", key)
        ensure_rest(
            clients["geological-properties"],
            "/GeologicalProperties",
            geological_id,
            {
                "metaInfo": {"id": geological_id},
                "name": f"{MARKER} {name} Geological Properties",
                "description": f"{MARKER} fictional observed reservoir profile; quality={quality:.4f}",
                "wellBoreID": well_bore_id,
                "trajectoryID": trajectory_id,
                "isPrognosed": False,
                "geologicalPropertyTable": table,
                "petrophysics": {
                    "provenance": {
                        "datasetName": f"{MARKER} synthetic petrophysics",
                        "datasetVersion": "1",
                        "classification": 4,
                    },
                    "logRuns": [
                        {
                            "id": scenario_id("geological-properties", "log-run", key),
                            "name": f"{MARKER} {name} interpreted reservoir log",
                            "tool": "Synthetic multi-physics composite",
                            "depthAxis": {
                                "reference": 0,
                                "originalUnit": "m",
                                "canonicalUnit": "m",
                                "datum": "well head",
                                "positiveDown": True,
                            },
                            "depthValues": depths,
                            "curves": [
                                {
                                    "id": scenario_id("geological-properties", "curve", f"{key}-{mnemonic.lower()}"),
                                    "originalMnemonic": mnemonic,
                                    "canonicalMnemonic": mnemonic,
                                    "originalUnit": unit,
                                    "canonicalUnit": unit,
                                    "values": curves[mnemonic],
                                    "classification": 4,
                                }
                                for mnemonic, unit in (
                                    ("PHIE", "fraction"),
                                    ("PERM", "m2"),
                                    ("SW", "fraction"),
                                    ("SO", "fraction"),
                                    ("SG", "fraction"),
                                    ("QO", "m3/d"),
                                    ("QG", "m3/d"),
                                    ("QW", "m3/d"),
                                )
                            ],
                        }
                    ],
                    "formationTops": [
                        {
                            "id": scenario_id("geological-properties", "formation-top", f"{key}-northstar"),
                            "formationName": "Northstar Sand",
                            "depths": [
                                {
                                    "reference": 0,
                                    "value": top,
                                    "unit": "m",
                                    "datum": "well head",
                                    "positiveDown": True,
                                }
                            ],
                            "confidence": 0.82 + 0.12 * quality,
                            "method": "Synthetic log correlation",
                            "classification": 4,
                        }
                    ],
                },
            },
            replace=True,
        )
        well.update(
            {
                "wellBoreId": well_bore_id,
                "surveyRunId": survey_run_id,
                "trajectoryId": trajectory_id,
                "architectureId": architecture_id,
                "drillStringId": drill_string_id,
                "geothermalId": geothermal_id,
                "geologicalPropertiesId": geological_id,
                "reservoirQuality": quality,
                "trajectoryState": trajectory.get("CalculationState") or trajectory.get("calculationState"),
            }
        )


def seed(surface_only=False):
    urls = discover_urls()
    clients = {
        name: McpClient(urls[name] + path)
        for name, path in MCP_PATHS.items()
    }

    projection_id = scenario_id("cartographic-projection", "projection", "utm15n")
    ensure_resource(
        clients["cartographic-projection"],
        "cartographic_projection_get_all_ids",
        "cartographic_projection_get_by_id",
        "cartographic_projection_create",
        projection_id,
        {
            "cartographicProjection": {
                "MetaInfo": {"ID": projection_id},
                "Name": f"{MARKER} UTM 15N",
                "Description": f"{MARKER} fictional WGS84 UTM projection",
                "ProjectionType": "UTM",
                "GeodeticDatumID": "a3f1d727-4e9b-4f19-8b06-1234567890ab",
                "Zone": 15,
                "IsSouth": False,
            }
        },
    )

    field_client = clients["field"]
    catalog_specs = (
        (
            "field_delineation_line_type",
            "fieldDelineationLineType",
            "LeaseBoundary",
            None,
        ),
        (
            "field_feature_category",
            "fieldFeatureCategory",
            "PlayType",
            "StructuralStratigraphic",
        ),
        (
            "field_identity",
            "fieldIdentity",
            "SyntheticFieldCode",
            None,
        ),
        (
            "field_membership_category",
            "fieldMembershipCategory",
            "Portfolio",
            "Exploration",
        ),
    )
    catalogs = {}
    for prefix, body_name, name, option_name in catalog_specs:
        resource_id = scenario_id("field", prefix, name)
        record = {"MetaInfo": {"ID": resource_id}, "Name": f"{MARKER} {name}"}
        if option_name:
            option_id = scenario_id("field", f"{prefix}-option", option_name)
            record.update(
                {
                    "IsExclusive": True,
                    "HasValidityPeriod": False,
                    "Options": [{"ID": option_id, "Name": option_name}],
                }
            )
            catalogs[f"{prefix}_option"] = option_id
        ensure_resource(
            field_client,
            f"{prefix}_get_all_ids",
            f"{prefix}_get_by_id",
            f"{prefix}_create",
            resource_id,
            {body_name: record},
        )
        catalogs[prefix] = resource_id

    field_id = scenario_id("field", "field", SLUG)
    boundary_offsets = ((-6000, -5000), (6000, -5000), (6000, 5000), (-6000, 5000), (-6000, -5000))
    field = {
        "MetaInfo": {"ID": field_id},
        "Name": f"{MARKER} Field",
        "Description": f"{MARKER} fictional exploration field",
        "ProjectionDefinitionID": projection_id,
        "ReferencePoint": global_position(0, 0),
        "FieldFeatureAssignments": [
            {
                "ID": scenario_id("field", "feature-assignment", "play-type"),
                "FeatureCategoryID": catalogs["field_feature_category"],
                "FeatureOptionID": catalogs["field_feature_category_option"],
            }
        ],
        "FieldIdentityAssignments": [
            {
                "ID": scenario_id("field", "identity-assignment", "field-code"),
                "IdentityID": catalogs["field_identity"],
                "Value": "NORTHSTAR-SYNTH-001",
            }
        ],
        "FieldMembershipAssignments": [
            {
                "ID": scenario_id("field", "membership-assignment", "portfolio"),
                "MembershipCategoryID": catalogs["field_membership_category"],
                "MembershipOptionID": catalogs["field_membership_category_option"],
            }
        ],
        "DelineationLines": [
            {
                "ID": scenario_id("field", "delineation", "lease-boundary"),
                "DelineationLineTypeID": catalogs["field_delineation_line_type"],
                "Name": f"{MARKER} Lease Boundary",
                "Description": f"{MARKER} 12 km by 10 km fictional lease",
                "Margin": 250.0,
                "TopDepth": 0.0,
                "BottomDepth": 4000.0,
                "Points": [global_position(east, north) for east, north in boundary_offsets],
            }
        ],
    }
    ensure_resource(
        field_client,
        "field_get_all_ids",
        "field_get_by_id",
        "field_create",
        field_id,
        {"field": field},
        "field_update_by_id",
    )

    rig_client = clients["rig"]
    rig_id = scenario_id("rig", "rig", "nearshore-jackup-rig")
    ensure_resource(
        rig_client,
        "rig_get_all_ids",
        "rig_get_by_id",
        "rig_create",
        rig_id,
        {
            "rig": {
                "MetaInfo": {"ID": rig_id},
                "Name": f"{MARKER} Nearshore Jackup Rig",
                "Description": f"{MARKER} fictional 3,500 m Gulf exploration rig",
                "DrillFloorElevation": 8.0,
                "IsFixedPlatform": False,
                "ClusterID": None,
            }
        },
    )

    survey_client = clients["survey-instrument"]
    instrument_id = "2af52fd1-84a9-4fe0-9ea3-3d4c6256b2b5"
    survey_client.call("survey_instrument_get_by_id", {"id": instrument_id})

    cluster_client = clients["cluster"]
    well_client = clients["well"]
    well_rest = RestClient(urls["well"] + "/well/api")
    ledger = {
        "slug": SLUG,
        "fieldId": field_id,
        "projectionId": projection_id,
        "rigId": rig_id,
        "surveyInstrumentId": instrument_id,
        "wells": [],
    }
    for name, east, north in WELLS:
        profile = reservoir_profile(east, north)
        cluster_id = scenario_id("cluster", "cluster", name.lower())
        slot_id = scenario_id("cluster", "slot", name.lower())
        well_id = scenario_id("well", "well", name.lower())
        position = global_position(east, north)
        cluster = {
            "MetaInfo": {"ID": cluster_id},
            "Name": f"{MARKER} {name} Cluster",
            "Description": f"{MARKER} fictional single-well exploration location",
            "FieldID": field_id,
            "IsSingleWell": True,
            "RigID": rig_id,
            "IsFixedPlatform": False,
            "ReferencePoint": position,
            "GroundMudLineDepth": gaussian(0.0, 0.05),
            "TopWaterDepth": gaussian(-1.0, 0.05),
            "Slots": {
                slot_id: {
                    "ID": slot_id,
                    "Name": f"{MARKER} {name} Slot",
                    "Description": f"{MARKER} fictional well slot",
                    "Latitude": gaussian(position["Latitude"], 1.6e-9),
                    "Longitude": gaussian(position["Longitude"], 1.6e-9),
                }
            },
        }
        ensure_resource(
            cluster_client,
            "cluster_get_all_ids",
            "cluster_get_by_id",
            "cluster_create",
            cluster_id,
            {"cluster": cluster},
            "cluster_update_by_id",
        )
        dataset = {
            "Provenance": {
                "DatasetName": f"{MARKER} production history",
                "DatasetVersion": "1",
                "Classification": 3,
            },
            "LifecycleEvents": [
                {"EventType": "spud", "EffectiveAt": "2024-01-15T00:00:00Z"},
                {"EventType": "completion", "EffectiveAt": "2024-03-20T00:00:00Z"},
            ],
            "MonthlyProduction": [
                {
                    "Year": 2025 + (month_index + 6) // 12,
                    "Month": (month_index + 6) % 12 + 1,
                    "Oil": {
                        "Value": (4200 + 11800 * profile["quality"]) * (0.975 ** month_index),
                        "Unit": "bbl",
                    },
                    "Gas": {
                        "Value": (76000 + 230000 * profile["quality"]) * (0.978 ** month_index),
                        "Unit": "scf",
                    },
                    "Water": {
                        "Value": 900 + 3400 * (1 - profile["quality"]) + 65 * month_index,
                        "Unit": "bbl",
                    },
                    "DaysOnProduction": 30,
                    "IsAllocated": False,
                    "Classification": 3,
                }
                for month_index in range(12)
            ],
        }
        persisted_well = ensure_resource(
            well_client,
            "well_get_all_ids",
            "well_get_by_id",
            "well_create",
            well_id,
            {
                "well": {
                    "MetaInfo": {"ID": well_id},
                    "Name": f"{MARKER} {name} Well",
                    "Description": f"{MARKER} fictional exploration well",
                    "SlotID": slot_id,
                    "ClusterID": cluster_id,
                    "IsSingleWell": True,
                    "Dataset": dataset,
                }
            },
        )
        if persisted_well.get("Dataset") != dataset:
            well_rest.put(f"/Well/{well_id}", {**persisted_well, "Dataset": dataset})
        ledger["wells"].append(
            {
                "name": name,
                "eastM": east,
                "northM": north,
                "clusterId": cluster_id,
                "slotId": slot_id,
                "wellId": well_id,
                "latitudeRad": position["Latitude"],
                "longitudeRad": position["Longitude"],
            }
        )

    found_clusters = cluster_client.call("cluster_get_all_by_field_id", {"fieldId": field_id}).get("data", [])
    if len(found_clusters) != len(WELLS):
        raise RuntimeError(f"Expected {len(WELLS)} clusters for field, found {len(found_clusters)}")
    if not surface_only:
        seed_downhole(urls, ledger)
    print(json.dumps(ledger, indent=2))


def self_check():
    ids = {
        scenario_id("well", "well", name.lower())
        for name, _, _ in WELLS
    }
    assert len(ids) == len(WELLS)
    assert global_position(0, 0)["Latitude"] == CENTER_LAT
    assert gaussian(10, 2)["GaussianValue"]["MinValue"] == 2
    assert 0 < reservoir_profile(1800, 1200)["quality"] <= 1
    print("self-check passed")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Seed the deterministic DrillSim Northstar surface graph.")
    parser.add_argument("--self-check", action="store_true")
    parser.add_argument("--surface-only", action="store_true")
    args = parser.parse_args()
    self_check() if args.self_check else seed(args.surface_only)
