using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sanctuary.Census.ClientData.Abstractions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.DataContributors;
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
    /// <returns>The service collection, so that calls may be chained.</returns>
    public static IServiceCollection AddClientDataServices(this IServiceCollection services)
    {
        services.AddCommonServices();
        services.TryAddTransient<IDatasheetLoaderService, DatasheetLoaderService>();
        services.TryAddScoped<IClientDataCacheService, ClientDataCacheService>();

        services.RegisterDataContributor<ItemProfileDataContributor>()
            .RegisterDataContributor<ClientItemDefinitionDataContributor>()
            .RegisterDataContributor<ImageSetMappingDataContributor>(1);

        return services;
    }
}
