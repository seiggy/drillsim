using MudBlazor;
using MudBlazor.Services;
using OSDC.Drilling.Cluster.WebApp;
using OSDC.Drilling.Cluster.WebPages;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(addSqlite: false);

WebPagesHostConfiguration webPagesConfiguration = new()
{
    ClusterHostURL = GetRequiredServiceUrl(builder.Configuration, "ClusterHostURL"),
    FieldHostURL = GetRequiredServiceUrl(builder.Configuration, "FieldHostURL"),
    RigHostURL = GetRequiredServiceUrl(builder.Configuration, "RigHostURL"),
    TrajectoryHostURL = GetRequiredServiceUrl(builder.Configuration, "TrajectoryHostURL"),
    EarthCartographicProjectionHostURL = GetRequiredServiceUrl(builder.Configuration, "EarthCartographicProjectionHostURL"),
    EarthGeodesyHostURL = GetRequiredServiceUrl(builder.Configuration, "EarthGeodesyHostURL"),
    EarthGravityHostURL = GetRequiredServiceUrl(builder.Configuration, "EarthGravityHostURL"),
    EarthMagneticFieldHostURL = GetRequiredServiceUrl(builder.Configuration, "EarthMagneticFieldHostURL"),
    EarthVerticalDatumHostURL = GetRequiredServiceUrl(builder.Configuration, "EarthVerticalDatumHostURL"),
    UnitConversionHostURL = GetRequiredServiceUrl(builder.Configuration, "UnitConversionHostURL"),
};

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();
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
builder.Services.AddSingleton<IClusterWebPagesConfiguration>(webPagesConfiguration);
builder.Services.AddSingleton<IClusterAPIUtils, ClusterAPIUtils>();
builder.Services.AddExternalWebPages(webPagesConfiguration);

var app = builder.Build();

app.UseForwardedHeaders();
var basePath = "/cluster/webapp";
app.UsePathBase(basePath);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapDefaultEndpoints();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

static string GetRequiredServiceUrl(IConfiguration configuration, string key)
{
    string? value = configuration[key];
    if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? uri))
    {
        throw new InvalidOperationException($"Configuration value '{key}' must be an absolute URL.");
    }

    string absoluteUrl = uri.AbsoluteUri;
    return absoluteUrl.EndsWith('/') ? absoluteUrl : $"{absoluteUrl}/";
}
