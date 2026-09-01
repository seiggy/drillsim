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

- 20 drilling service APIs and the Cluster Blazor web application
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

## Service endpoints

Ports are assigned at launch. Open a service from the Aspire dashboard, then
use these paths:

| Path | Purpose |
| --- | --- |
| `/swagger` | Scalar API reference |
| `/swagger/v1/swagger.json` | Generated OpenAPI document |
| `/health` | Readiness health check |
| `/alive` | Liveness health check |
| `/mcp` | MCP HTTP transport |
| `/mcp/ws` | MCP WebSocket compatibility transport |

## Services

| Aspire resource | Source project |
| --- | --- |
| `cartographic-projection` | CartographicProjection |
| `cluster` | Cluster |
| `cluster-ui` | Cluster WebApp |
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
