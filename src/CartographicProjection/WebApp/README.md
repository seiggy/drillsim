# WebApp

The WebApp project is a Blazor Server application that provides a UI for the CartographicProjection microservice. Users can manage cartographic projections and conversion sets and run conversions between geodetic and cartographic coordinates.

## Purpose

- Offer a browser‑based UI for CRUD operations on `CartographicProjection` and `CartographicConversionSet`.
- Provide a conversion tool UI (see `Pages/CartographicConverter.razor:1`).
- Integrate unit/measurement widgets from OSDC (MudBlazor‑based) for user‑friendly inputs.

## Install & Run

- Prerequisites: .NET SDK 8.0, Docker (optional) and access to the Service API.

- Local run (dev):
  - `dotnet run --project WebApp/WebApp.csproj`
  - Base path: `http://localhost:8080/CartographicProjection/webapp`
  - Main pages:
    - `/CartographicProjection` — list/edit projections
    - `/CartographicConverter` — run conversions
    - `/SingleUnitConversion` — convert a single value between unit choices

- Navigation:
  - `Projection Management`: cartographic projections
  - `Contextual Data`: geodetic datums and spheroids
  - `Calculators`: cartographic conversions and unit conversion
  - `Monitoring`: usage statistics

- Configuration (service endpoints):
  - `CartographicProjectionHostURL`: CartographicProjection Service base URL
  - `GeodeticDatumHostURL`: GeodeticDatum Service base URL
  - `UnitConversionHostURL`: UnitConversion Service base URL
  - Defaults:
    - Dev: `WebApp/appsettings.Development.json:1`
    - Prod: `WebApp/appsettings.Production.json:1`
  - Override via environment variables of the same names.

- Docker:
  - Build: `docker build -t norcedrillingcartographicprojectionwebappclient -f WebApp/Dockerfile .`
  - Run: `docker run -p 8080:8080 -e CartographicProjectionHostURL=https://dev.digiwells.no/ -e GeodeticDatumHostURL=https://dev.digiwells.no/ -e UnitConversionHostURL=https://dev.digiwells.no/ norcedrillingcartographicprojectionwebappclient`
  - Open: `http://localhost:8080/CartographicProjection/webapp`

## Usage Examples

- Browse projections: navigate to `/CartographicProjection` to view, add, edit, or delete projections.
- Edit projections: the editor separates `Description` from `Configuration`, uses unit-aware inputs for projection parameters, and asks for confirmation if `Close` is selected with unsaved changes.
- Convert coordinates: navigate to `/CartographicConverter` to enter geodetic/cartographic inputs and convert using selected projection. Components use OSDC unit inputs (e.g., `MudInputWithUnitAdornment`).
- Convert units: navigate to `/SingleUnitConversion` from the `Calculators` menu to use the reusable UnitConversion calculator.

## Dependencies

- Packages (`WebApp/WebApp.csproj:1`):
  - `OSDC.UnitConversion.DrillingRazorMudComponents` (unit inputs and selectors)
  - `OSDC.UnitConversion.WebPages` (single unit conversion calculator)
  - `OSDC.DotnetLibraries.General.DataManagement`
  - `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` (container tooling)
- Project References:
  - `../ModelSharedOut/ModelSharedOut.csproj` — provides the generated `Client` and DTOs for calling the Service API.
- UI Frameworks:
  - MudBlazor (`Program.cs:1` registers `AddMudServices`).

## Integration

- Service: WebApp calls Service endpoints through the generated `Client` in `ModelSharedOut`.
- ModelSharedOut: Supplies strongly‑typed DTOs and client methods used across pages (`Shared/APIUtils.cs:1`).
- Model/ModelSharedIn: Not referenced directly by WebApp.

## Configuration Notes

- Base path is set to `/CartographicProjection/webapp` in `Program.cs:1` via `UsePathBase`. Ensure your ingress or reverse proxy aligns with this path.
- `Shared/APIUtils.cs:1` constructs `HttpClient` instances for CartographicProjection, GeodeticDatum and UnitConversion services and disables certificate validation in development.

## Deployed URLs

- Swagger (Service, dev): https://dev.digiwells.no/CartographicProjection/api/swagger
- WebApp (dev): https://dev.digiwells.no/CartographicProjection/webapp
- Swagger (Service, prod): https://app.digiwells.no/CartographicProjection/api/swagger
- WebApp (prod): https://app.digiwells.no/CartographicProjection/webapp

## Funding

The current work has been funded by the Research Council of Norway and Industry partners in the SFI Digiwells (2020–2028) centre for research‑based innovation.

## Contributors

- Eric Cayeux — NORCE Energy Modelling and Automation
- Gilles Pelfrene — NORCE Energy Modelling and Automation
- Andrew Holsaeter — NORCE Energy Modelling and Automation
