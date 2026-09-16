---
name: drillsim-synthetic-assets
description: >-
  Seed the Field, Rig, Cluster, Slot, Well, and Survey Instrument MCP services
  with coherent synthetic exploration assets. Use after the DrillSim synthetic
  foundation or when asked to populate a sample oil field surface graph.
---

# DrillSim synthetic surface assets

Use the foundation ledger from `drillsim-synthetic-foundation`. If it is
missing, run that skill first. Keep the same slug, UUIDv5 convention,
`SYNTH-{slug}` marker, SI units, and get-before-create collision rule.

## MCP inventory

- Field (41): `ping`; `field_batch_{export,restore}`;
  `field_{get_all_ids,get_all_meta_info,get_by_id,get_all,get_all_light,create,update_by_id,delete_by_id}`;
  `field_{forward,inverse}_convert_coordinates`; and seven-operation
  get/list/create/update/delete families for `field_delineation_line_type`,
  `field_feature_category`, `field_identity`, and
  `field_membership_category`.
- Rig (18): `ping`; the
  `rig_{get_all_ids,get_all_meta_info,get_by_id,get_all_light,get_all,create,update_by_id,delete_by_id,batch_export,batch_restore}`
  family; and seven CRUD/list tools for `rig_feature_category`.
- Cluster (36): `ping`; `cluster_batch_{export,restore}`;
  `cluster_{get_all_ids,get_all_meta_info,get_by_id,get_all,get_all_light,get_all_by_field_id,get_all_by_rig_id,get_all_single_well,get_all_fixed_platform,create,update_by_id,delete_by_id}`;
  and seven-operation families for `cluster_feature_category`,
  `cluster_identity`, and `slot_feature_category`.
- Survey Instrument (16): `ping`; eight tools for `survey_instrument`
  including light reads and CRUD; seven CRUD/list tools for `error_source`.
- Well (11): `ping`;
  `well_{get_all_ids,get_all_meta_info,get_by_id,get_all,get_all_by_slot_id,get_all_by_cluster_id,get_used_slot_meta_info_by_cluster_id,create,update_by_id,delete_by_id}`.

Inspect the live input schema of every create tool. Do not guess nested
Gaussian-property, equipment, feature-option, or error-model shapes.

## Dataset

Create the smallest graph that still supports spatial interpolation:

1. One field with the foundation projection/reference point and closed boundary.
2. Minimal field catalogs:
   - delineation type `LeaseBoundary`
   - feature category `PlayType`, option `StructuralStratigraphic`
   - identity `SyntheticFieldCode`
   - membership category `Portfolio`, option `Exploration`
3. One mobile land rig. Set `IsFixedPlatform=false` and `ClusterID=null` to
   avoid the fixed-platform circular reference. Include plausible static
   ratings and only equipment required by the live schema.
4. Reuse the service-seeded `WdWGoodMag` survey instrument
   (`2af52fd1-84a9-4fe0-9ea3-3d4c6256b2b5`). Its Wolff-DeWardt parameters are
   complete and accepted by the Trajectory covariance calculator. Create a
   custom error model only when explicitly testing survey-model authoring.
5. Six single-well clusters at projected offsets from field center:

   | Name | East (m) | North (m) |
   | --- | ---: | ---: |
   | Aster | -3500 | -2200 |
   | Birch | -2100 | 1800 |
   | Cedar | -200 | -2800 |
   | Dogwood | 900 | 2600 |
   | Elm | 3300 | -900 |
   | Fir | 3900 | 2300 |

   Each cluster references the field and rig, has `IsSingleWell=true`,
   `IsFixedPlatform=false`, and contains one slot whose dictionary key exactly
   equals `Slot.ID`.
6. One well per cluster/slot with matching `ClusterID`, `SlotID`, and
   `IsSingleWell=true`.

Convert every offset to WGS 84 with `field_inverse_convert_coordinates`; use
those coordinates for cluster reference points and slots. Keep slot latitude
and longitude in the exact Gaussian-property shape required by the MCP schema.

## Creation order

Create field-owned catalogs, field, rig, then each cluster and well. Cluster
external references are checked live, so never create it before its field and
rig.

After writes, verify:

- `cluster_get_all_by_field_id` returns exactly the six scenario clusters.
- `cluster_get_all_by_rig_id` returns the same six.
- every slot dictionary key equals its nested slot ID.
- `well_get_all_by_cluster_id` and `well_get_all_by_slot_id` each return the
  expected well.
- a forward conversion of each stored WGS 84 point returns its original offset
  within one metre.
Return the complete ID ledger for the downhole skill. Do not use batch restore
for initial generation; direct creates provide clearer failure isolation.
for initial generation; direct creates provide clearer failure isolation.
