# DrillSim

DrillSim is a .NET 10 monorepo for the active C# microservices maintained by the
[Open Source Drilling Community](https://github.com/Open-Source-Drilling-Community).
The Aspire AppHost starts the complete service graph with unique, dynamically
assigned HTTP and HTTPS endpoints. Each service receives an Aspire-managed
SQLite resource persisted under `data\`. The Cluster Blazor Server UI is wired
to its ten supporting services through Aspire endpoint references.

## Prerequisites

- .NET SDK 10.0.400 or a compatible .NET 10 feature-band SDK
- Aspire CLI 13.5.3 or newer

## Restore and build

Several upstream projects generate Swagger artifacts with a checked-in local
tool manifest. Restore those tools before building:

```powershell
Get-ChildItem .\src -Recurse -Filter dotnet-tools.json |
    ForEach-Object { dotnet tool restore --tool-manifest $_.FullName }

dotnet restore .\DrillSim.slnx
dotnet build .\DrillSim.slnx --no-restore
```

## Run

```powershell
aspire start
```

The command prints the Aspire dashboard URL. Aspire assigns ports at launch, so
use the dashboard to open a service or inspect its SQLite resource. Every API
exposes `/health` and `/alive` in this development environment. Stop the
complete application with:

```powershell
aspire stop
```

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
