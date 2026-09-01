# NORCE.Drilling.CartographicProjection.WebPages

Reusable Razor class library for the Cartographic Projection web UI.

It contains the `CartographicProjection`, `CartographicProjectionEdit`, `CartographicConverter`, and `StatisticsMain` pages together with the supporting API and UI utility code they depend on.

## Package contents

- Cartographic projection list and edit pages
- Cartographic conversion page
- Usage statistics page
- Host-configurable API access through injected configuration

## UI behavior

- The cartographic projection edit page uses MudBlazor expansion panels to separate `Description` from `Configuration`.
- The description panel contains metadata and contextual selections such as name, description, geodetic datum, and projection type.
- The configuration panel contains projection-type-specific parameters, using unit-aware inputs where relevant.
- The editor uses `Save` and `Close` actions. Closing with unsaved edits opens a confirmation dialog before leaving the editor.
- The WebApp left-side menu is organized with grouped sections for projection management, contextual data, calculators, and monitoring.

## Dependencies

- `OSDC.DotnetLibraries.Drilling.WebAppUtils`
- `MudBlazor`
- `OSDC.UnitConversion.DrillingRazorMudComponents`
- `ModelSharedOut`

## Host integration

The consuming app should:

1. Reference this package.
2. Provide an implementation of `ICartographicProjectionWebPagesConfiguration`.
3. Register that configuration and `ICartographicProjectionAPIUtils` in DI.
4. Add the `WebPages` assembly to the Blazor router `AdditionalAssemblies`.

## Required configuration

- `CartographicProjectionHostURL`
- `GeodeticDatumHostURL`
- `UnitConversionHostURL`
