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
    /// <param name="services">The service collection.</param>
    /// <param name="hostEnvironment">The current host environment.</param>
    /// <returns>The service collection, so that calls may be chained.</returns>
    public static IServiceCollection AddClientDataServices(this IServiceCollection services, IHostEnvironment hostEnvironment)
    {
        services.AddCommonServices(hostEnvironment);
        services.TryAddScoped<ILocaleDataCacheService, LocaleDataCacheService>();
        services.TryAddScoped<IClientDataCacheService, ClientDataCacheService>();

        return services;
    }
}
