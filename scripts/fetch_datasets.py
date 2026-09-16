#!/usr/bin/env python3
import argparse
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
from urllib.request import Request, urlopen


CHUNK_SIZE = 1024 * 1024


def file_hash(path, algorithm):
    digest = hashlib.new(algorithm)
    with path.open("rb") as stream:
        while chunk := stream.read(CHUNK_SIZE):
            digest.update(chunk)
    return digest.hexdigest()


def validate_manifest(manifest):
    if manifest.get("schemaVersion") != 1:
        raise ValueError("schemaVersion must be 1")
    datasets = manifest.get("datasets")
    if not isinstance(datasets, list) or not datasets:
        raise ValueError("datasets must be a non-empty array")
    dataset_ids = set()
    artifact_paths = set()
    for dataset in datasets:
        dataset_id = dataset.get("id")
        if not dataset_id or dataset_id in dataset_ids:
            raise ValueError("dataset IDs must be non-empty and unique")
        dataset_ids.add(dataset_id)
        license_info = dataset.get("license", {})
        if not license_info.get("id") or not license_info.get("url") or not license_info.get("attribution"):
            raise ValueError(f"{dataset_id}: license id, url, and attribution are required")
        if dataset.get("redistributable") is not True:
            raise ValueError(f"{dataset_id}: redistributable must be explicitly true")
        artifacts = dataset.get("artifacts")
        if not isinstance(artifacts, list) or not artifacts:
            raise ValueError(f"{dataset_id}: artifacts must be a non-empty array")
        for artifact in artifacts:
            relative = PurePosixPath(artifact.get("path", ""))
            if not relative.parts or relative.is_absolute() or ".." in relative.parts:
                raise ValueError(f"{dataset_id}: unsafe artifact path")
            if str(relative) in artifact_paths:
                raise ValueError(f"duplicate artifact path: {relative}")
            artifact_paths.add(str(relative))
            if not artifact.get("url", "").startswith("https://"):
                raise ValueError(f"{dataset_id}/{relative}: HTTPS URL required")
            checksum = artifact.get("checksum")
            if checksum is not None:
                algorithm = checksum.get("algorithm")
                value = checksum.get("value", "")
                lengths = {"md5": 32, "sha256": 64}
                if algorithm not in lengths or len(value) != lengths[algorithm] or any(
                    character not in "0123456789abcdefABCDEF" for character in value
                ):
                    raise ValueError(f"{dataset_id}/{relative}: invalid checksum")
            expected_bytes = artifact.get("bytes")
            if expected_bytes is not None and (not isinstance(expected_bytes, int) or expected_bytes <= 0):
                raise ValueError(f"{dataset_id}/{relative}: bytes must be a positive integer")
    return manifest


def verify_artifact(path, artifact):
    expected_bytes = artifact.get("bytes")
    if expected_bytes is not None and path.stat().st_size != expected_bytes:
        raise ValueError(f"{path}: expected {expected_bytes} bytes, found {path.stat().st_size}")
    checksum = artifact.get("checksum")
    if checksum:
        actual_source_hash = file_hash(path, checksum["algorithm"])
        if actual_source_hash.lower() != checksum["value"].lower():
            raise ValueError(f"{path}: {checksum['algorithm']} mismatch")
    return file_hash(path, "sha256")


def fetch_artifact(root, dataset, artifact):
    destination = (root / Path(*PurePosixPath(artifact["path"]).parts)).resolve()
    destination.relative_to(root.resolve())
    destination.parent.mkdir(parents=True, exist_ok=True)
    if destination.exists():
        actual_hash = verify_artifact(destination, artifact)
    else:
        request = Request(artifact["url"], headers={"User-Agent": "DrillSim dataset fetcher/1.0"})
        temporary = destination.with_suffix(destination.suffix + ".part")
        try:
            with urlopen(request, timeout=120) as response, temporary.open("wb") as output:
                while chunk := response.read(CHUNK_SIZE):
                    output.write(chunk)
            actual_hash = verify_artifact(temporary, artifact)
            os.replace(temporary, destination)
        finally:
            temporary.unlink(missing_ok=True)
    receipt = {
        "datasetId": dataset["id"],
        "artifactPath": artifact["path"],
        "sourceUrl": artifact["url"],
        "retrievedAt": datetime.now(timezone.utc).isoformat(),
        "bytes": destination.stat().st_size,
        "sha256": actual_hash,
        "sourceChecksum": artifact.get("checksum"),
        "license": dataset["license"],
    }
    receipt_path = destination.with_suffix(destination.suffix + ".receipt.json")
    receipt_path.write_text(json.dumps(receipt, indent=2) + "\n", encoding="utf-8")
    print(f"{dataset['id']}: {artifact['path']} ({destination.stat().st_size} bytes)")


def load_manifest(path):
    return validate_manifest(json.loads(path.read_text(encoding="utf-8")))


def self_check():
    valid = {
        "schemaVersion": 1,
        "datasets": [
            {
                "id": "example",
                "license": {"id": "CC0-1.0", "url": "https://example.test/license", "attribution": "Example"},
                "redistributable": True,
                "artifacts": [{"url": "https://example.test/data.csv", "path": "example/data.csv"}],
            }
        ],
    }
    validate_manifest(valid)
    invalid = json.loads(json.dumps(valid))
    invalid["datasets"][0]["artifacts"][0]["path"] = "../escape.csv"
    try:
        validate_manifest(invalid)
    except ValueError:
        print("self-check passed")
        return
    raise AssertionError("unsafe path was accepted")


def main():
    parser = argparse.ArgumentParser(description="Fetch rights-cleared DrillSim benchmark artifacts.")
    parser.add_argument("command", choices=("check", "fetch", "self-check"))
    parser.add_argument("--manifest", type=Path, default=Path("datasets/manifest.json"))
    parser.add_argument("--dataset", action="append", dest="dataset_ids")
    parser.add_argument("--root", type=Path, default=Path("data/raw"))
    args = parser.parse_args()
    if args.command == "self-check":
        self_check()
        return
    manifest = load_manifest(args.manifest)
    if args.command == "check":
        print(f"manifest valid: {len(manifest['datasets'])} datasets")
        return
    selected = set(args.dataset_ids or [])
    known = {dataset["id"] for dataset in manifest["datasets"]}
    unknown = selected - known
    if unknown:
        raise ValueError(f"unknown datasets: {', '.join(sorted(unknown))}")
    for dataset in manifest["datasets"]:
        if selected and dataset["id"] not in selected:
            continue
        for artifact in dataset["artifacts"]:
            if artifact.get("selected", True):
                fetch_artifact(args.root, dataset, artifact)


if __name__ == "__main__":
    main()
