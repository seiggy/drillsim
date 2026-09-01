using MudBlazor;
using MudBlazor.Services;
using NORCE.Drilling.DrillingFluid.WebApp;
using NORCE.Drilling.DrillingFluid.WebPages;

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
    DrillingFluidHostURL = builder.Configuration["DrillingFluidHostURL"] ?? string.Empty,
    DrillStringHostURL = builder.Configuration["DrillStringHostURL"] ?? string.Empty,
    UnitConversionHostURL = builder.Configuration["UnitConversionHostURL"] ?? string.Empty
};

builder.Services.AddSingleton<IDrillingFluidWebPagesConfiguration>(webPagesConfiguration);
builder.Services.AddSingleton<IDrillingFluidAPIUtils, DrillingFluidAPIUtils>();

var app = builder.Build();

app.UseForwardedHeaders();
var basePath = "/drillingfluid/webapp";
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

