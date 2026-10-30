---
name: drillsim-glossary
description: Plain-language definitions of the petrophysics, reservoir and forecasting terms used on DrillSim screens (net pay, porosity, permeability, P90/P50/P10, contacts, gas cap, oil leg, formation volume factor, score). Use when a user asks what a term or number means.
---

# DrillSim glossary

Explain terms in one or two plain sentences, then relate them to the number on the user's screen when there is one.

- **Net pay / expected pay**: thickness (metres) of reservoir rock good enough to produce, i.e. it passes the porosity and
  permeability cutoffs in Settings. With a target hydrocarbon selected, only good rock holding that fluid counts ("oil pay",
  "gas pay"). It is a thickness, never a volume of oil or gas and never reserves.
- **Porosity**: share of the rock that is open pore space that can hold fluid, as a percentage. Good sandstone: 20–30%.
- **Permeability (mD)**: how easily fluid flows through the rock. Below about 1 mD fluids barely flow; hundreds of mD is
  excellent.
- **P90 / P50 / P10**: the low, best and high cases of a forecast. Reality should fall between P90 and P10 about 80% of the
  time if the forecast is well calibrated.
- **Score (Prospects)**: expected pay × porosity × log10(1 + permeability in mD) ÷ (1 + relative uncertainty). Better rock
  can outrank a thicker but poorer or less certain target.
- **Uncertainty**: grows when the nearby wells disagree and when the target is far from them.
- **TVD**: true vertical depth below the reference datum, in metres. **MD** is measured depth along the borehole.
- **Fluid contacts**: depths where one fluid gives way to another in the reservoir. GOC = gas–oil contact, OWC = oil–water
  contact, GWC = gas–water contact. Gas floats on oil, which floats on water.
- **Gas cap / oil leg**: the gas-filled top of a reservoir and the oil-filled band beneath it. Troll has a thin oil leg, so a
  vertical well into it often produces gas pulled down from the cap.
- **Bound (irreducible) water**: water held on the rock grains that cannot flow; it is why logs show 5–15% water even in
  gas zones, and why shale reads near 100% water without being a water zone.
- **Formation volume factor**: converts reservoir volumes to surface volumes. Gas expands about 194× on the way up here;
  oil shrinks slightly (factor 1.12).
- **Reservoir model**: DrillSim's static model built from the visible wells: interpolated top/base surfaces, layer rock
  properties from logs, and flat field-wide fluid contacts fitted to the log saturations.
- **Blind simulator / hidden truth**: a separate model of the "real" reservoir that the forecast cannot see; Results compares
  the sealed forecast against it.
