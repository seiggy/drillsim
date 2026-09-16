# DrillSim

DrillSim is an independent .NET 10 and Aspire monorepo built from the
MIT-licensed drilling microservices originally published by the
[Open Source Drilling Community (OSDC)](https://github.com/Open-Source-Drilling-Community).
It brings the services and Cluster web application into one solution that can
be built, launched, observed, and managed together.

> [!IMPORTANT]
> This repository is maintained independently by Zachary Way. It is not
> maintained by, affiliated with, or endorsed by OSDC.

## What is included

- 20 drilling service APIs, a .NET analysis API, a React interpretation
  workstation, and the Cluster Blazor web application
- .NET Aspire orchestration, service discovery, health checks, and telemetry
- A persistent Aspire-managed SQLite database for each service
- OpenAPI documents with Scalar API reference pages
- Model Context Protocol (MCP) endpoints

## Prerequisites

- [.NET SDK 10.0.400](https://dotnet.microsoft.com/download/dotnet/10.0)
  or a compatible .NET 10 feature-band SDK
- [Aspire CLI 13.5.3](https://aspire.dev/get-started/install-cli/) or newer

## Get started

```powershell
git clone git@github.com:seiggy/drillsim.git
Set-Location .\drillsim

dotnet restore .\DrillSim.slnx
dotnet build .\DrillSim.slnx --no-restore
aspire start
```

Open the Aspire dashboard URL printed by `aspire start`. The dashboard provides
the dynamically assigned endpoints for the Cluster UI, service APIs, logs,
traces, health status, and SQLite resources.

Stop the application with:

```powershell
aspire stop
```

SQLite files are persisted under `data\` and are excluded from source control.

## Validation

Use Release outputs while Aspire runs Debug resources:

```powershell
dotnet test .\DrillSim.slnx -c Release --nologo
.\scripts\test_service_contracts.ps1
```

The solution includes the Analysis API, Drilling Operations, and Reservoir
Simulation test projects; it does not include every legacy test project.
The second command runs twelve additional self-contained service-contract
projects, including in-process Earth-service HTTP/MCP tests, isolated controller
tests, and explicitly selected local fixtures from mixed test projects. Their
test frameworks and MCP client contracts match the .NET 10 services.

Legacy REST/MCP fixtures hardcoded to `localhost:5001` or `localhost:8080` are
not run by this command: some mutate their target service. Do not point them at
the seeded application or an unrelated local app. Model and unit-conversion
library tests are separate from both commands. Passing these suites does not
establish calibration accuracy or complete live distributed-system coverage.

## Service endpoints

Ports are assigned at launch. Open a service from the Aspire dashboard, then
use these paths:

| Path | Purpose |
| --- | --- |
| `/swagger` | Scalar API reference |
| `/swagger/v1/swagger.json` | Generated OpenAPI document |
| `/health` | Readiness health check |
| `/alive` | Liveness health check |
| `/mcp` | MCP HTTP transport, where implemented |
| `/mcp/ws` | MCP WebSocket compatibility transport, where implemented |

The AppHost advertises implemented public HTTP MCP endpoints at their service
base paths. Discover and exercise them through Aspire:

```powershell
aspire mcp tools --non-interactive
aspire mcp call field ping --input '{}' --non-interactive
```

Geological Properties, Trajectory, Drilling Fluid, Geothermal Properties, and
YPL Calibration currently use REST rather than HTTP MCP. Some other services
advertise connectivity-only tools; discovery does not imply complete CRUD tool
coverage. Hidden Reservoir Simulation and internal Drilling Operations are
intentionally not advertised as public MCP servers.

## Analysis workstation

The `analysis-web` resource provides the Hypothesis Sequencer field-analysis
portal. Its deterministic field package and expected-paydirt analysis work
without a model connection. Aspire exposes it at the stable development origin
`http://localhost:5173` for Azure Maps CORS configuration.

### Demo guide and guided tutorial

Open **Demo & architecture guide** in the app for the HTML presenter script, or
open [`docs/index.html`](docs/index.html) directly in a browser. It covers every
task, the engineering inputs and outputs, and the services behind the workflow.
The same document is the repository's GitHub Pages site. The Pages workflow
publishes it after these files reach `main`; select **GitHub Actions** under the
repository's **Settings → Pages → Build and deployment**. Pages serves the guide,
not the running simulator.

Choose **Guided tutorial** to follow four short chapters in the app. The yellow
outline identifies the relevant control. Use **Prev** and **Next**, skip an
exercise, or close the tutorial at any time. The grid exercise waits for your
actual Apply result; tutorial navigation never applies settings or saves records.
Any edits you make remain after you close the tutorial.

The Field menu distinguishes **Original data** from **Simulated results**.
Simulated results include a scenario description and date. Scenario status is
shown separately from its name: **Scored** means evaluation is complete.
These labels do not rename or merge stored fields.

In **Correlation**, choose **Draft interpretation with AI** to generate a proposed
name, target rationale and formation notes from the displayed evidence and applied
settings. The request includes your current notes; a loaded hypothesis uses its
saved evidence. Review the citations and limitations, then choose **Use this
draft** to fill the form. Existing notes require confirmation before replacement.
**Save hypothesis** remains a separate action. The agent uses the configured
Azure OpenAI connection; unavailable models and invalid responses leave your text
unchanged. Manual notes and the example note remain available.

AI availability reports configuration, not available model capacity. If the
provider returns 429, drafting shows **AI capacity limit reached**, retains your
notes, and makes no automatic retry. Retry only after quota or capacity is available.
In Aspire's `analysis-api` structured logs, `FormationInterpretationFailure`
(event 8300) includes the original exception, its stack and inner exceptions, and
`ProviderStatusCode`, correlated with the HTTP request trace and span. Browser
errors remain sanitized.

Formation drafting runs a local Microsoft Agent Framework agent, not a
Foundry-hosted agent. It uses the Responses API for reasoning-capable function
tools, with response storage and background execution disabled. Encrypted
reasoning continuation stays within the request's local tool loop.
Its `invoke_agent` span contains a child model span for
the model request. In **Development**, both spans capture input/output messages,
including notes and field evidence; treat trace exports accordingly. Message
capture is off outside Development. The chat span records token usage when the
provider returns it; a rejected request may have no usage or output. Both layers
use Aspire's existing telemetry source and exporter configuration, with no
separate OTLP endpoint.
The formation agent starts with a small brief rather than sending the complete
dataset on every request. Its read-only tools summarize formation picks, read the
exact applied screening results, search records, inspect formation/log summaries,
and read engineer notes. It can request larger pages or the full prepared context
when the task needs it. Tools operate on the same verified snapshot throughout
the request, and final citations must refer to evidence actually returned.

The initial brief is at most 8 KiB; the complete prepared snapshot retains the
existing 192 KiB safety bound. Generous execution safeguards allow 128 tool calls,
40 function-invocation iterations, 2 MiB of cumulative retrieved data and a
10-minute request deadline. These are runaway-execution limits, not a small-context
target. Cancellation remains available, and provider failures are not retried
automatically.

In **Evidence → Data sources**, records are grouped by dataset, preparation and
classification. Expand the affected wellbores to trace each record to its source.
Shared files, licenses and credits are listed once. Original source, curve and
coordinate metadata is available through JSON downloads rather than raw data dumps.

Tutorial checks use the existing frontend runner:

```powershell
Set-Location .\src\DrillSim.AnalysisWeb
npm run test:tutorial
# Optional browser checks using an already installed Playwright module:
$env:DRILLSIM_PLAYWRIGHT_MODULE = "<absolute-path-to-playwright>\index.mjs"
npm run test:tutorial:browser
```

Configure the AppHost parameters with its existing user-secrets store:

```powershell
dotnet user-secrets set "Parameters:azure-openai-endpoint" "https://<resource>.openai.azure.com/" --project .\aspire\DrillSim.AppHost
dotnet user-secrets set "Parameters:azure-openai-deployment-name" "<deployment>" --project .\aspire\DrillSim.AppHost
dotnet user-secrets set "Parameters:azure-openai-subscription-id" "<openai-subscription-id>" --project .\aspire\DrillSim.AppHost
dotnet user-secrets set "Parameters:azure-maps-client-id" "<azure-maps-account-client-id>" --project .\aspire\DrillSim.AppHost
dotnet user-secrets set "Parameters:azure-maps-tenant-id" "<maps-tenant-id>" --project .\aspire\DrillSim.AppHost
dotnet user-secrets set "Parameters:azure-maps-subscription-id" "<maps-subscription-id>" --project .\aspire\DrillSim.AppHost
dotnet user-secrets set "Parameters:reservoir-simulation-operator-key" "<random-operator-key>" --project .\aspire\DrillSim.AppHost
dotnet user-secrets set "Parameters:drilling-operations-internal-key" "<random-internal-key>" --project .\aspire\DrillSim.AppHost
dotnet user-secrets set "Parameters:drilling-operations-analysis-callback-key" "<random-callback-key>" --project .\aspire\DrillSim.AppHost
```

Azure parameters remain optional so the portal still starts offline. The
reservoir-simulation operator key is required and is not referenced by the
analysis API or browser. Drilling Operations is internal-only and receives the
Stage A operator key plus its own internal and future Analysis API callback
credentials; none are forwarded to the browser. Azure authentication uses
`AzureCliCredential`; start
Aspire from the intended AzEnv profile so its `AZURE_CONFIG_DIR` is forwarded
to the analysis API. Credentials are never sent to the browser. The analysis
API brokers short-lived Azure Maps tokens for
`https://atlas.microsoft.com/.default`.

For an uncommitted S8 publication rejected by a destination service, **New
evidence** can offer **Recover staged publication** after the underlying import
problem is corrected. Inspect the staged fingerprints and operation counts,
enter the local audit label and correction reason, and acknowledge that exact
review. Recovery preserves the same run and staged plan; **Publish reveal and
evaluate** is a separate action with a new attempt key. An uncertain response
must be retried with its saved key, not treated as rollback. Recovery is unavailable
for unrelated failures, changed evidence, or an activated/prepared/committed reveal.
It uses the same local-only antiforgery-protected operator facade; labels are
audit annotations, not authenticated identities.
Architecture and geology cleanup retain every publication-marked record,
regardless of its simulation date. A missing previously verified record blocks
recovery; restoration requires explicit review of the saved immutable payload
and confirmation of its original business hash.

For an eligible rejected scorecard with the historical absolute-error-bound
defect, **Evaluation** offers **Append corrected evaluation**. Review the
original/corrected fingerprints and correction reason before appending the
versioned artifact. The rejected artifact, original inputs and point scores
remain intact. Use **Run / retry evaluation** separately to publish it; neither
operation repeats drilling or publication.

The hidden reservoir service conditions deterministic structured worlds from
logged controls and runs an adaptive immiscible oil/water/gas IMPES kernel. Its
cell truth remains in service memory. An operator can seed the Troll-Sognefjord
world without exposing that truth to the analysis agent:

```powershell
$env:RESERVOIR_SIMULATION_OPERATOR_KEY = "<same operator key>"
python .\scripts\seed_reservoir_world.py
```

## Services

| Aspire resource | Source project |
| --- | --- |
| `cartographic-projection` | CartographicProjection |
| `analysis-api` | DrillSim.AnalysisApi |
| `analysis-web` | DrillSim.AnalysisWeb |
| `cluster` | Cluster |
| `cluster-ui` | Cluster WebApp |
| `drilling-operations` | Internal checkpointed drilling workflow |
| `drilling-fluid` | DrillingFluid |
| `drill-string` | DrillString |
| `earth-gravity` | EarthGravity |
| `earth-magnetic-field` | EarthMagneticField |
| `earth-vertical-datum` | EarthVerticalDatum |
| `field` | Field |
| `geodetic-datum` | GeodeticDatum |
| `geological-properties` | GeologicalProperties |
| `geothermal-properties` | GeothermalProperties |
| `rig` | Rig |
| `reservoir-simulation` | ReservoirSimulation hidden ground-truth service |
| `simulator-4ndof` | Simulator4nDOF |
| `survey-instrument` | SurveyInstrument |
| `trajectory` | Trajectory |
| `unit-conversion` | UnitConversion |
| `well` | Well |
| `well-bore` | WellBore |
| `well-bore-architecture` | WellBoreArchitecture |
| `ypl-calibration` | YPLCalibrationFromRheometer |

## Repository layout

| Path | Contents |
| --- | --- |
| `aspire\DrillSim.AppHost` | Aspire application topology and service wiring |
| `aspire\DrillSim.ServiceDefaults` | Shared telemetry, health, discovery, and resilience configuration |
| `src` | Ported service, model, UI, and supporting projects |
| `data` | Local SQLite databases created at runtime |

## Attribution and license

The projects under `src\` are derived from software published by OSDC under the
MIT License. Their original copyright and license notices remain applicable to
the imported code and its original contributors.

This repository and its independently authored changes are licensed under the
[MIT License](LICENSE), copyright (c) 2026 Zachary Way.
