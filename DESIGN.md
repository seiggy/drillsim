# DrillSim Design

## Visual World

The analysis portal is a scientific instrument surface: charcoal and
calibrated, not a dashboard plus chatbot. Geological reasoning is expressed as
five plain questions answered in order, not as a long sequence of tasks.

The step colors are state laws and always appear with text labels and numbers.
Yellow marks the current step, focus and best estimates; orange marks AI
panels; red marks failures and High risks; white is source evidence. Colors
never carry meaning alone.

## Composition

- A single header holds the **Field**, **Reservoir** and **Run** selectors and
  the **Tour**, **Demo guide** and **Settings** links. Field and reservoir are
  separate selectors; the Run menu lists earlier runs with a plain status.
- Below it, a five-step bar: **1 Field** (What do we know?), **2 Prospects**
  (Where should we drill?), **3 Forecast** (What will we find?), **4 Drill**
  (Drill it in the simulator) and **5 Results** (How did we do?). Each step
  shows its number, label and question; steps that need a run say *Needs a
  target*. The row never wraps; narrow screens scroll it horizontally.
- Each screen opens with a heading, one explanatory sentence and one primary
  action that leads to the next step. Secondary detail sits in collapsed
  sections, not in extra screens.
- Field and Prospects share one synchronized analysis workspace with Map, 2D
  Section, 3D Reservoir, Logs and Crossplot modes. Top-down geography uses
  Azure Maps; custom SVG/canvas plotting is reserved for subsurface and
  petrophysical views. Existing wells and ranked targets share one surface.
- Prospects shows at most five drillable targets as cards with a low / best /
  high range bar. Only targets inside the simulator's area are shown, so the
  UI cannot lead a user to an undrillable well.
- Forecast is a plain form (depth ranges, total depth, fluids, production)
  beside its AI panel. Drill is a ten-stage progress track. Results lead with
  three headline numbers and a rules-of-thumb table; production and the full
  metric list are collapsed appendices.
- **Settings** is a separate page holding every adjustable value (rock
  cutoffs, target search, simulator detail, completion approval, operator name,
  run filtering), each with a one-line plain-language explanation.
- **Tour** is a side panel with *purpose*, *do this* and *look for* for the
  current step. It navigates only; it never clicks, saves or drills.

## AI Interaction

AI appears as inline panels on the screen it serves, plus one optional chat:

- **AI second opinion** (Prospects) explains why the selected target ranks well
  and what could make it wrong, with a verdict, rated risks and checks.
- **AI forecast** (Forecast) drafts depths, fluids and production into the form.
  **Fill from reservoir model** sits beside it as a non-AI fallback.
- **Ask DrillSim** (header button) opens a right-hand chat panel, hidden by
  default and closed with Escape. It answers from what the current screen shows,
  states what it "Sees", offers screen-specific starter questions and shows each
  lookup it makes as a quiet line (repeated lookups are collapsed). It can show
    pipe tables and draw bar, line or scatter charts inline (with a **Show data**
    table); model output is never rendered as HTML. It can only
    read; it cannot change settings, forecasts or runs.

Each panel states what the AI does, shows elapsed time with a typical duration
and **Cancel**, and explains failures (including capacity limits) in plain
language with **Try again**. Every point names the wells it is based on;
evidence IDs stay out of the prose. The AI never changes ranking numbers and
never saves, locks, approves or drills: the user edits the form and presses
**Lock forecast**. Expected net pay is never described as reserves.

The analysis surface never receives hidden simulator cell truth. The forecast
is locked first; the simulator then drills, publishes the new well's data and
returns only aggregate scores.

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
- The 3D Reservoir view draws the server-side static reservoir model
  (`POST /api/fields/{id}/reservoir-model`, `contact-static-model-v1`): top and
  base surfaces interpolated through every located formation pick, proportional
  ~5 m layers whose net-to-gross, porosity and permeability come from the nearest
  logged wells, and flat field-wide GOC/OWC (or GWC) contacts fitted to the log
  fluid readings in good rock. Wells, the proposed target and the model share one
  coordinate frame. The top surface is shaded by hydrocarbon column; block walls
  show gas, oil and water bands. Prospects and Forecast read the same model at
  each target (`/reservoir-model/columns`), and the forecast agent calls it as
  `read_reservoir_model_at_target`. Rock volumes are never presented as reserves.
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
- `Consolas` only for package hashes and instrument readouts.
- Controls use square instrument proportions with 10px radii, not floating
  pills or nested cards.
- Focus uses a high-contrast yellow outline.
- Motion is limited to direct manipulation feedback and progress spinners;
  reduced-motion users receive immediate state changes.

## Responsive Rules

Below 1100px, split layouts (workspace and side panel, target list and map,
form and AI panel) stack into one column. Below 640px, header selectors take
the full width and depth inputs stack. The step bar remains horizontally
scrollable rather than shrinking labels below legibility.

## Color Tokens

All component colors are referenced through `--cp-*` variables. The core
instrument tokens are `--cp-machine`, `--cp-machine-deep`,
`--cp-machine-raised`, `--cp-machine-rule`, and the four `--cp-step-*` state
colors.
