# NORCE.Drilling.DrillingFluid.WebPages

Reusable Razor class library containing the DrillingFluid web pages and dependent editor components extracted from the `WebApp` host.

## Included UI
- completion order pages
- drilling fluid pages
- drilling fluid description pages
- fluid composition pages
- rheology editors and plotting components used by those pages

## Dependencies
- `OSDC.DotnetLibraries.Drilling.WebAppUtils`
- `ModelSharedOut`
- `MudBlazor`
- `OSDC.UnitConversion.DrillingRazorMudComponents`
- `Plotly.Blazor`

## Host integration
The consuming Blazor host must:

1. Reference this package.
2. Provide an implementation of `IDrillingFluidWebPagesConfiguration`.
3. Register `IDrillingFluidWebPagesConfiguration` and `IDrillingFluidAPIUtils` in DI.
4. Add the library assembly to the router `AdditionalAssemblies`.

Example registration:

```csharp
builder.Services.AddSingleton<IDrillingFluidWebPagesConfiguration>(new WebPagesHostConfiguration
{
    DrillingFluidHostURL = builder.Configuration["DrillingFluidHostURL"] ?? string.Empty,
    DrillStringHostURL = builder.Configuration["DrillStringHostURL"] ?? string.Empty,
    UnitConversionHostURL = builder.Configuration["UnitConversionHostURL"] ?? string.Empty,
});
builder.Services.AddSingleton<IDrillingFluidAPIUtils, DrillingFluidAPIUtils>();
```
