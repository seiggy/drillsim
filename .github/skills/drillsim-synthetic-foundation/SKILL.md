---
name: drillsim-synthetic-foundation
description: >-
  Create or reuse the deterministic geodetic, projection, unit, gravity,
  magnetic, and vertical-datum foundation for a synthetic DrillSim oil field.
  Use when asked to seed a sample field, build synthetic oilfield data, prepare
  geospatial drilling data, or start an exploration scenario.
---

# DrillSim synthetic foundation

Build the shared spatial foundation before creating field assets. Use the
service MCP tools directly; do not add generator code or dependencies.

## Defaults

Unless the user supplies values, use:

- scenario slug: `northstar`
- center: 59.2000 degrees north, 2.1000 degrees east
- field size: 12 km east-west by 10 km north-south
- reference date: `2026-07-01T00:00:00Z`
- units: SI in service payloads; convert degrees to radians before calls
- names/descriptions: prefix with `SYNTH-{slug}` and state that records are
  fictional

Generate every resource UUID deterministically as UUIDv5 using the URL
namespace and `drillsim:{slug}:{service}:{entity}:{name}`. Before a create,
call its get-by-ID tool. Reuse a matching synthetic record, create when absent,
and stop on an ID collision with non-synthetic data. Never update or delete
records outside this scenario.

## MCP inventory

The source currently exposes these foundation tools:

- Unit Conversion (10): `search_physical_quantities`,
  `get_physical_quantity`, `convert_values`,
  `convert_between_unit_systems`, `list_unit_systems`, `get_unit_system`,
  `create_unit_system`, `replace_unit_system`, `delete_unit_system`, and
  `search_documentation`.
- Geodetic Datum (28): `ping`; CRUD/list/find families for `spheroid` and
  `geodetic_datum`; CRUD/list for `geodetic_conversion_set`;
  `geodetic_datum_convert_coordinate`; and
  `geodetic_datum_usage_statistics_get`. Metadata list tools in this service
  end in `_get_all_meta`, not `_meta_info`.
- Cartographic Projection (21): `ping`; CRUD/list families for
  `cartographic_projection` and `cartographic_conversion_set`; read-only
  `cartographic_projection_type_{get_all_ids,get_by_id,get_all}`; and
  `cartographic_projection_usage_statistics_get`.
- Earth Gravity (3): `ping`, `earth_gravity_get_model_info`,
  `earth_gravity_evaluate`.
- Earth Magnetic Field (3): `ping`,
  `earth_magnetic_field_get_model_info`,
  `earth_magnetic_field_evaluate`.
- Earth Vertical Datum (4): `ping`,
  `earth_vertical_datum_get_model_info`,
  `earth_vertical_datum_convert_mean_sea_level_to_wgs84`,
  `earth_vertical_datum_convert_wgs84_to_mean_sea_level`.

Inspect each selected tool's input schema before constructing its payload.
Usage-statistics and documentation-search tools are inventory/admin aids, not
scenario records.

## Workflow

1. Check connectivity and record model provenance with each available `ping`
   and `*_get_model_info` tool.
2. Use `list_unit_systems`; reuse the built-in SI system. Use conversion tools
   only at input/output boundaries. Do not create a custom unit system.
3. Find and reuse WGS 84 with `spheroid_find_id_by_name` and
   `geodetic_datum_find_id_by_name`. Create neither when the seeded definitions
   exist.
4. Read projection types and select UTM. Create one projection for the center's
   zone, referencing the WGS 84 datum. Use only parameters marked applicable by
   the projection-type prototype.
5. Build the rectangular boundary around the center in projected metres. Use a
   temporary `cartographic_conversion_set`: create it, read the calculated
   center/corners, then delete it. Persist it only when the user asks for an
   audit record.
6. Evaluate gravity, magnetic field, and both vertical-datum directions at the
   center and four corners. Preserve input order and model provenance.
7. Return a compact ledger containing the slug, deterministic IDs, center,
   projected boundary, WGS 84 boundary, model versions, and reference date for
   the asset skill.

When the request is for a complete dataset, continue immediately with
`drillsim-synthetic-assets` and `drillsim-synthetic-downhole`; the foundation
ledger is not a finished oilfield.

## Guardrails

- Respect positive-down depth conventions.
- Keep angles in radians and distances in metres in service payloads.
- Treat stateless earth-model results as observations in the ledger, not as
  persisted service entities.
- Fail the whole foundation step if any corner fails conversion; do not keep a
  partial geometry.
