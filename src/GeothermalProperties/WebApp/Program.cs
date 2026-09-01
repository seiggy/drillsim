using MudBlazor;
using MudBlazor.Services;
using NORCE.Drilling.GeothermalProperties.WebApp;
using NORCE.Drilling.GeothermalProperties.WebPages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

var webPagesConfiguration = new WebPagesHostConfiguration
{
    GeothermalPropertiesHostURL = builder.Configuration["GeothermalPropertiesHostURL"],
    UnitConversionHostURL = builder.Configuration["UnitConversionHostURL"],
    TrajectoryHostURL = builder.Configuration["TrajectoryHostURL"],
    WellBoreHostURL = builder.Configuration["WellBoreHostURL"]
};

builder.Services.AddSingleton<IGeothermalPropertiesWebPagesConfiguration>(webPagesConfiguration);
builder.Services.AddSingleton<IGeothermalPropertiesAPIUtils, GeothermalPropertiesAPIUtils>();

var app = builder.Build();

app.UseForwardedHeaders();
var basePath = "/geothermalproperties/webapp";
app.UsePathBase(basePath);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

