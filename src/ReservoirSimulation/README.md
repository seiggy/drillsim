# ReservoirSimulation private setup

The operator-only setup endpoints prepare conditioned synthetic workflow models,
not calibrated commercial forecasts. Both require `X-DrillSim-Operator-Key`, like
the existing world endpoints. They are intended for the DrillingOperations
operator facade, not direct browser or AI access.

- `GET /reservoirsimulation/api/worlds/setup-profiles?fieldId=<guid>&reservoirName=<name>`
  returns `{fieldId,reservoirName,profiles:[{profileId,name,description,worldModelVersion}]}`.
  An empty catalog means no supported prepared model exists for that exact field
  and reservoir. The catalog contains no world IDs, seeds, conditioning controls,
  physics values, calibration paths, or truth arrays.
- `POST /reservoirsimulation/api/worlds/setup` accepts exactly
  `{fieldId,reservoirName,profileId,resolution,realizationSeed}` and returns the
  existing private `WorldSummary` for binding. All properties are required;
  unknown properties are rejected. Field IDs use nonempty lowercase hyphenated
  GUIDs, reservoir names must be nonblank and at most 200 characters, resolution
  is exactly `Preview` or `Standard`, and seeds are JSON integers in
  `0..2147483647`. Unavailable scoped profile IDs return 404; invalid input
  returns 400.

Profiles are derived from persisted current-model specifications, including any
installed Troll/Sognefjord model without a hardcoded world ID. A profile hashes
the canonical request with seed zero and Standard grid counts, retaining padding,
calibration identity, heterogeneity, structural/property conditioning, and fluid
contacts. Thus seed/grid-count variants share a stable opaque profile ID, while
distinct model blueprints remain separate. Scoped specifications are read in
bounded, parameterized pages ordered by creation time and world ID. Every
candidate is checked for canonical identity and truth checksum, even when cached;
the earliest verified representative supplies the template. Historical model
versions are excluded, never migrated. Integrity failures stop setup rather than
silently hiding models.

Preview uses `16 × 16 × 8` cells; Standard uses `64 × 64 × 20`. Setup copies the
template and changes only its seed and grid counts. Existing deterministic
persistence makes repeated identical requests idempotent. No existing world
specification, simulation state, or state pin is rewritten or deleted; setup does
not run a simulation or perform Save, Seal, Approve, or Start.
