# DrillSim public-data pipeline

`manifest.json` is the file-level rights and checksum allowlist. Raw downloads
and staged records stay under ignored `data\raw` and `data\staged` directories.

```powershell
python .\scripts\fetch_datasets.py check
python .\scripts\fetch_datasets.py fetch --dataset force-2020 --dataset sodir-force-troll --dataset sodir-force-15-9 --dataset kgs --dataset usgs-williston

python .\scripts\stage_datasets.py force-2020 --limit 10
python .\scripts\stage_datasets.py force-sodir-15-9
python .\scripts\stage_datasets.py force-sodir-troll
python .\scripts\stage_datasets.py kgs --limit 250
python .\scripts\stage_datasets.py usgs-williston --limit 250

python .\scripts\load_staged.py .\data\staged\force-sodir-15-9\records.jsonl --limit 5
python .\scripts\load_staged.py .\data\staged\force-sodir-troll\records.jsonl --limit 46
python .\scripts\load_staged.py .\data\staged\kgs-ellis-11s-16w\records.jsonl --limit 5 --dry-run
python .\scripts\load_staged.py .\data\staged\usgs-williston\records.jsonl --limit 5
$env:RESERVOIR_SIMULATION_OPERATOR_KEY = "<same-random-operator-key>"
python .\scripts\seed_reservoir_world.py

python .\scripts\build_benchmark_splits.py `
  .\data\staged\force-2020 `
  .\data\staged\kgs-ellis-11s-16w `
  .\data\staged\usgs-williston `
  --output .\data\staged\benchmark-splits.json `
  --seed drillsim-v1 `
  --latitude-cell-size 0.02 `
  --longitude-cell-size 0.02
```

The staging output is DrillSim-shaped JSONL, not a silent database import.
Every source value remains distinguishable as observed, human-interpreted,
derived, model-estimated, or synthetic. Original units and nulls are retained.

| Phase | Status | Purpose |
| --- | --- | --- |
| Fetch and checksum | Implemented | Download only allowlisted, redistributable artifacts and write receipts. |
| FORCE LAS staging | Implemented | Preserve common MD axes, observed curves, source nulls, and supplied interpretations. |
| FORCE-SODIR 15/9 merge | Implemented | Join five exact UWIs to WGS84 wellheads, metadata, lithostratigraphy, casing, and varied calculated trajectories. |
| Troll-Sognefjord reservoir package | Implemented | Preserve all 29 parent wells and 46 intersecting wellbores; nine FORCE-logged bores provide petrophysics and trajectories. |
| KGS pilot staging | Implemented | Stage Ellis County headers, lifecycle dates, and explicitly unconfirmed tops. |
| USGS staging | Implemented | Stage authoritative Williston locations and TVDSS formation picks. |
| Coordinate/topology load | Implemented | Transform NAD27 or accept SODIR WGS84, create the field graph, calculate trajectories, load architecture, then POST geology. |
| Benchmark splits | Implemented | Freeze whole-well and spatial-block splits; reserve later source snapshots for prospective holdout. |

Do not redistribute KGS third-party LAS/scans, proprietary USGS source logs, or
SODIR linked documents. Those are excluded from `manifest.json`.

The FORCE-SODIR pilot keeps FORCE curves and SODIR picks as observed or
human-interpreted evidence. PHIE, permeability, Archie saturation, phase split,
flow, and pressure-differential curves are explicitly classified as model
estimates. They make the opportunity workflow executable; they are not measured
reservoir properties or reserves evidence.

`reservoir-fluid-calibration.json` seals model-estimated PVT, relative
permeability, and FORCE-derived GWC controls in the field-local Riemannian frame.
Its SHA-256 is part of both scenario and hidden-world identity.

The 5 m screening resample uses the FORCE lithofacies codes for sandstone and
sandstone/shale, density-neutron RMS porosity with a 2.65 g/cc sandstone matrix
and 1.05 g/cc fluid, and Archie `a=1`, `m=n=2`, `Rw=0.08`. The permeability,
phase split, pressure differential, and flow values are uncalibrated model
proxies and remain tagged `ModelEstimated`.
