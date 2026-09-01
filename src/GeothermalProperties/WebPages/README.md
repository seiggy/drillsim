# NORCE.Drilling.GeothermalProperties.WebPages

Reusable Razor class library containing the GeothermalProperties pages and supporting components.

## Contents
- `GeothermalPropertiesMain`, `GeothermalPropertiesEdit`, `GeothermalPropertiesView`
- `GeothermalPropertiesInterpolatedMain`
- `GeothermalPropertiesCompletionOrderMain`, `GeothermalPropertiesCompletionOrderEdit`, `GeothermalPropertiesCompletionOrderView`
- shared dialog and chart components for geothermal data visualisation

## Dependencies
- `ModelSharedOut` for generated DTOs, pseudo-constructors, and API clients
- `OSDC.DotnetLibraries.Drilling.WebAppUtils` for host URL configuration contracts and shared API utility support

## Host Integration
Register a host configuration object implementing `IGeothermalPropertiesWebPagesConfiguration`, then register `IGeothermalPropertiesAPIUtils` with `GeothermalPropertiesAPIUtils`.

The host application must also include this assembly in Blazor routing:

```csharp
typeof(NORCE.Drilling.GeothermalProperties.WebPages.Pages.GeothermalPropertiesMain).Assembly
```

## Packaging
This project is intended to be packed and published as the NuGet package `NORCE.Drilling.GeothermalProperties.WebPages`.
