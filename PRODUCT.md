# DrillSim

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

The primary user is a petrophysicist or subsurface interpreter investigating a
field during a working analysis session. Exploration managers and technical
demonstration audiences are secondary users.

## Product Purpose

DrillSim assembles drilling, well, trajectory, petrophysical, geological, and
provenance data into a coherent field graph. It helps a user inspect the
evidence, compare exploratory targets, estimate net pay with uncertainty, and
prepare a bounded data package for agent-assisted reasoning.

The workspace must also surface transparent calculations that are not stored
directly—such as reservoir top/base, fluid classes and contacts, qualifying
intervals, and uncertainty—so a petrophysicist can use and challenge them when
choosing where to drill next.

## Positioning

DrillSim keeps observed measurements, human interpretations, deterministic
derivations, model estimates, and synthetic realizations distinct while letting
an agent reason across their relationships. Recommendations remain traceable to
specific wells, curves, intervals, transformations, and source artifacts.

## Operating Context

Users work with field maps, formation tops, well trajectories, log tracks,
crossplots, property distributions, uncertainty ranges, and prospect rankings.
The local development environment is a .NET Aspire application with independent
drilling microservices and persistent SQLite stores.

## Capabilities and Constraints

- Select a field and assemble a versioned analysis package from live services.
- Select a reservoir and include every known intersecting wellbore in its
  evidence package; separately identify the subset with usable petrophysics and
  survey trajectories.
- Explore petrophysical curves, tops, trajectories, provenance, and data gaps.
- Rank candidate drilling locations and report P90/P50/P10 expected net pay.
- Stream agent work and results through AG-UI.
- Make AI a contextual interpretation layer controlled through hypotheses,
  evidence, and selected targets; chat is not the primary experience.
- Use Microsoft Agent Framework with Azure AI Foundry or Azure OpenAI.
- Generate a deterministic hidden geological ground truth conditioned to public
  reservoir evidence, simulate drilling and multiphase production against it,
  and reveal only synthetic observations after a decision is recorded.
- Use Azure Maps for top-down field geography; select canvas or specialized
  visualization libraries only when a concrete subsurface requirement needs one.
- Do not present synthetic values as observations or net pay as reserves.
- Do not require an agent connection to inspect field data.
- The model endpoint and credentials are deployment configuration, never source
  code or browser-exposed secrets.

## Evidence on Hand

- Live DrillSim field, cluster, well, wellbore, trajectory, geological, and
  petrophysics service records.
- Rights-cleared FORCE 2020, KGS, and USGS pilot artifacts under the dataset
  manifest in `datasets/manifest.json`.
- Deterministic benchmark splits and source checksums in ignored staged data.
- No customer claims, production benchmark results, or reserve estimates are
  available and none should be fabricated.

## Product Principles

- Evidence before explanation.
- Uncertainty is part of every recommendation.
- Preserve source lineage and source-time context.
- Human review controls persisted interpretations and drilling decisions.
- Ground-truth cells and future outcomes are isolated from the analysis API,
  browser, and analysis agent; only operator-triggered simulation results may
  advance the observable field.
- Prefer transparent baselines over impressive but ungrounded outputs.

## Accessibility & Inclusion

The analysis surface must support keyboard operation, visible focus, semantic
labels, non-color status cues, reduced motion, and readable dense-data layouts.
