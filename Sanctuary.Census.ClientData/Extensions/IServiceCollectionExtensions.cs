using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.Services;
using Sanctuary.Census.Common.Extensions;

namespace Sanctuary.Census.ClientData.Extensions;

/// <summary>
/// Extension methods for the <see cref="IServiceCollection"/> type.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services relevant to retrieving client data to the service collection.
    /// </summary>
    /// <param name="builder">The host builder.</param>
    /// <returns>The host builder, so that calls may be chained.</returns>
    public static IHostApplicationBuilder AddClientDataServices(this IHostApplicationBuilder builder)
    {
        builder.AddCommonServices();

        builder.Services.TryAddScoped<ILocaleDataCacheService, LocaleDataCacheService>();
        builder.Services.TryAddScoped<IClientDataCacheService, ClientDataCacheService>();

        return builder;
    }
}
