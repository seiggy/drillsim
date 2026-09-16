# DrillSim Design

## Visual World

The analysis portal uses the **Hypothesis Sequencer** system: a scientific
instrument surface where geological reasoning is expressed as a playable
sixteen-step sequence rather than a dashboard plus chatbot.

The portal is charcoal and calibrated. White steps represent source evidence,
yellow steps interpretation settings, orange steps prospect generation, and red
steps challenge/alternative work. These colors are state laws and always appear
with text labels and step numbers.

## Composition

- The selected field hypothesis owns the upper stage.
- The upper stage is one synchronized analysis workspace with Map, 2D Section,
  3D Reservoir, Logs, and Crossplot modes.
- Top-down field geography uses Azure Maps; custom SVG/canvas plotting is
  reserved for subsurface and petrophysical views.
- Existing well controls and ranked targets share one spatial plotting surface.
- The target readout remains registered to the selected target.
- A selected well is shared across every workspace mode. Selecting a map marker,
  trajectory, neighbor, or well menu updates all views.
- Field and reservoir are separate selectors. Reservoir options report
  intersecting wellbore counts; evidence readouts separately report parent
  wells, wellbores, and logged controls.
- The sixteen-step row never wraps; narrow screens scroll it horizontally.
- Evidence-bank and AI field-note bays sit below the sequence as consequences of
  the active interpretation, not permanent sidebars.

## AI Interaction

AI appears through contextual actions on the active target and evidence package:
challenge ranking, generate an alternate interpretation, or audit evidence.
Responses become field notes in the work surface. Free-form chat is not the
primary navigation or composition.

The sequence is an interactive workflow rather than decoration. Each step
activates its relevant workspace mode, explains the evidence operation, and
retains a reviewed state after the user moves on.

Every agent request includes the field ID, package SHA-256, active sequence
step, and selected candidate. Agent output must cite service evidence IDs,
distinguish observed, interpreted, derived, estimated, and synthetic data, and
never describe expected paydirt as reserves.

The analysis surface never receives hidden simulator cell truth. A future
opportunity run records the prediction first, then an operator-owned reveal
advances simulated time and publishes synthetic measurements as new evidence.

## Mapping

The browser authenticates Azure Maps through a server-issued Entra token. Wells,
field boundaries, and ranked targets are real WGS84 overlays. The map renders an
explicit offline state when `AZURE_MAPS_CLIENT_ID` is absent; it never falls
back to a fake top-down plot or a browser-visible subscription key.

## Subsurface views

- The 2D section aligns trajectory stations, formation intervals, fluid
  interpretation, and wellbore openings on a positive-down depth axis. Its
  full-depth column uses the selected formation's top/base, with separate fluid
  and rock-quality tracks. Green/solid means both rock cutoffs are met;
  red/horizontal stripes means below cutoff; neutral diagonal hatching means
  missing, invalid, or unclassified inputs. Unknown is neither passing nor failing.
  Below-cutoff rock can still have a known fluid. Adjacent equal classifications
  merge; unsampled intervals remain explicit unknowns. Dotted intervals lie
  outside the selected formation. Inferred contact lines are distinct from
  measured contacts, with text and interval reasons below the plot.
- The 3D Reservoir view places validated survey paths against bounded,
  triangulated gas/oil/water volumes inferred only from visible evidence.
  Finite-radius interpolation and its grid resolution are explicit assumptions,
  not calibrated confidence. Known non-pay constrains geometry; absent logs do not.
  Rotation, tilt, fluid selection, common depth exaggeration, and TVD cutting
  expose the model rather than hiding it behind a fixed projection. Gross
  geometric zone volumes are never presented as reserves.
- Separated qualifying intervals remain separated in the section and geometry.
  Inconsistent survey coordinates and missing covering trajectories are reported,
  not replaced with wellhead locations or guessed coordinate transforms.
- Log tracks align effective porosity, logarithmic permeability, phase
  saturation, interval flow, and monthly liquid production by depth or time.
- The crossplot exposes deterministic cutoffs and phase context rather than
  decorative points.
- Calculated contacts and fluid classes are labeled as model estimates unless a
  source explicitly provides an observed contact.
- Human-readable well names and useful distance/pay context are primary.
  Stable UUID evidence IDs remain available as secondary details.

## Type and Controls

- `"Segoe UI", Aptos, Calibri` for interface copy.
- `Consolas` only for package hashes, step numbers, and instrument readouts.
- Controls use square instrument proportions with 10px radii, not floating
  pills or nested cards.
- Focus uses a high-contrast yellow outline.
- Motion is limited to direct manipulation feedback and the sequence playhead;
  reduced-motion users receive immediate state changes.

## Responsive Rules

The field stage and target readout stack below 900px. Evidence and AI bays become
one column. Below 620px, readouts simplify, evidence counts use two columns, and
the sequence remains horizontally scrollable rather than shrinking labels below
legibility.

## Color Tokens

All component colors are referenced through `--cp-*` variables. The core
instrument tokens are `--cp-machine`, `--cp-machine-deep`,
`--cp-machine-raised`, `--cp-machine-rule`, and the four `--cp-step-*` state
colors.
