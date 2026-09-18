using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace DrillSim.AppHost;

internal static class ServiceKeyParameters
{
    internal static IResourceBuilder<ParameterResource> AddServiceKey(
        this IDistributedApplicationBuilder builder, string name) =>
        builder.AddParameter(name, new GenerateParameterDefault
        {
            MinLength = 64,
            Lower = true,
            Upper = true,
            Numeric = true,
            Special = false
        }, secret: true, persist: true);
}
