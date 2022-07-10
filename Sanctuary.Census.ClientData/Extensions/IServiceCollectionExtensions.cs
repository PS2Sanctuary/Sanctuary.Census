﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    /// <param name="appDataDirectory">The app data directory that various services may use.</param>
    /// <returns>The service collection, so that calls may be chained.</returns>
    public static IServiceCollection AddClientDataServices(this IServiceCollection services, string appDataDirectory)
    {
#if DEBUG
        services.AddHttpClient<IManifestService, DebugManifestService>(h => new DebugManifestService(h, appDataDirectory));
#else
        services.AddHttpClient<IManifestService, CachingManifestService>(h => new CachingManifestService(h, AppDataDirectory));
#endif

        services.TryAddTransient<IDatasheetLoaderService, DatasheetLoaderService>();

        services.AddCommonServices();

        services.RegisterDataContributor
        (
            (s, e) => new ItemProfileDataContributor(s.GetRequiredService<IDatasheetLoaderService>(), e)
        );

        services.RegisterDataContributor
        (
            (s, e) => new ClientItemDefinitionDataContributor(s.GetRequiredService<IDatasheetLoaderService>(), e)
        );

        return services;
    }
}
