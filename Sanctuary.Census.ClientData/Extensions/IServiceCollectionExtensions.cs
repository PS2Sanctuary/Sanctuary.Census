﻿using Microsoft.Extensions.DependencyInjection;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.Services;

namespace Sanctuary.Census.ClientData.Extensions;

/// <summary>
/// Extension methods for the <see cref="IServiceCollection"/> type.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services relevant to retrieve client data to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="appDataDirectory">The app data directory that various services may use.</param>
    /// <returns>The service collection, so that calls may be chained.</returns>
    public static IServiceCollection AddClientDataService(this IServiceCollection services, string appDataDirectory)
    {
#if DEBUG
        services.AddHttpClient<IManifestService, DebugManifestService>(h => new DebugManifestService(h, appDataDirectory));
#else
        services.AddHttpClient<IManifestService, CachingManifestService>(h => new CachingManifestService(h, AppDataDirectory));
#endif

        services.AddTransient<IDatasheetLoaderService, DatasheetLoaderService>();

        return services;
    }
}