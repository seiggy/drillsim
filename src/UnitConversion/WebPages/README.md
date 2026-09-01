# OSDC.UnitConversion.WebPages

Reusable Razor class library containing the UnitConversion web pages.

## Usage

Reference the package from a Blazor Server web application, configure the UnitConversion service URL, and include the assembly in the application router:

```razor
<Router AppAssembly="@typeof(App).Assembly"
        AdditionalAssemblies="new[] { typeof(OSDC.UnitConversion.WebPages.UnitConversionMain).Assembly }">
```

The host application must register MudBlazor and MudBlazor.Markdown services.

```csharp
builder.Services.AddMudServices();
builder.Services.AddMudMarkdownServices();
OSDC.UnitConversion.WebPages.Configuration.UnitConversionHostURL = builder.Configuration["UnitConversionHostURL"];
```

## Pages

- `/UnitConversion`
- `/SingleUnitConversion`
- `/PhysicalQuantity`
- `/Statistics`
