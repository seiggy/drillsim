---
name: drillsim-opportunity-analysis
description: >-
  Analyze a DrillSim synthetic oil field to rank exploratory drilling
  locations and estimate P90/P50/P10 net pay for a proposed wellbore. Use when
  asked where to drill next, to find the best prospect, or to estimate expected
  paydirt from the synthetic dataset.
---

# DrillSim exploratory opportunity analysis

Analyze existing service data; do not create a planned well unless explicitly
asked after presenting the recommendation.

If the requested synthetic scenario does not exist yet, run
`drillsim-synthetic-foundation`, `drillsim-synthetic-assets`, and
`drillsim-synthetic-downhole` first.

## Load and validate

1. Identify the scenario field by its deterministic ID and `SYNTH-{slug}`
   marker.
2. Use MCP reads for field, clusters, wells, projection, rig, survey instrument,
   units, and earth-model provenance.
3. Use the existing REST reads for well bores, trajectories, geology,
   geothermal data, architecture, strings, fluids, YPL records, and simulations.
4. Reject unresolved IDs, mixed scenario slugs, non-SI values, prognosed records
   presented as observations, or fewer than four actual geological wells.

## Rank candidates

Build a 250 m projected grid inside the field boundary. Exclude points:

- within 500 m of an existing bottom-hole location
- within 250 m of the boundary
- whose proposed straight-line well path comes within 100 m of an existing path

For each remaining point, use the four nearest actual wells and inverse-distance
squared weights to interpolate reservoir top, base, net pay, porosity, and
`log10(permeability_mD)`. Derive observed net pay by summing depth segments where
porosity is at least 0.12, permeability is at least 1 mD, and pressure
differential is positive.

Estimate uncertainty from weighted neighbor disagreement plus distance to the
nearest observation. Rank with:

```text
score = p50_net_pay_m
      * mean_porosity
      * log10(1 + permeability_mD)
      / (1 + relative_uncertainty)
```

`ponytail: brute-force grid and nearest-neighbor scan; use a spatial index only
if the scenario grows beyond roughly 10,000 candidates.`

For net-pay mean `mu` and standard deviation `sigma`, report petroleum
exceedance conventions:

```text
P90 = max(0, mu - 1.2816*sigma)
P50 = mu
P10 = mu + 1.2816*sigma
```

## Output

Return the top five candidates with projected and WGS 84 coordinates, nearest
well distance, target top/base TVD, P90/P50/P10 net pay, expected porosity,
permeability, uncertainty, and score. Explain the winning location in terms of
nearby evidence and whether the result is interpolation or extrapolation.

Call expected net pay "expected paydirt." Do not label it reserves or
recoverable volume: the current ontology lacks saturation, fluid contacts,
formation-volume factor, drainage area, and recovery factor.

If the user asks to persist the winner, create a new single-well cluster/slot,
well, well bore, planned definitive trajectory, prognosed geological record,
architecture, and simulation case using the same creation rules as the other
skills. Keep observed and prognosed geology distinct.
