---
name: drillsim-synthetic-downhole
description: >-
  Complete a synthetic DrillSim oil field with wellbores, trajectories,
  architecture, geology, temperature, drill strings, fluids, rheology, and
  simulation records. Use after synthetic surface assets or when asked for a
  full downhole oilfield ontology.
---

# DrillSim synthetic downhole graph

Use the asset ledger from `drillsim-synthetic-assets`. Preserve its deterministic
IDs, marker, SI units, and collision rules.

## Current transport gaps

Do not pretend every service has domain MCP tools:

- Ping-only MCP servers: Drill String, Well Bore, Well Bore Architecture, and
  Simulator 4nDOF. Each exposes only `ping`.
- No MCP server registration: Drilling Fluid, Geological Properties,
  Geothermal Properties, Trajectory, and YPL Calibration.

Call the four pings. For all nine services, use their existing REST APIs and
live OpenAPI documents at `/swagger/v1/swagger.json`. Select operations by
operation ID, inspect request schemas, and use the advertised server/base path.
This is a transport fallback, not new application logic.

## Per-well graph

For each of the six wells, create in order:

1. Well bore referencing the well and mobile rig; not a sidetrack.
2. Survey run referencing field, cluster, well, well bore, and the service-seeded
   `WdWGoodMag` survey instrument. Supply at least three monotonically
   increasing survey stations and wait for its calculation to complete.
3. Actual definitive trajectory referencing the same four parents and the
   survey run through `SurveyRunSectionList`. Use a positive `MDStep` and a
   gentle build-and-hold path aimed toward the synthetic reservoir lens; wait
   for its calculation to complete.
4. Well-bore architecture referencing the well bore, with wellhead, surface
   section, conductor, casing sections, and open-hole interval in top-to-bottom
   order.
5. Drill string referencing the well bore, with drill pipe, heavyweight pipe,
   and BHA sections plus one downhole sensor.
6. Drilling-fluid description and drilling-fluid record referencing the drill
   string. Use a water-based fluid near 1200 kg/m3 and a plausible
   yield-power-law flow curve.
7. Geothermal properties referencing well bore and trajectory, sampled from
   surface to total depth with a baseline 0.030 K/m gradient.
8. Actual geological properties referencing well bore and trajectory.
9. One simulation referencing the well bore and IDs for rig, trajectory,
   architecture, drill string, fluid description, and geothermal properties.

Create one shared Couette rheometer, rheogram, YPL calibration, and correction
through the YPL REST API, then reference or describe that calibration in the
fluid records where the live schemas permit it.

## Synthetic geology

Let `x` and `y` be projected metres east and north of field center. Generate an
elongated northeast-trending reservoir quality:

```text
u =  (x - 1800) * cos(25deg) + (y - 1200) * sin(25deg)
v = -(x - 1800) * sin(25deg) + (y - 1200) * cos(25deg)
q = exp(-(u / 3500)^2 - (v / 1400)^2)
top_tvd_m       = 2450 + 0.018*x - 0.010*y
gross_thickness = 28 + 32*q
porosity        = 0.13 + 0.11*q
permeability_m2 = 9.869233e-16 * 5 * 10^(2*q)
pressure_pa     = 4.0e6 + 3.0e6*q
```

Sample from 20 m above reservoir top to 20 m below base at 10 m spacing.
Inside the reservoir, populate measured depth, porosity, permeability, pressure
differential, internal friction angle, and plausible confined/unconfined
strengths. Outside it, use compact non-pay background values. Give every
Gaussian property a non-zero standard deviation; uncertainty grows with
distance from the lens center.

The six wells are evidence, not identical copies: apply deterministic,
zero-mean variations derived from each well UUID, capped at 5% for depth and
10% for rock properties. Never use random values without a recorded seed.

## Verification

Read every created record back and check every UUID edge. Confirm trajectory
stations are ordered, architecture intervals do not overlap, geological and
geothermal depths cover total depth, and simulation contextual IDs resolve.
Do not report success for a partial graph; return the failed service/entity and
retain the successful ID ledger for a safe rerun.
