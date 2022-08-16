using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Services;
using System.IO.Abstractions;

namespace Sanctuary.Census.Common.Extensions;

/// <summary>
/// Extension methods for the <see cref="IServiceCollection"/> type.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services common Sanctuary.Census services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance, so that calls may be chained.</returns>
    public static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IFileSystem, FileSystem>();
        services.TryAddScoped<EnvironmentContextProvider>();

        services.AddSingleton(new MongoClient("mongodb://localhost:27017"))
            .AddScoped<IMongoContext, MongoContext>();

        services.AddHttpClient(nameof(ManifestService));
#if DEBUG
        services.TryAddSingleton<IManifestService, DebugManifestService>();
#else
        services.TryAddSingleton<IManifestService, CachingManifestService>();
#endif

        return services;
    }
}
