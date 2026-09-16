#!/usr/bin/env python3
import argparse
from datetime import datetime, timezone
from decimal import Decimal, InvalidOperation, ROUND_FLOOR
import hashlib
import json
import math
import os
from pathlib import Path
import tempfile


SPLITS = ("train", "validation", "test")
DEFAULT_RATIOS = (0.70, 0.15, 0.15)
CHUNK_SIZE = 1024 * 1024


def strict_object(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise ValueError(f"duplicate JSON key: {key}")
        result[key] = value
    return result


def strict_json(text):
    def reject_constant(value):
        raise ValueError(f"invalid JSON constant: {value}")

    return json.loads(text, object_pairs_hook=strict_object, parse_constant=reject_constant)


def file_sha256(path):
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        while chunk := stream.read(CHUNK_SIZE):
            digest.update(chunk)
    return digest.hexdigest()


def source_paths(inputs):
    paths = []
    for item in inputs:
        path = item.resolve()
        if path.is_dir():
            paths.extend(candidate.resolve() for candidate in path.rglob("*.jsonl"))
        elif path.is_file() and path.suffix.lower() == ".jsonl":
            paths.append(path)
        else:
            raise ValueError(f"{item}: expected a JSONL file or dataset directory")
    unique = sorted(set(paths), key=lambda path: str(path).casefold())
    if not unique:
        raise ValueError("no JSONL files found")
    return unique


def validate_ratios(values):
    if len(values) != len(SPLITS):
        raise ValueError("ratios must provide train, validation, and test values")
    ratios = tuple(float(value) for value in values)
    if any(not math.isfinite(value) or value <= 0 for value in ratios):
        raise ValueError("ratios must be finite positive numbers")
    if not math.isclose(sum(ratios), 1.0, rel_tol=0.0, abs_tol=1e-12):
        raise ValueError("ratios must sum to 1")
    return ratios


def validate_cell_size(value, name):
    try:
        size = Decimal(str(value))
    except InvalidOperation as error:
        raise ValueError(f"{name} must be a finite positive number") from error
    if not size.is_finite() or size <= 0:
        raise ValueError(f"{name} must be a finite positive number")
    return size


def spatial_block(location, latitude_size, longitude_size, context):
    if not isinstance(location, dict):
        raise ValueError(f"{context}: SourceLocation must be an object")
    latitude = location.get("Latitude")
    longitude = location.get("Longitude")
    if (
        isinstance(latitude, bool)
        or isinstance(longitude, bool)
        or not isinstance(latitude, (int, float))
        or not isinstance(longitude, (int, float))
        or not math.isfinite(latitude)
        or not math.isfinite(longitude)
        or not -90 <= latitude <= 90
        or not -180 <= longitude <= 180
    ):
        raise ValueError(f"{context}: invalid SourceLocation latitude or longitude")
    lat_cell = int((Decimal(str(latitude)) / latitude_size).to_integral_value(rounding=ROUND_FLOOR))
    lon_cell = int((Decimal(str(longitude)) / longitude_size).to_integral_value(rounding=ROUND_FLOOR))
    return f"{lat_cell}:{lon_cell}"


class Groups:
    def __init__(self):
        self.parent = {}

    def add(self, node):
        self.parent.setdefault(node, node)

    def find(self, node):
        parent = self.parent[node]
        if parent != node:
            self.parent[node] = self.find(parent)
        return self.parent[node]

    def union(self, left, right):
        self.add(left)
        self.add(right)
        left_root, right_root = self.find(left), self.find(right)
        if left_root != right_root:
            first, second = sorted((left_root, right_root))
            self.parent[second] = first


def read_groups(paths, latitude_size, longitude_size):
    groups = Groups()
    wells = set()
    for path in paths:
        with path.open("r", encoding="utf-8") as stream:
            for line_number, line in enumerate(stream, 1):
                context = f"{path}:{line_number}"
                if not line.strip():
                    raise ValueError(f"{context}: blank lines are not valid JSONL records")
                try:
                    record = strict_json(line)
                except (json.JSONDecodeError, ValueError) as error:
                    raise ValueError(f"{context}: invalid JSON: {error}") from error
                if not isinstance(record, dict):
                    raise ValueError(f"{context}: record must be a JSON object")
                try:
                    well_id = record["Well"]["MetaInfo"]["ID"]
                except (KeyError, TypeError) as error:
                    raise ValueError(f"{context}: missing Well.MetaInfo.ID") from error
                if not isinstance(well_id, str) or not well_id.strip():
                    raise ValueError(f"{context}: Well.MetaInfo.ID must be a non-empty string")
                well_node = f"well:{well_id}"
                groups.add(well_node)
                wells.add(well_id)
                if "SourceLocation" in record:
                    block = spatial_block(
                        record["SourceLocation"], latitude_size, longitude_size, context
                    )
                    groups.union(well_node, f"block:{block}")
    by_root = {}
    for node in groups.parent:
        by_root.setdefault(groups.find(node), []).append(node)
    well_groups = {}
    for nodes in by_root.values():
        block_nodes = sorted(node for node in nodes if node.startswith("block:"))
        group = "spatial:" + "|".join(block_nodes) if block_nodes else nodes[0]
        for node in nodes:
            if node.startswith("well:"):
                well_groups[node[5:]] = group
    return wells, well_groups


def split_for(seed, group, ratios):
    digest = hashlib.sha256(f"{seed}\0{group}".encode("utf-8")).digest()
    value = int.from_bytes(digest, "big") / (1 << 256)
    if value < ratios[0]:
        return "train"
    if value < ratios[0] + ratios[1]:
        return "validation"
    return "test"


def display_path(path):
    try:
        return path.relative_to(Path.cwd().resolve()).as_posix()
    except ValueError:
        return str(path)


def validate_manifest(manifest, wells, well_groups):
    ratios_map = manifest.get("ratios")
    if not isinstance(ratios_map, dict) or set(ratios_map) != set(SPLITS):
        raise ValueError("manifest ratios must contain train, validation, and test")
    ratios = validate_ratios(tuple(ratios_map[name] for name in SPLITS))
    splits = manifest.get("splits")
    if not isinstance(splits, dict) or set(splits) != set(SPLITS):
        raise ValueError("manifest splits must contain train, validation, and test")
    seen = {}
    group_splits = {}
    for split in SPLITS:
        if not isinstance(splits[split], list):
            raise ValueError(f"{split} must be an array")
        for well_id in splits[split]:
            if not isinstance(well_id, str) or well_id in seen:
                raise ValueError("well IDs must be strings and globally unique")
            seen[well_id] = split
            group = well_groups.get(well_id)
            if group is None:
                raise ValueError(f"unknown well ID: {well_id}")
            if group in group_splits and group_splits[group] != split:
                raise ValueError(f"group leakage detected for {group}")
            group_splits[group] = split
            if split_for(manifest["seed"], group, ratios) != split:
                raise ValueError(f"incorrect deterministic split for {well_id}")
    if set(seen) != wells:
        raise ValueError("split well IDs do not match source well IDs")
    encoded = json.dumps(manifest, allow_nan=False, sort_keys=True)
    strict_json(encoded)


def build_manifest(paths, seed, ratios, latitude_size, longitude_size, generated_at=None):
    wells, well_groups = read_groups(paths, latitude_size, longitude_size)
    splits = {name: [] for name in SPLITS}
    for well_id in sorted(wells):
        splits[split_for(seed, well_groups[well_id], ratios)].append(well_id)
    manifest = {
        "schemaVersion": 1,
        "generatedAt": generated_at or datetime.now(timezone.utc).isoformat().replace("+00:00", "Z"),
        "seed": seed,
        "ratios": dict(zip(SPLITS, ratios)),
        "sourceFiles": [
            {"path": display_path(path), "sha256": file_sha256(path)} for path in paths
        ],
        "strategy": {
            "unit": "Well.MetaInfo.ID",
            "locatedRecords": "latitude/longitude spatial block",
            "unlocatedRecords": "whole well ID",
            "latitudeCellSizeDegrees": float(latitude_size),
            "longitudeCellSizeDegrees": float(longitude_size),
            "assignment": "SHA-256(seed + NUL + canonical group), without randomness",
        },
        "splits": splits,
        "prospectiveHoldout": {
            "status": "Current records are development-only.",
            "policy": (
                "Future source snapshots must be assigned by first-seen transaction time, "
                "never retroactively mixed with development records."
            ),
        },
    }
    validate_manifest(manifest, wells, well_groups)
    return manifest


def write_manifest(path, manifest):
    path.parent.mkdir(parents=True, exist_ok=True)
    temporary = path.with_suffix(path.suffix + ".part")
    try:
        temporary.write_text(
            json.dumps(manifest, indent=2, allow_nan=False) + "\n", encoding="utf-8", newline="\n"
        )
        strict_json(temporary.read_text(encoding="utf-8"))
        os.replace(temporary, path)
    finally:
        temporary.unlink(missing_ok=True)


def self_check():
    with tempfile.TemporaryDirectory(prefix=".benchmark-splits-", dir=Path.cwd()) as directory:
        root = Path(directory)
        located = root / "located" / "records.jsonl"
        force = root / "force" / "records.jsonl"
        located.parent.mkdir()
        force.parent.mkdir()
        records = [
            {"SourceLocation": {"Latitude": 10.01, "Longitude": -20.01}, "Well": {"MetaInfo": {"ID": "A"}}},
            {"SourceLocation": {"Latitude": 10.02, "Longitude": -20.02}, "Well": {"MetaInfo": {"ID": "B"}}},
            {"SourceLocation": {"Latitude": 11.0, "Longitude": -21.0}, "Well": {"MetaInfo": {"ID": "C"}}},
        ]
        located.write_text(
            "".join(json.dumps(record) + "\n" for record in records), encoding="utf-8"
        )
        force.write_text(
            json.dumps({"Well": {"MetaInfo": {"ID": "D"}}}) + "\n", encoding="utf-8"
        )
        paths = source_paths([root / "located", force])
        ratios = validate_ratios(DEFAULT_RATIOS)
        lat_size = validate_cell_size("0.1", "latitude cell size")
        lon_size = validate_cell_size("0.1", "longitude cell size")
        timestamp = "2000-01-01T00:00:00Z"
        first = build_manifest(paths, "self-check", ratios, lat_size, lon_size, timestamp)
        second = build_manifest(paths, "self-check", ratios, lat_size, lon_size, timestamp)
        assert first == second
        membership = {
            well_id: split for split, well_ids in first["splits"].items() for well_id in well_ids
        }
        assert membership["A"] == membership["B"]
        output = root / "manifest.json"
        write_manifest(output, first)
        assert strict_json(output.read_text(encoding="utf-8")) == first
        leaked = json.loads(json.dumps(first))
        source_split = membership["A"]
        target_split = next(split for split in SPLITS if split != source_split)
        leaked["splits"][source_split].remove("A")
        leaked["splits"][target_split].append("A")
        try:
            _, well_groups = read_groups(paths, lat_size, lon_size)
            validate_manifest(leaked, set(membership), well_groups)
        except ValueError:
            pass
        else:
            raise AssertionError("group leakage was accepted")
    print("self-check passed")


def main():
    parser = argparse.ArgumentParser(
        description="Build deterministic, leakage-resistant benchmark split manifests."
    )
    parser.add_argument("inputs", nargs="*", type=Path, help="JSONL files or dataset directories")
    parser.add_argument("--output", type=Path, help="output manifest path")
    parser.add_argument("--seed", default="0", help="deterministic split seed")
    parser.add_argument(
        "--ratios",
        nargs=3,
        type=float,
        default=DEFAULT_RATIOS,
        metavar=("TRAIN", "VALIDATION", "TEST"),
    )
    parser.add_argument(
        "--latitude-cell-size", "--lat-cell-size", type=str, default="0.1", dest="latitude_size"
    )
    parser.add_argument(
        "--longitude-cell-size", "--lon-cell-size", type=str, default="0.1", dest="longitude_size"
    )
    parser.add_argument("--self-check", action="store_true")
    args = parser.parse_args()
    try:
        if args.self_check:
            self_check()
            return
        if not args.inputs:
            raise ValueError("at least one JSONL file or dataset directory is required")
        if args.output is None:
            raise ValueError("--output is required")
        paths = source_paths(args.inputs)
        output = args.output.resolve()
        if output in paths:
            raise ValueError("output path must not overwrite a source JSONL file")
        ratios = validate_ratios(args.ratios)
        latitude_size = validate_cell_size(args.latitude_size, "latitude cell size")
        longitude_size = validate_cell_size(args.longitude_size, "longitude cell size")
        manifest = build_manifest(paths, args.seed, ratios, latitude_size, longitude_size)
        write_manifest(args.output, manifest)
        print(f"{args.output}: {sum(map(len, manifest['splits'].values()))} wells")
    except (OSError, ValueError) as error:
        parser.error(str(error))


if __name__ == "__main__":
    main()
