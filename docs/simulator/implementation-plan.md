# DrillSim Two-Stage Simulator Implementation Plan

**Status:** Active  
**Scope:** P0-P10 (P9 adds evidence fidelity; P10 makes the sequencer an actionable workflow)  
**Canonical demo field:** TROLL FORCE-SODIR Field  
**Canonical reservoir:** SOGNEFJORD FM  
**Last updated:** 2026-09-16

## 1. Mission

Build a closed-loop subsurface decision environment in which:

1. A hidden geological and reservoir simulator contains deterministic ground
   truth conditioned to public FORCE-SODIR evidence.
2. The user and analysis agent see only evidence available at the scenario's
   simulated date.
3. The user or agent proposes a drilling opportunity and records a prediction.
4. A human seals and approves that prediction before hidden simulation begins.
5. A separate drilling and observation simulator executes the well against the
   hidden world.
6. Synthetic survey, petrophysical, completion, and production observations are
   published into the existing DrillSim ontology.
7. The prediction is compared with hidden truth and with the noisy observations
   a real interpreter would have received.

The simulator measures decision quality inside a controlled synthetic world. It
does not demonstrate real-world reserves accuracy.

## 2. Architecture and trust boundary

```text
Browser / Analysis Agent
        |
        | public, as-of evidence only
        v
    Analysis API
        ^
        | revealed observations, run status, scorecards
        | authenticated internal callbacks
        |
 Drilling Operations -----------------> Existing DrillSim services
        |                               Field / Cluster / Well / WellBore
        | operator-only                 Trajectory / Architecture / Geology
        |                               Rig / DrillString / DrillingFluid
        v
 Reservoir Simulation
 hidden geological and dynamic truth
```

### 2.1 Stage A: reservoir simulation

Stage A owns:

- the procedurally generated geological property volume;
- fluid contacts and initial phase distribution;
- pressure and saturation state;
- flow schedules, completion connections, and dynamic states;
- truth sampling along an approved path;
- numerical diagnostics and hidden truth used for scoring.

Stage A must never expose:

- cell arrays;
- complete truth surfaces or property volumes;
- undrilled trajectory samples;
- future production outcomes;
- truth-derived opportunity rankings;
- operator credentials.

### 2.2 Stage B: drilling and observation simulation

Stage B owns:

- scenario-to-world binding;
- planned and as-drilled trajectories;
- rig execution and stage checkpoints;
- calls to the restricted Stage A sampler;
- synthetic survey, LWD, wireline, pressure, temperature, cuttings, and
  production observations;
- completion design and reservoir connections;
- publication into existing services;
- reveal manifests, audit records, and scoring.

Stage B is the only application allowed to call Stage A truth routes.

### 2.3 Public analysis boundary

The browser and analysis agent may access:

- scenario clock;
- as-of field package;
- currently revealed evidence;
- prediction drafts;
- sealed prediction after human approval;
- public run stage and progress;
- reveal manifests;
- public production series;
- public scorecards and documented assumptions.

They may not bind worlds, sample truth, execute simulation, publish evidence, or
score before reveal.

## 3. Current foundation

Already implemented:

- TROLL FORCE-SODIR reservoir package:
  - 29 parent wells;
  - 46 Sognefjord-intersecting wellbores;
  - 12 multi-bore well families;
  - 9 FORCE-logged petrophysical and trajectory controls.
- Reservoir-aware expected-paydirt analysis and five ranked candidates.
- React analysis workspace with map, section, 3D fluid geometry, logs, and
  crossplot.
- Microsoft Agent Framework through AG-UI.
- Hidden `reservoir-simulation` Aspire resource.
- Deterministic conditioned structured 3D world generation.
- 64 x 64 x 20 default grid; the Troll conditioning run generates 81,920 cells
  in approximately 1.2 seconds on the current development machine.
- Adaptive immiscible oil/water/free-gas IMPES kernel.
- Matrix-free Jacobi-preconditioned conjugate-gradient pressure solve.
- Gravity, Corey relative permeability, rate-controlled wells, timestep
  adaptation, bounded execution, and material-balance diagnostics.
- Operator-key protection and no browser or analysis-service reference.
- `scripts/seed_reservoir_world.py`.
- SQLite-backed deterministic world regeneration and compressed immutable state
  continuation.
- World model v3 conditioning in the field-local Riemannian frame from 46
  structural intersections, nine property controls, FORCE-derived NTG, and four
  sealed GWC controls.
- Durable scenario clock, as-of evidence filtering, hidden-count summaries, and
  isolated blind-scenario AG-UI tools.
- Immutable prediction, reveal, production-metadata, and aggregate-scorecard
  ledgers.
- Complete S0-S9 drilling workflow with atomic ontology publication and a scored
  five-year demonstration run.
- Versioned deterministic survey, drilling, petrophysical, pressure, temperature,
  cuttings, completion, and metered-production observations.
- Scenario workspace for clock, evidence visibility, proposed paths, reveal
  differences, production checkpoints, assumptions, and aggregate scoring.

Current limitations:

- reveal/scorecard pinning for retained dynamic states is not yet implemented;
- contact initialization uses saturation transition zones without independent capillary phase pressures;
- Stage A path registration currently trusts the internal operator credential;
  a distinct service identity is required before distributed deployment;
- no counterfactual hidden candidate sampling, rank correlation, or ranking
  regret;
- separate reported production readings and meter-error bands are not exposed;
  the workspace uses corrected monthly observations from the immutable clone package;
- browser operator controls are single-user/local only; shared authentication
  and replay/reset controls remain open;
- no Simulator4nDOF coupling;
- no replay calibration harness.

## 4. End-to-end state machine

```text
Draft
  -> Armed
  -> PredictionDrafted
  -> PredictionSealed
  -> HumanApproved
  -> WorldBound
  -> Queued
  -> Drilling
  -> Surveying
  -> Logging
  -> CompletionDesigned
  -> Producing
  -> ReadyToReveal
  -> Revealed
  -> Scored
```

Terminal and recovery states:

- `Cancelled`: no evidence publication and no clock advance.
- `Failed`: stage and diagnostics retained; publication remains unavailable.
- `PublishFailed`: retry publication idempotently from the last complete
  observation batch.
- `Reset`: create a new scenario and world seed; never rewrite the old run.

No transition after `PredictionSealed` may mutate the prediction body or its
hash.

## 5. Identity, time, and provenance rules

### 5.1 Deterministic identity

Use UUIDv5 or SHA-256-derived IDs from:

```text
scenario ID
world model version
observation model version
scoring model version
run ID
source entity ID
simulated valid time
semantic record role
```

Retries must reproduce the same IDs and payload hashes.

### 5.2 Bitemporal time

Every scenario-generated record carries:

- `ValidTimeStart`: simulated time at which the observation becomes available;
- `ValidTimeEnd`: optional simulated supersession time;
- `TransactionTimeStart`: wall-clock persistence time;
- `TransactionTimeEnd`: optional replacement time;
- scenario, run, reveal, and observation-model identifiers.

Historical as-of packages must remain byte-stable after future reveals.

### 5.3 Classification

- Sampled physical truth never enters public services.
- Instrument-like outputs are `Synthetic`.
- Values calculated from synthetic observations are `Derived`.
- Interpreter or agent picks are `HumanInterpreted` or explicitly
  `ModelEstimated`.
- Public FORCE-SODIR records retain their original provenance.

## 6. P0 — Contract and decision lock

**Objective:** Freeze the decisions that every later phase depends on.

**Status:** Complete  
**Effort:** S  
**Depends on:** Current foundation

### Deliverables

- [x] Record the two-stage trust boundary in product and architecture tests.
- [x] Set the conditioned grid default to 64 x 64 x 20.
- [x] Set horizontal correlation lengths to 1,200-1,500 m.
- [x] Confirm scenario isolation by cloning the source field graph.
- [x] Confirm agent drafts and human seals/approves predictions.
- [x] Set the production horizon to five simulated years.
- [x] Set public checkpoints at 1, 3, and 5 years.
- [x] Reuse sequencer colors for Seal, Execute, Reveal, and Score.
- [x] Define `WorldModelVersion`, `ObservationModelVersion`, and
  `ScoringModelVersion`.
- [x] Define the scenario state machine and allowed transitions.
- [x] Add a trust-boundary topology test.

Version contract:

| Contract | Initial version |
|---|---|
| Hidden geological world | `reservoir-hidden-world-v3` |
| Synthetic observation model | `observation-model-v1` |
| Blind scoring model | `scoring-model-v1` |

### Recommended defaults

| Decision | Default |
|---|---|
| Grid | 64 x 64 x 20 structured Cartesian |
| Static field isolation | Clone field per scenario |
| Prediction authority | Agent drafts; human seals and approves |
| Production horizon | Five years |
| Reveal cadence | One explicit reveal; production checkpoints at 1/3/5 years |
| Solver | Adaptive three-phase IMPES |
| First fluid model | Immiscible oil/water/free gas |

### Acceptance gate

- One versioned contract describes every state, identifier, trust boundary, and
  default.
- An automated topology test proves that `analysis-api` and `analysis-web` do
  not receive the Stage A URL or operator key.

## 7. P1 — Complete Stage A

**Objective:** Turn the numerical kernel into persistent, restartable,
path-sampleable hidden truth.

**Status:** Complete  
**Effort:** L  
**Depends on:** P0

### P1.1 Persistent world specification

- [x] Add `ReservoirSimulation.db`.
- [x] Store canonical `WorldSpec` JSON, model version, seed, creation time, sealed
  calibration hash, and truth checksum.
- [ ] Store conditioning evidence IDs and source hashes with the world record.
- [x] Regenerate static worlds on cache miss.
- [x] Verify regenerated world ID and checksum before use.
- [x] Keep the bounded in-memory LRU as a hot cache.

### P1.2 Dynamic state persistence

- [x] Add immutable `SimulationState` records.
- [x] Store parent state ID, run request hash, simulated time, and state checksum.
- [x] Brotli-compress pressure, oil, water, and gas arrays.
- [x] Retain the latest 20 states per bound world.
- [ ] Never evict a state referenced by a published reveal or scorecard.
- [x] Return only opaque `stateId` and safe diagnostics.

Live restart validation regenerated the calibrated 81,920-cell world, restored an
immutable state at 10 simulated seconds, and continued it to a child state at 20
seconds with the parent link intact.

### P1.3 Geological conditioning

- [x] Condition top/base structure from all 46 Sognefjord intervals.
- [x] Condition facies and net-to-gross from FORCE lithofacies controls.
- [x] Condition porosity and log-permeability jointly by facies.
- [x] Use the nine FORCE logs as petrophysical controls.
- [x] Treat the 37 interval-only wellbores as structural constraints.
- [x] Add deterministic property realizations that honor controls.
- [ ] Record every conditioning assumption and source hash.

### P1.4 Fluid equilibrium

- [x] Add explicit GOC, GWC, and OWC surfaces.
- [x] Initialize phase pressure hydrostatically by density.
- [ ] Initialize transition-zone saturation from capillary-height functions.
- [x] Ensure `So + Sw + Sg = 1` in every active cell.
- [x] Version contact assumptions independently from the random seed.

### P1.5 PVT and relative permeability

- [x] Add `datasets/reservoir-fluid-calibration.json`.
- [x] Store density, viscosity, compressibility, Corey endpoints/exponents, and
  contact-transition parameters.
- [x] Mark unsupported field-specific values `ModelEstimated`.
- [x] Record units and provenance for every calibration.
- [x] Keep compositional and solution-gas behavior explicitly disabled.

### P1.6 Well physics and schedules

- [x] Add Peaceman well index.
- [x] Support rate and BHP controls.
- [x] Support automatic rate/BHP constraint switching.
- [x] Support multiple completion connections per wellbore and reservoir.
- [x] Add schedule segments with start time, duration, controls, and state ID.
- [x] Continue a run from an immutable state ID.

### P1.7 Restricted truth sampling

```text
POST /reservoirsimulation/api/worlds/{worldId}/path-bindings
POST /reservoirsimulation/api/worlds/{worldId}/samples
POST /reservoirsimulation/api/worlds/{worldId}/runs
GET  /reservoirsimulation/api/worlds/{worldId}/states/{stateId}
DELETE /reservoirsimulation/api/worlds/{worldId}
```

- [x] Require the operator key.
- [x] Accept only a sealed planned or as-drilled path binding.
- [x] Reject arbitrary grid sweeps.
- [x] Cap points per request.
- [x] Audit path, caller, payload hash, and sampled count.
- [x] Return samples only to Stage B.

### Numerical acceptance gate

- [x] Thiem radial pressure within 2%.
- [x] Buckley-Leverett front within one grid block at 0.3 PV injected.
- [x] Ten-year gravity segregation drifts less than 1% PV per phase.
- [x] One-year equilibrium run changes pressure less than 1%.
- [x] Grid refinement converges monotonically.
- [x] Material-balance error is at most `1e-6` on reference cases.
- [x] 64 x 64 x 20 generation completes within 2 seconds locally.
- [x] Five-year reference run completes within 60 seconds locally.

Independent full-grid gates complete in 21 seconds for the uniform reference
and 2 seconds for the heterogeneous completion-production reference on the
current development machine.

## 8. P2 — Scenario clock and immutable prediction

**Objective:** Make the analysis surface genuinely blind and temporally
reproducible.

**Status:** Complete  
**Effort:** M  
**Depends on:** P0

### P2.1 Analysis persistence

- [x] Add `analysis-api-db` to Aspire.
- [ ] Persist `Scenario`, `PredictionRecord`, `PredictionApproval`,
  `EvidenceVisibility`, `PublicScorecard`, `ProductionSeries`, and
  `IdempotencyRecord`.

### P2.2 Scenario contract

`Scenario` contains:

- scenario ID and source field ID;
- cloned field ID;
- selected reservoir;
- simulated `AsOfUtc`;
- world/observation/scoring model versions;
- seed label without raw truth seed exposure;
- status;
- creation and modification timestamps;
- assumptions hash.

Implemented with an immutable `InitialAsOfUtc` cutoff and a separately advancing
current `AsOfUtc`.

### P2.3 Evidence visibility

- [x] Add `scenarioId` and `asOf` to package and analysis routes.
- [x] Include records whose valid interval contains `asOf`.
- [x] Keep hidden record values out of responses.
- [x] Expose only hidden counts grouped by record kind.
- [x] Preserve current behavior when no scenario is supplied.

### P2.4 Prediction draft and seal

Prediction body:

- candidate and proposed well path;
- formation top/base P90/P50/P10;
- expected-paydirt P90/P50/P10;
- predicted fluid classes and contacts;
- 1/3/5-year oil, gas, and water forecasts;
- uncertainty assumptions;
- cited evidence IDs;
- field package SHA-256;
- rationale.

- [x] Permit optimistic-concurrency-controlled drafts until sealed.
- [x] Canonicalize and hash the sealed body and its baseline bundle.
- [x] Reject mutation after seal with 409.
- [x] Require a separate local human approval action.
- [x] Preserve approval actor label, time, and sealed hash.

Browser saves use the draft revision in `If-Match`. Browser seals now send the
same quoted revision; the service checks it before analysis and inside the SQLite
seal transaction, rejecting stale reviews with 409. Legacy seal callers without
`If-Match` retain their existing behavior.

### P2.5 Baselines

Capture deterministic baselines before Stage B:

- [x] nearest-well;
- [x] field mean;
- [x] current four-neighbor IDW;
- [x] uncertainty-aware rank-1.

The actor header is an audit label for the local simulator, not authenticated
identity. Claims-based identity remains required before multi-user deployment.
Live restart validation preserved the canonical prediction body, seal hash,
baseline bundle hash, approval, and exact T0 package hash.

### Acceptance gate

- [x] A record with `ValidTimeStart > asOf` never appears in package or analysis.
- A sealed prediction remains byte-identical after execution and reveal.
- [x] A no-scenario analysis remains behaviorally unchanged.

## 9. P3 — Drilling Operations core

**Objective:** Add one internal service that owns the execution workflow.

**Status:** Complete  
**Effort:** XL  
**Depends on:** P1, P2

### P3.1 Service and stores

Add `src/DrillingOperations/Service` and focused tests.

Persist:

- `Run`;
- `RunStage`;
- `TruthBinding`;
- `ObservationBatch`;
- `RevealManifest`;
- `AuditEntry`;
- `IdempotencyRecord`.

The service is internal-only and receives:

- Stage A URL and operator key;
- existing service URLs;
- a separate internal key for callbacks to `analysis-api`.

The browser and analysis agent receive neither.

### P3.2 Checkpointed stages

| Stage | Name | Output |
|---|---|---|
| S0 | Bind world | World and source-package binding |
| S1 | Materialize plan | Scenario well, bore, planned trajectory |
| S2 | Execute drilling | Validated planned-path binding, as-drilled path and drilling timeline |
| S3 | Generate survey | Noisy survey stations and covariance |
| S4 | Sample geology | Restricted truth samples |
| S5 | Generate logs | Synthetic instrument observations |
| S6 | Design completion | Openings and reservoir connections |
| S7 | Run production | Dynamic state and metered production |
| S8 | Publish reveal | Idempotent ontology writes |
| S9 | Score | Truth and observation comparisons |

S0 now verifies the human-approved seal, source package, scenario field/reservoir,
world model, and calibration against Analysis API and Stage A before persisting
an immutable binding. S1 persists the approved plan. S2 first registers that
Planned path with Stage A, rejecting incompatible coverage before creating
drilling artifacts; transient Stage A failures pause S2 as a dependency.
It then persists the calibrated kinematic as-drilled path internally.
S3 produces an immutable noisy survey, and S4
registers the as-drilled path with Stage A and stores its restricted truth sample
batch internally. S5 persists versioned observable petrophysics, MDT-like evidence, ROP,
torque/drag/vibration, hydraulics/ECD/losses, temperature, and lagged cuttings. S6
selects/approves log-derived completion openings that Stage A maps to opaque
Peaceman connections, and S7 persists pinned 1/3/5-year states plus 60 noisy
monthly observations. S8 publishes 176 verified ontology records atomically, and
S9 publishes the aggregate dual-basis scorecard.

Reservoir problem responses log field-level validation errors on the request
trace. Planned and as-drilled coverage failures have separate run diagnostic
codes mapped to safe remediation text in the operator facade. Unknown backend
text and private model bounds are not forwarded. Existing failed runs keep
their original diagnostics; no model expansion or historical rewrite is performed.

Each stage stores:

- input hash;
- output hash;
- attempt count;
- status;
- start/end time;
- diagnostics;
- previous audit hash.

### P3.3 Long-running behavior

- [x] Return 202 and `Location`.
- [x] Stream stage-only progress over SSE.
- [x] Support cancellation.
- [x] Allow one active run per scenario.
- [x] Resume from the last complete stage after restart.
- [x] Never publish partial observations.
- [x] Make S8 and S9 independently retryable.

### P3.4 Idempotency and audit

- [x] Require `Idempotency-Key` on creation and action POSTs.
- [x] Store route, key, response status, and body hash.
- [x] Return the original response on retry.
- [x] Add append-only hash-chained audit entries.
- [x] Audit bind, sample, run, cancel, publish, reveal, and score.

### Operator API

```text
POST /drillingoperations/api/scenarios/{scenarioId}/bind
GET  /drillingoperations/api/scenarios/{scenarioId}/binding
POST /drillingoperations/api/runs
GET  /drillingoperations/api/runs/{runId}
GET  /drillingoperations/api/runs/{runId}/events
POST /drillingoperations/api/runs/{runId}/cancel
POST /drillingoperations/api/runs/{runId}/publish
POST /drillingoperations/api/runs/{runId}/score
GET  /drillingoperations/api/audit?scenarioId=
```

### Acceptance gate

- [x] S0-S5 can run, restart, and resume without publishing.
- [x] Repeating S8 produces no duplicate records.
- [x] Cancellation publishes nothing and does not advance the scenario clock.

## 10. P4 — Observation forward models

**Objective:** Convert hidden truth into the imperfect evidence a real operation
would obtain.

**Status:** Complete  
**Effort:** L  
**Depends on:** P1, P3

### P4.1 Survey and trajectory

- [x] Apply instrument-specific bias, scale, misalignment, and random walk.
- [x] Produce covariance and error ellipses.
- [x] Distinguish planned, as-drilled, and definitive trajectories.
- [x] Preserve positive-down and reference-datum conventions.

### P4.2 Drilling observations

- [x] Generate ROP from strength, bit, WOB, RPM, hydraulics, and dysfunction.
- [x] Generate torque, drag, vibration, and stick-slip indicators.
- [x] Generate mud pressure, ECD, return flow, and losses.
- [x] Generate temperature and geothermal observations.
- [x] Generate cuttings with transport lag and depth uncertainty.

### P4.3 Petrophysical logs

Forward-model from truth:

- gamma ray from facies and shale volume;
- bulk density from matrix, porosity, and fluid density;
- neutron response from hydrogen index and gas effect;
- deep resistivity from saturation and Archie assumptions;
- caliper and washout;
- optional sonic and photoelectric factor.

Apply:

- vertical resolution;
- sampling interval;
- tool noise;
- calibration drift;
- bias;
- missing intervals;
- clipping;
- bad-hole quality flags.

The public interpreter must derive PHIE, permeability proxies, saturation,
contacts, and pay from these outputs. Do not publish truth properties directly.

### P4.4 Pressure and contacts

- [x] Generate MDT-like pressure points with gauge uncertainty.
- [x] Generate formation tests with failed or supercharged points.
- [x] Make contacts inferable, not directly copied from truth.
- [x] Publish interpreted contacts separately from measurements.

### P4.5 Reproducibility

Every observation stream derives from:

```text
scenario seed
run ID
instrument ID
observation model version
sample ordinal
```

No unseeded randomness is allowed.

### Acceptance gate

- Published raw logs can be inverted independently to recover porosity and
  water saturation within their stated uncertainty.
- Survey observations fall inside reported 3-sigma ellipses at the expected
  frequency.
- Same seed and inputs produce identical observation-batch hashes.

## 11. P5 — Evidence publication and reveal

**Objective:** Convert a completed hidden run into a coherent observable field
without contaminating source data.

**Effort:** M  
**Depends on:** P2, P3, P4

### P5.1 Scenario field clone

- [x] Clone field, clusters, wells, wellbores, trajectories, architecture, and
  geology references with deterministic scenario IDs.
- [x] Preserve source provenance links.
- [x] Keep source fields immutable.
- [ ] Make reset create a new clone rather than deleting history.

### P5.2 Planned records

Before execution publish only:

- planned cluster/slot;
- planned well and wellbore;
- proposed trajectory;
- prognosed geology;
- proposed architecture.

### P5.3 Revealed records

At S8 publish:

- as-drilled and definitive trajectory;
- survey run and uncertainty;
- synthetic geological properties and log runs;
- derived formation tops, contacts, and pay;
- completion architecture and openings;
- drill string, mud, rig, and simulation references;
- production series when available.

### P5.4 Atomic visibility

Cross-service writes cannot be globally transactional. Use idempotent-forward
publication:

1. [x] write deterministic records;
2. [x] read every record back;
3. [x] verify all UUID edges;
4. [x] create `RevealManifest`;
5. [x] update `EvidenceVisibility`;
6. [x] advance the simulated clock.

If verification fails, the reveal remains hidden and S8 is retryable.

The live reveal retains 29 source wells and 46 source bores, adds one scenario
well/bore and one observable geology record, and publishes 60 corrected monthly
production records. Source T0 remains unchanged.

### Internal callbacks

```text
POST /internal/scenarios/{id}/state
POST /internal/scenarios/{id}/reveal
POST /internal/scenarios/{id}/production
POST /internal/scenarios/{id}/scorecard
```

Require `X-DrillSim-Internal-Key`, distinct from the Stage A operator key.

### Acceptance gate

- Repeating publication returns the same manifest and evidence IDs.
- A failed publication never changes public visibility or scenario time.
- `package?asOf=T0` remains identical after a T1 reveal.

## 12. P6 — Completion and production

**Objective:** Turn the revealed well into a dynamic five-year outcome.

**Effort:** L  
**Depends on:** P1, P3, P4, P5

### P6.1 Completion design

- [x] Select intervals using only synthetic observed logs.
- [ ] Support multiple reservoirs and multiple openings per reservoir.
- [x] Represent perforated, open-hole, and isolated intervals.
- [x] Map each opening to Stage A grid connections.
- [x] Record skin, completion efficiency, and uncertainty.
- [x] Require human approval before production.

### P6.2 Production schedule

- [ ] Start from the post-drilling reservoir state.
- [x] Apply BHP and facility-rate constraints.
- [x] Support shut-in, restart, and control changes.
- [x] Run five simulated years.
- [x] Persist 1-, 3-, and 5-year states.

### P6.3 Metering

Generate oil, gas, and water measurements with:

- meter bias and precision;
- downtime;
- allocation uncertainty;
- missing months;
- test-separator calibration;
- reported and corrected values.

Publish monthly data into `WellDataset.MonthlyProduction`.

### Quality gate

- [x] Simulation completes without timestep underflow.
- [x] Material-balance error meets the numerical threshold.
- [x] Cumulative phase volumes are monotone.
- [x] Rates and pressures remain finite.
- [x] Constraint switches are recorded.
- [x] Water or gas breakthrough can be detected.

## 13. P7 — Blind scoring and baselines

**Objective:** Measure the quality of the opportunity decision.

**Effort:** M  
**Depends on:** P2, P5, P6

### Metrics

| Prediction | Metric |
|---|---|
| Target location | Distance to best hidden candidate |
| Formation top/base | Absolute error in metres |
| Expected paydirt | P50 error and P90/P10 interval coverage |
| Fluid classes | Precision, recall, and weighted F1 |
| GOC/GWC/OWC | Contact-depth error |
| Initial rates | Oil/gas/water relative error |
| Production profile | Monthly RMSE and normalized RMSE |
| Cumulative production | Error at 1/3/5 years |
| Ranking | Spearman correlation |
| Decision quality | Ranking regret |
| Uncertainty | CRPS, PIT, and empirical coverage over replays |

The live scoring-model-v1 scorecard publishes 94 aggregate truth,
revealed-observation, observation-gap, and baseline metrics. Counterfactual
target distance, rank correlation, ranking regret, and calibration coverage
remain explicitly unavailable until multiple candidate paths and at least 20
worlds are sampled.

### Dual basis

Every score is calculated:

1. against hidden truth;
2. against revealed noisy observations.

The difference is the `observationGap`.

### Rules

- [ ] Score all five candidate locations against hidden truth.
- [ ] Ranking regret is the headline metric.
- [ ] Compare AI prediction with every deterministic baseline.
- [ ] Do not report calibration claims for a single run.
- [ ] Require at least 20 independent worlds for PIT/coverage reporting.
- [ ] Record scoring-model version and exact input hashes.
- [ ] Make scorecards unavailable before `Scored`.

### Acceptance gate

- Hand-computable fixture predictions produce exact expected scores.
- Scorecard input hashes match the sealed prediction and truth snapshot.
- No score endpoint reveals raw truth values beyond the declared metric.

## 14. P8 — UX, rig coupling, and production hardening

**Objective:** Complete the operator experience and make the full loop reliable.

**Status:** In progress  
**Effort:** L  
**Depends on:** P1-P7

### P8.1 Analysis workspace

Add:

- [x] scenario selector and simulated clock;
- [x] available, hidden, and revealed evidence counts;
- [x] planned versus as-drilled trajectory;
- [x] prediction editor;
- [x] immutable prediction seal;
- [x] human approval control;
- [x] stage progress and cancellation;
- [x] reveal diff;
- [x] 1/3/5-year production plots;
- [x] truth-versus-observation scorecard;
- [x] assumptions and model-version panel;
- [ ] replay/reset controls.

The Reveal step plots corrected synthetic oil/gas/water volumes from the existing
scenario-scoped, immutable field package, matched to the production receipt by
source-artifact series ID. Monthly and cumulative displays support 1/3/5-year
windows, independent phase scales, exact-value tables, allocation/day counts, and
sealed cumulative forecast comparisons. Missing readings remain gaps; cumulative
totals become unavailable after a missing reading rather than silently assuming
zero. Checkpoints aggregate the first 12/36/60 published calendar months, not
hidden solver states. No new API, simulator output, or chart dependency is needed.

Runnable check: `npm run test:production` from `src\DrillSim.AnalysisWeb`.

The Seal step includes a complete JSON prediction editor and a readable
server-validated ledger. Candidate templates reuse only the visible ranking's
pay quantiles, field-local location, evidence IDs, and package hash; required
depths, fluids, and forecasts are deliberately not invented. Local unfinished
text and its base revision survive navigation/reload in per-scenario tab storage.
Save/review conflicts retain local text, with download and explicit reload
controls. An unchecked confirmation must be selected for the current saved
revision before sealing. Sealing uses the local operator antiforgery boundary
and an exact revision precondition. The browser never receives internal
operator credentials; explicit operator commands use the protected server facade.

Runnable checks: `npm run test:prediction` in `src\DrillSim.AnalysisWeb` and
`dotnet test src\DrillSim.AnalysisApi\Tests\DrillSim.AnalysisApi.Tests.csproj -c Release --filter "FullyQualifiedName~PredictionLedgerTests|FullyQualifiedName~AgentTopologyTests"`.

Additional offline regression covers 334 checks through
`scripts\test_service_contracts.ps1`, 3,200 model/conversion/component tests, two
Field client-contract checks, and eight isolated publication HTTP import/restart
checks. The service runner uses explicit local fixture filters and current
.NET/MCP contracts. The unit-selection component test uses built-in unit systems
and a proper MudBlazor provider instead of a remote development service.
Trajectory and Simulator4nDOF model projects currently contain no test methods;
they do not add coverage. Legacy fixtures targeting fixed live-service ports
remain excluded, not marked as passing. These results do not complete the
separate calibration, leakage, recovery, or live end-to-end gates.

Extend the sequencer:

```text
Prediction -> Simulation -> New evidence -> Evaluation
```

Prediction supports draft save and explicit revision-bound sealing. Simulation
supports separate prediction approval, prerequisite checks, start/cancel/resume,
and reviewed-opening-hash completion approval. Queued/running progress refreshes
automatically through read-only requests and stops at approval pauses. New
evidence explicitly publishes a verified clone, then automatically evaluates it;
Evaluation offers a score retry without republishing an already revealed clone.
Uncertain retries preserve the original request/key, while confirmed failures
require explicit state review before a new attempt.

### Guarded publication recovery

- [x] Recover a reviewed, uncommitted S8 write rejection without replacing its run.

The configured validation run reached S8 with an intact staged plan before a
source-geology import was rejected. Fixing the nullable trajectory contract did
not reopen its terminal failed state. The user approved an explicit guarded
recovery action rather than creating a replacement run.

Recovery must verify the owned run, failure category, completed S0-S7 checkpoints,
immutable staged plan and verified operations, current approved prediction, and
absence of activation/reveal/clock advancement. The operator reviews the exact
manifest fingerprint and supplies an audit label and reason. A protected,
idempotent transaction appends recovery history and makes that same plan eligible
for a separate publication attempt. No data reset, plan reconstruction, automatic
retry, or implicit publication is permitted. Other terminal failures remain
blocked. This gate does not claim general replay/reset or fault-injection coverage.

The implemented review is exposed at
`GET /api/operator/scenarios/{scenarioId}/runs/{runId}/publication-recovery`.
`reviewedPublicationHash` binds the full recovery guard, not only the manifest.
The explicit `recover-publication` POST takes that hash, an actor label and a
correction reason; replay with the same key/body returns the same audited result.
The New evidence workspace shows counts and hashes, invalidates acknowledgement
when the guard or reason changes, and never chains a publish after recovery.
Final regression passed 713 core tests and an isolated desktop/mobile browser
fixture for exact-hash capture, refresh, retry and separate publication controls.
The same live run was recovered and published through those browser controls.
An intervening antiforgery rejection exposed concurrent initial cookie requests;
the client now shares in-flight session initialization without caching token
lifetimes, and the original publication attempt key was reused successfully.

Recovery also detected a previously verified completion architecture missing from
the service. Its 2024 simulation modification date made it eligible for the
legacy 90-day cleaner. With explicit owner approval, only that missing record was
restored through the existing authenticated API from its immutable saved payload;
its original verified business hash matched exactly. Architecture/geology
retirement now excludes all staged, activated and visible publication markers in
the same SQLite transaction as deletion. Ordinary expired unregistered records
still retire. A real service restart preserved the restored staged architecture.

Run `4cd0fd6c-4e59-5de8-9bf7-efd7ab4d371c` retained its original S0-S7
checkpoints and committed the same clone, with 177 records and 60 production
months. Historical seals, snapshots and live source records remained unchanged.
Automatic evaluation initially failed separately at S9. The append-only
correction below completed scoring without repeating publication.

### Append-only scoring correction

- [x] Preserve a rejected immutable scorecard and append a reviewed correction.

The recovered run exposed an absolute-error interval bug: if an actual value
falls inside the forecast interval, zero must be included in the error interval.
Endpoint-only errors omitted zero, so Analysis correctly rejected the saved
scorecard. The calculation is corrected, but overwriting that immutable artifact
would destroy its history.

The user approved an audited correction for this same run. It must retain the
rejected scorecard with its original ID/hash and all original scoring inputs,
create a separately identified/versioned corrected artifact, and link the two.
Only this verified failure with no published evaluation is eligible. A reviewed
correction must not rerun drilling, republish the reveal, or advance the clock.
Publication of the corrected evaluation is a separate explicit, idempotent
operator action. Successfully published historical evaluations remain unchanged.

The actual run now reaches **Scored**. Its rejected scorecard
`c2291c41-3279-5b7e-b8b5-12c8261f8c72` remains byte-for-byte intact, including
all 94 original metrics and inputs. The reviewed correction appends
`b1237f89-cf4a-54bf-8dc7-d25b1533c7d4`, using
`scoring-model-v1-absolute-error-bounds-v2` and explicit original-artifact hashes.
Five metric bounds change; point scores, metric meanings, S0-S8 checkpoints and
the publication receipt do not. Three previously invalid metrics now have valid
enclosing bounds. The correction and evaluation were separate browser actions
with separate keys; all 94 corrected metrics are published. Independent
before/after hashes verify original history, source evidence and seals unchanged.
All seven frontend suites and the production build pass. Twenty-five Aspire
application resources are healthy. Twelve opt-in reservoir cases were not part
of this regression; multi-world calibration and the broader P8 hardening gates
remain open. A final storage test exposed leaked trajectory schema-initialization
connections; those connections now dispose deterministically, and actual chunk
readback/restart plus the complete 332-test Drilling suite pass.

### P8.2 Analysis-agent tools

Implemented read-only tools:

- `get_scenario_clock`;
- `get_asof_field_package`;
- `analyze_asof_field`;
- `get_scenario_scorecard`.

`submit_prediction_draft` remains planned.

The agent cannot seal, approve, bind, execute, reveal, or score.

### P8.3 Simulator4nDOF coupling

- [ ] Convert planned trajectory and completion into existing simulation inputs.
- [ ] Resolve rig, drill string, mud, trajectory, and architecture IDs.
- [ ] Feed truth-derived strength and friction only through Stage B.
- [ ] Run torque/drag and dynamic drilling simulation.
- [ ] Publish synthetic operational measurements.
- [ ] Serialize calls until the existing singleton manager is made concurrency-safe.

### P8.4 Telemetry

Add OpenTelemetry spans for S0-S9 and metrics for:

- run/stage duration;
- accepted/rejected reservoir steps;
- CG iterations;
- balance error;
- sampled truth points;
- generated and published observations;
- idempotent publication retries;
- scoring duration.

No telemetry attribute may contain cell truth, raw operator keys, or hidden
future outcomes.

### P8.5 Restart and fault injection

- [ ] Kill Stage A mid-run.
- [ ] Kill Stage B at every stage boundary.
- [ ] Fail one publication service.
- [ ] Cancel drilling and production.
- [ ] Corrupt a persisted state checksum.
- [ ] Retry reveal and scoring.
- [ ] Verify no partial visibility and no clock advance.

### P8.6 Performance

| Operation | Initial budget |
|---|---:|
| Generate 64 x 64 x 20 world | 2 seconds |
| Sample 5,000 approved-path points | 200 ms |
| Five-year production | 60 seconds |
| Full S0-S9 run | 5 minutes |
| Assemble as-of package | 3 seconds |

Upgrade only when measured:

- parallelize matrix-vector multiplication when production exceeds 60 seconds;
- add AMG preconditioning when a run exceeds 5 minutes;
- replace package N+1 reads when assembly exceeds 3 seconds;
- add a queue when more than five runs execute concurrently.

### Final acceptance gate

The end-to-end test must prove:

```text
arm scenario at T0
capture exact T0 evidence hash
seal and approve prediction
bind hidden world
execute drilling and observation generation
publish reveal at T1
run production
score AI and baselines
assert prediction bytes/hash unchanged
assert package(asOf=T0) still has the original evidence hash
assert package(asOf=T1) contains the deterministic reveal
assert no public response, bundle, log, or trace contains hidden cell truth
```

## P9 — Evidence fidelity and generality

**Status:** In progress  
**Depends on:** P2, P4; complements rather than waits for P8.

The original percentage counted implementation tasks, not scientific readiness.
These additional tasks make previously untracked limitations explicit. Existing
P5-P8 tasks remain open; they are not duplicated or silently redefined.

### Delivery order

1. **Interpretation and geometry:** use downhole interval positions, retain
   separated zones, and distinguish unknown logs from observed non-pay. Replace
   averaged ellipses with bounded, triangulated fluid volumes and inspection
   controls. Fit only visible evidence, never Stage A truth.
2. **Generality and publication:** replace canonical well-count assumptions with
   graph-driven invariants, then complete existing P5 planned/survey/opening/
   operational-context publication and P6 multi-reservoir work.
3. **Human operation:** complete existing P8 approval, progress/cancellation,
   replay/reset, and agent drafting. Evolve the JSON editor into guided field
   authoring without inventing missing physical estimates.
4. **Scientific acceptance:** establish realistic rate/PVT/schedule calibration,
   finish candidate-level P7 scoring, and run the existing P8 multi-world,
   recovery, leakage, and end-to-end acceptance gates.

### Tracked additions

- [x] P9.1 Resolve and validate downhole positions; select definitive trajectories deterministically.
- [x] P9.2 Preserve separated fluid intervals and known non-pay; missing observations remain unknown.
- [x] P9.3 Replace ellipse envelopes with bounded triangulated fluid volumes.
- [x] P9.4 Add rotation, common depth exaggeration, fluid selection, and depth cutting.
- [ ] P9.5 Calibrate uncertainty envelopes and held-out geometry accuracy; no heuristic confidence percentages.
- [ ] P9.6 Resolve common vertical datums and incorporate structural-only controls without fabricated trajectories.
- [ ] P9.7 Remove canonical 29/46-to-30/47 publication-count assumptions.
- [ ] P9.8 Establish physically defensible production-rate, PVT, and schedule calibration.
- [x] P9.9 Add guided prediction authoring alongside the complete JSON contract.
- [ ] P9.10 Share a versioned interpretation model across logs, sections, geometry, and ranking.

### First geometry slice: acceptance and limits

The initial observable-only method uses finite-radius interpolation and a bounded
grid, with triangulated isosurfaces instead of ellipse fitting. It must preserve
multiple separated zones, show insufficient support explicitly, retain valid
negative controls, and expose its distance/resolution assumptions. It must not
label geometric volume as reserves or support counts as probability.

Coordinate-inconsistent published paths are excluded with a diagnostic, not
silently translated. MD intervals require a covering survey; structural-only
bores remain listed as unavailable to this first mesh until P9.6 supplies a
defensible common-datum/position model. Fine layers below the grid resolution,
fault discontinuities, calibrated confidence bounds, and correlation of repeated
formation occurrences remain explicit limits rather than invented detail.

Implemented as `visible-interval-mesh-v1` in the **3D Reservoir** view. A
24×24×32 stratigraphic grid uses finite-radius inverse-distance phase indicators
and marching tetrahedra. Each fluid requires three independent, non-collinear
positive controls; known non-pay and other phases reduce its indicator. Missing
values, invalid SI units, or conflicting coincident controls supply no negative
evidence. The 0.5 surface threshold is not a probability.

The initial search-radius assumption is 10 km, adjustable to 3/6/10 km. The
canonical field has insufficient positive support at 6 km under this conservative
rule; this sensitivity is not hidden. Nine logged/surveyed controls contribute;
37 source bores without covering surveys are excluded. The revealed demo's new
survey disagrees with its inherited tie-in and is also excluded with a diagnostic.
That historical observation remains unchanged. New S8 plans now use
`absolute-riemannian-collar-v2`: field-local coordinates are projected to the
field's absolute Riemannian axes, the tie-in matches the first definitive station,
and a dedicated single-well cluster anchors the new collar without copied
template locations. Unsupported latitude/longitude and vertical collar elevation
are not invented. Persisted plans retain their original storage format and replay
without reconstruction. All 27 coordinate/collar tests and the 245-test Drilling
suite pass. A new live configured run verified the actual imported trajectories,
tie-ins and collar after S7, with all historical hashes unchanged. Its first S8
attempt exposed a separate import guard that incorrectly required a trajectory
for 37 structural-only source geology records. Controller and storage validation
now accept an absent trajectory while rejecting empty IDs and missing wellbores;
six isolated geology-import tests cover preservation and receipt gating.
The same run has now published its original clone: all 177 records, including 37
structural-only geology records, pass snapshot readback. Corrected trajectories,
tie-ins and the dedicated collar are present, along with 60 production months.
The final corrected 94-metric evaluation is now published; the same run is Scored.
Common-datum generalization remains P9.6. No immutable source/reveal records were
rewritten.

The view reports full-mesh gross geometric volume, not reserves, and clips away
geometry above the selected TVD without recomputing a misleading sliced total.
Paths and meshes share the same vertical exaggeration; the old separate 25×
trajectory and 8× body-thickness distortions and heuristic confidence percentages
are removed. `npm run test:reservoir` covers closed surfaces, known-negative vs
unknown controls, interval separation, units, downhole interpolation, deterministic
survey selection, and depth cutting.

The 2D fluid column now separates fluid classification from rock-quality
eligibility. It covers the selected formation top-to-base with explicit unknown
and outside-formation intervals, merging equal neighboring classes at sample
midpoints without filling large data gaps. Green/solid quality meets both
cutoffs, red/horizontal stripes is below cutoff, and neutral diagonal hatching
is missing/invalid/unclassified data. Below-cutoff rock is not empty rock.
Contact labels and the depth/reason ledger sit below the plot to avoid competing
with thin bands. This display refinement does not change ranked pay estimates
or the qualifying-volume cutoff rules.

## P10 — Actionable hypothesis workflow

**Status:** Implemented and live-validated  
**Approved:** 2026-09-11  
**Interaction:** flexible task navigation; prerequisites gate consequential
actions, not every analytical step.

The original sequencer mostly switched visualization modes and marked visited
steps as reviewed. This phase replaces those misleading affordances with
task-specific inputs, actions, outputs, and readiness reasons. Clicking navigation
never runs paid inference or mutates a prediction/simulation/reveal.

### Approved mapping of the original 20 positions

| Original position | Task workspace | Intended action/output |
|---|---|---|
| 1 Ingest | Evidence | Load/refresh scoped inventory, provenance, coverage and package identity; no implied browser import pipeline |
| 2 Normalize | Units & QC | Inspect units, depth references, null flags and transformations; acknowledge actual warnings |
| 3 Position | Position | Inspect linked coordinate/datum/survey issues and focus their controls |
| 4 Correlate | Correlation | Compare multiple bore intervals and record a control/formation interpretation |
| 5 Cutoffs | Rock & pay criteria | Apply validated criteria to a versioned analysis, not cosmetic sliders |
| 6 Net pay | Pay intervals | Inspect contributions, excluded/unknown intervals and per-bore calculations |
| 7 Grid | Search area | Display full bounded grid, candidate count and supported resolution controls |
| 8 Exclude | Exclusions | Inspect excluded points, screening buffers and reasons; apply supported changes |
| 9 Interpolate | Reservoir model | Inspect mesh/control support; distinguish geometry/view settings from ranking settings |
| 10 Uncertainty | Uncertainty | Inspect quantiles, disagreement, distance effects and sensitivity without false calibration claims |
| 11 Rank | Targets | Compare/select ranked targets and inspect score components |
| 12 Challenge | Challenge | Explicit manual/AI critique with evidence references; save outcomes rather than mark navigation complete |
| 13 Alternate A | Alternatives | Create named hypothesis/configuration branches and recalculate |
| 14 Alternate B | Alternatives (merged) | Conservative/spatial presets in the same workspace; no arbitrary two-alternative limit |
| 15 Compare | Compare hypotheses | Compare saved revisions and record the preferred interpretation |
| 16 Package | Evidence bundle | Download exact evidence, settings, selected target and rationale, not simulator publication |
| 17 Seal | Prediction | Save, review and explicitly seal the intended revision |
| 18 Execute | Simulation | Human approval, preflight, start/cancel/resume, completion pause and safe live progress |
| 19 Reveal | New evidence | Explicit verified publication/clock advance; then inspect clone diff and production |
| 20 Score | Evaluation | Automatic idempotent evaluation after reveal, or explicit authorized retry; inspect aggregate metrics |

Stable task IDs replace numeric-index routing. Map, Section, Reservoir, Logs and
Crossplot remain lenses within tasks. "Visited" never means reviewed or complete.
Changing analytical inputs must not relabel stale outputs or historical seals;
camera-only changes do not invalidate calculations.

### Tracked additions

- [x] P10.1 Implement grouped stable task navigation and merge duplicate Alternatives slots.
- [x] P10.2 Derive honest scoped visited/readiness/freshness states; remove automatic review claims.
- [x] P10.3 Add the evidence inventory and provenance workspace.
- [x] P10.4 Add the units, references and missingness/QC workspace.
- [x] P10.5 Link position diagnostics to the selected well and map.
- [x] P10.6 Add multi-bore correlation and explicit control interpretation.
- [x] P10.7 Add validated, fingerprinted analysis configuration and real recalculation.
- [x] P10.8 Expose per-bore pay contributions and the actual screening method.
- [x] P10.9 Expose the full bounded search grid and resolution controls.
- [x] P10.10 Expose excluded candidate decisions, reasons and screening-radius controls.
- [x] P10.11 Expose quantitative uncertainty components and honest calibration limits.
- [x] P10.12 Add target comparison/selection and score explanation.
- [x] P10.13 Persist evidence-linked challenge results and human dispositions.
- [x] P10.14 Persist named hypotheses/configuration revisions.
- [x] P10.15 Compare saved hypotheses and record the preferred decision.
- [x] P10.16 Export an exact evidence/configuration/result bundle.
- [x] P10.17 Bind configured predictions, baselines and execution to the same analysis; preserve legacy hashes.
- [x] P10.18 Draft a formation interpretation with AI from the displayed evidence, then review and accept it into the hypothesis.

### Formation interpretation assistant

Correlation introduces the first guided AI action: **Draft interpretation with
AI**. It proposes a name, target rationale and formation notes from the selected
field/reservoir, applied analysis and current engineer notes. A saved hypothesis
uses its pinned evidence. The server verifies the requested package, result,
configuration and revision before inference; client-supplied raw evidence is not
trusted.

The engineer reviews the draft, supporting references and limitations before
choosing **Use this draft**. Accepting fills the local form and retains citations;
it does not save, change calculations or start simulation. Existing text requires
review before replacement, an existing name is kept, and stale/malformed/cancelled
results cannot replace notes. **Save hypothesis** remains a separate step.
The existing configured Azure OpenAI connection and Agent Framework are reused.

Implementation and live validation are complete. The targeted formation suite
passes 101 cases. Separate fake-model checks cover
both full Troll packages (46 and 47 wellbores), retaining every formation and log
curve summary in the prepared server-side snapshot. Initial briefs are about
2 KiB; the full prepared snapshot retains its 192 KiB safety bound. Repeated metadata is compacted;
quality-rejected samples are excluded from valid ranges. The browser checks cover
review before replacement, cancellation, changed context, citations and HTTP 429
without automatic retry. Existing field-note agents retain their retry settings.

On 2026-09-16, the original full-context `gpt-6-astra` request produced a verified draft with 16
citations. The browser displayed the review step, preserved existing notes,
required review before replacement, and made no hypothesis or simulator writes.
The model reported 49,403 input tokens and 3,572 output tokens against a requested
4,000-token output limit.

The adaptive version starts with a small brief and exposes six read-only tools:
whole-formation depth/coverage summaries, exact applied screening, evidence
search, paged inspection, engineer notes, and the full prepared context. The agent
chooses how far to expand; broad execution safeguards do not force a small-context
answer. Tools use the same verified snapshot, and citations are checked against
evidence actually returned. No tool can change settings, save data, or fetch
unrelated or hidden evidence.

A live adaptive run used 13 tools over five model requests and returned a valid
12-citation draft without saving anything. Initial input was 1,888 tokens and the
largest model context was 18,610 tokens. Cumulative input was 53,172 tokens,
including 34,550 cache hits; output was 2,419 tokens. This reduces fresh input and
peak context, but is not a reduction in total input tokens for that run. Both the
targeted and full-context paths are covered by native function-invocation tests.

Formation drafting and both AG-UI field-note agents use the Responses API so
reasoning and function tools can work together. All three use the same stateless
transport options: response storage and background execution are disabled, and
encrypted reasoning continuation stays in the local request. The scenario agent
retains its clock-bound tools rather than the unrestricted field tools.
Formation drafting's Development telemetry captures agent, model-request and tool spans, including
messages actually exchanged. Message capture is disabled outside Development. Aspire retains ownership
of OTLP endpoint/exporter configuration. Provider exceptions remain available in
structured logs, while browser errors stay sanitized.

Existing P8 approval/progress/replay/agent tasks and P9 guided authoring/shared
interpretation tasks remain their own acceptance gates; they are not counted twice
here. An unavailable action with a correct explanation is useful navigation but
does not complete the corresponding backend feature.

The first configured-analysis path is live: strict
`POST /api/fields/{fieldId}/analysis` returns the effective six-field configuration,
configuration/result fingerprints, the full classified grid, and score/uncertainty
components. Default GET calculations and legacy seals remain compatible. The full
grid and exclusion decisions are currently tabular; the map still shows the
shortlist. Screening distances cover located screening controls, not every
structural-only bore, and are not an anti-collision safety assessment.

The configured handoff backend now exposes
`GET /api/analysis/prediction-capabilities` and validates
`prediction-analysis-binding-v1` against the frozen scenario source package and
initial clock. The binding commits the configuration/result fingerprints,
candidate coordinates and pay quantiles. Its point-screening geometry requires
every planned station to retain that candidate's easting/northing; directional
or path-integrated forecasting is not claimed or silently flattened.

Configured baseline families use version 3 and
`baseline-analysis-binding-v1`. Nearest well/field mean use configured summaries;
the four-neighbor baseline independently uses exactly four neighbors even if the
main analysis chooses a different count. At least four located controls are
required. Legacy absent/null bindings retain their exact bytes, hashes and
version-2 baselines; old unversioned bound drafts require explicit rebuilding.
The capability-aware editor and live simulator handoff now pass end-to-end.
A real nondefault configuration (porosity 0.13, three interpolation neighbors)
was sealed, approved, executed, revealed and evaluated, while the independent
four-neighbor baseline retained four controls. A separate UI acceptance exercised
a non-shortlist eligible target with a 9-by-9 grid and verified immutable sealing
without automatic approval. There is no substitution of default settings.

The first UI acceptance passed for all 19 task workspaces: navigation performs
no mutations or inference, the map remains mounted, settings affect actual
calculations only after Apply, and the evidence bundle contains the exact scope,
package, configuration, result and selected target. The sequence heading remains
stationary while the task rail scrolls on desktop and mobile.

The saved-hypothesis backend now persists immutable revisions and evidence
snapshots, branches, evidence-linked challenges with versioned dispositions, and
deterministic comparisons. Its 31 focused persistence tests pass; the expanded
Analysis API suite now passes 268 tests including configured handoff.
Correlation/Alternatives/Challenge/Compare are now connected to these APIs.
Parent acceptance persisted a real named baseline and annotated revision,
reanalyzed a conservative branch against the saved package, compared exact
revisions, and saved a cited challenge with a versioned disposition. A preference
decision is recorded in revision rationale, not a separate preferred flag.
Earlier snapshots remained unchanged. An isolated browser pass additionally
verified stale-edit recovery, retained local text, pinned comparison references
and mobile layout. Correlation/control notes remain annotations, not solver
inputs or invented formation ties. AI critique remains an explicit live-context
aid requiring reviewed attachment to a specific saved revision.

### Local operator decision

The user selected a single-user local operator workflow for this stage, rather
than requiring Entra now. Browser operator commands require a local-only,
antiforgery-protected server facade and explicit human intent. Actor names are
audit labels, not authenticated identities. No Drilling Operations key, Stage A
URL/key, world identifier, connection metadata or raw truth may be returned to
the browser or analysis agent. Unconfigured/unbound prerequisites fail closed
with a specific reason. Shared deployment still requires proper authentication.
Live checks cover origin/antiforgery rejection, retired approval bypasses, stale
sealing, safe progress/completion projections, and unchanged historical seals.
An isolated browser fixture also verifies that an unchanged completion hash
retains acknowledgement, changed openings invalidate it, and approval submits
the exact reviewed hash. These checks did not approve the historical scored run.

### Guided Chapter 4 live acceptance

On 2026-09-16, all 39 steps in tutorial Chapters 1-4 passed against the running
application. The screenshot-backed record is in
[`docs/index.html`](../index.html#tutorial). Chapters 1-3 exercised
the real evidence, sensitivity, AI drafting, saved alternatives, cited review,
comparison and export paths.

Chapter 4 created scenario `b50182fd-fbba-8b08-aed7-7fd84c6fa3c0` through the GUI,
authored the editable demonstration forecast, saved and sealed it, and separately
approved its exact seal. Model preparation used a scoped profile, Preview
16x16x8 resolution and seed 12345; Drilling Operations generated and bound the
model privately without exposing world identities or conditioning controls.
The run `41fbbd11-923c-540f-8069-b44ca4de664a` then passed drilling, survey/log
generation, explicit completion review/approval, production, publication and
evaluation. Its final state is Scored, with 60 production months and 94 metrics.
All monthly/cumulative 1/3/5-year views were exercised.

The guided forecast fills blank inputs with clearly labeled demonstration
assumptions and keeps the advanced JSON contract available. It never flattens
directional edits or infers missing physical estimates. Save, Seal, Approve,
Prepare, Start, completion approval and publication remain separate actions.
The approval controls refresh when the seal changes, even if the draft timestamp
does not. Completed runs display results rather than a blocked start preflight or
disabled publication controls.

Original field records and both earlier scored scenarios matched their pre-run
baselines afterward. The earlier sealed tutorial scenario was left unchanged.
This completes the guided point-screening authoring slice, not scientific
calibration, general directional forecasting, or the remaining P8 fault,
leakage and multi-world acceptance gates.

### Implementation streams

1. Frontend task workspaces and existing output/action placement.
2. Shared configurable analysis, grid/reason/component output and prediction compatibility.
3. Isolated local-operator API modules; integration occurs only after contract checks.
4. Parent integration, live validation and work-bible synchronization.

Task workspaces, persisted interpretation/review, configured prediction
preparation and live handoff acceptance are complete.
Replay controls and broader operational hardening remain separate P8 work.
The work bible records 141 of 163 tasks complete (P8 13/21, P9 5/10, P10 18/18).
Completed task counts are not an estimate of effort or scientific readiness.

## 15. Delivery sequence

```text
P0 Contract lock
 ├── P1 Stage A completion ────────────────┐
 └── P2 Scenario clock and prediction ─────┤
                                           v
                                  P3 Drilling Operations
                                           |
                                  P4 Observation models
                                           |
                                  P5 Publish and reveal
                                      /          \
                                     v            v
                          P6 Completion/production P7 Partial scoring
                                     \            /
                                      v          v
                                      P7 Full scoring
                                           |
                                      P8 UX/hardening
```

### Demo A — drilling reveal

Requires P0-P5, partial P7, and the scenario UX from P8.

Demonstrates:

- blind candidate selection;
- immutable prediction;
- drilling and synthetic LWD reveal;
- top/base, fluid, contact, pay, and ranking-regret scoring.

Production is not required.

### Demo B — production reveal

Adds P6 and full P7.

Demonstrates:

- completion selection;
- five-year multiphase production;
- water/gas breakthrough;
- predicted versus simulated monthly and cumulative production;
- comparison against deterministic baselines.

## 16. Testing matrix

### Unit and property checks

- deterministic IDs and random streams;
- conditioning honors controls;
- saturation closure and bounds;
- monotonic Corey curves;
- Peaceman well index;
- survey covariance coverage;
- log forward-model inversion;
- completion-to-cell mapping;
- scoring formulas.

### Numerical references

- Thiem radial flow;
- Buckley-Leverett displacement;
- hydrostatic equilibrium;
- gravity segregation;
- grid convergence;
- material balance.

### Contract and integration

- Stage A authorization;
- internal callback authorization;
- as-of filtering;
- scenario clone integrity;
- Simulator4nDOF round-trip;
- idempotent publication;
- cancellation;
- restart recovery.

### Leakage

- no Stage A URL/key in analysis configuration;
- no truth fields in public response schemas;
- no truth values in frontend bundles;
- no raw truth in telemetry;
- no unrestricted sample requests;
- no scorecard before reveal;
- no post-cutoff records in T0 packages.

### Replay calibration

Run at least 20 world seeds and report:

- empirical P90/P10 coverage;
- PIT/rank histogram;
- CRPS;
- ranking-regret distribution;
- baseline comparisons;
- observation-gap distribution.

## 17. Explicit deferrals

| Deferred capability | Upgrade trigger |
|---|---|
| Solution gas and compositional EOS | Condensate dropout, miscibility, injection, or surface GOR becomes a graded outcome |
| Corner-point/unstructured grid | Fault geometry or local grid refinement materially changes a target |
| Two-way geosteering | Trajectory execution becomes part of the scored decision |
| Parallel/AMG solver | A five-year run exceeds its budget |
| Entra and multi-tenant isolation | More than one real user or shared deployment |
| Distributed transaction coordinator | Idempotent-forward publication fails in practice |
| External MCP for scenario operations | A non-product agent host needs the same closed allowlist |

## 18. Risks and mitigations

| Risk | Mitigation |
|---|---|
| Hidden world too similar to public IDW | Increase grid resolution and use multi-scale conditioned heterogeneity |
| Contact geometry incoherent | Initialize from explicit contact surfaces and capillary transition zones |
| Synthetic logs too clean | Publish versioned instrument bias, resolution, missingness, and QC |
| Synthetic logs impossible to interpret | Round-trip test the observation and inversion workflows |
| Future evidence leaks through package assembly | Central scenario/as-of visibility filter and leakage tests |
| Scorecard rewards lucky point predictions | Require uncertainty ranges and rank all five candidates |
| Published scenario contaminates source data | Clone the field graph per scenario |
| Numerical solver becomes the bottleneck | Enforce budgets; optimize only after measurement |
| Restart changes truth | Persist canonical specs/checksums and dynamic state blobs |
| Single run presented as calibration | Suppress calibration claims below 20 replays |

## 19. Definition of done

The two-stage simulator is fully functional when:

- a scenario can be armed from the Troll-Sognefjord package;
- the hidden world survives restart;
- the public T0 package is immutable;
- an agent can draft but not seal a prediction;
- a human can seal and approve it;
- Stage B can drill the approved trajectory;
- truth is sampled only along that path;
- realistic noisy observations are produced;
- completion openings are selected from observed logs;
- five-year oil/gas/water production runs successfully;
- reveal publication is idempotent;
- simulated time advances only after complete publication;
- AI and baselines receive a dual-basis scorecard;
- ranking regret is reported;
- replay calibration requires at least 20 worlds;
- no public API, browser bundle, log, or trace exposes hidden cell truth.
