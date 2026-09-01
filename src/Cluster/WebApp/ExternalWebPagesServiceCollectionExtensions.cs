using Microsoft.Extensions.DependencyInjection;

namespace OSDC.Drilling.Cluster.WebApp;

public static class ExternalWebPagesServiceCollectionExtensions
{
    public static IServiceCollection AddExternalWebPages(this IServiceCollection services, WebPagesHostConfiguration configuration)
    {
        services.AddSingleton<OSDC.Drilling.Field.WebPages.IFieldWebPagesConfiguration>(configuration);
        services.AddSingleton<
            OSDC.Drilling.Field.WebPages.IFieldAPIUtils,
            OSDC.Drilling.Field.WebPages.FieldAPIUtils>();
        services.AddSingleton<OSDC.Drilling.Rig.WebPages.IRigWebPagesConfiguration>(configuration);
        services.AddSingleton<
            OSDC.Drilling.Rig.WebPages.IRigAPIUtils,
            OSDC.Drilling.Rig.WebPages.RigAPIUtils>();
        services.AddScoped<OSDC.Drilling.Rig.WebPages.Shared.RigApiClient>();
        services.AddScoped<OSDC.Drilling.Rig.WebPages.Shared.FieldClusterApiClient>();
        services.AddSingleton<
            NORCE.Drilling.CartographicProjection.WebPages.ICartographicProjectionWebPagesConfiguration>(configuration);
        services.AddSingleton<
            NORCE.Drilling.CartographicProjection.WebPages.ICartographicProjectionAPIUtils,
            NORCE.Drilling.CartographicProjection.WebPages.CartographicProjectionAPIUtils>();
        services.AddSingleton<NORCE.Drilling.GeodeticDatum.WebPages.IGeodeticDatumWebPagesConfiguration>(configuration);
        services.AddSingleton<
            NORCE.Drilling.GeodeticDatum.WebPages.IGeodeticDatumAPIUtils,
            NORCE.Drilling.GeodeticDatum.WebPages.GeodeticDatumAPIUtils>();
        services.AddSingleton<OSDC.Drilling.EarthGravity.WebPages.IEarthGravityWebPagesConfiguration>(configuration);
        services.AddSingleton<
            OSDC.Drilling.EarthGravity.WebPages.IEarthGravityAPIUtils,
            OSDC.Drilling.EarthGravity.WebPages.APIUtils>();
        services.AddSingleton<OSDC.Drilling.EarthMagneticField.WebPages.IEarthMagneticFieldWebPagesConfiguration>(configuration);
        services.AddSingleton<
            OSDC.Drilling.EarthMagneticField.WebPages.IEarthMagneticFieldAPIUtils,
            OSDC.Drilling.EarthMagneticField.WebPages.APIUtils>();
        services.AddSingleton<OSDC.Drilling.EarthVerticalDatum.WebPages.IEarthVerticalDatumWebPagesConfiguration>(configuration);
        services.AddSingleton<
            OSDC.Drilling.EarthVerticalDatum.WebPages.IEarthVerticalDatumAPIUtils,
            OSDC.Drilling.EarthVerticalDatum.WebPages.APIUtils>();
        return services;
    }
}
