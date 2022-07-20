using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sanctuary.Census.Common.Extensions;
using Sanctuary.Census.PatchData.Abstractions.Services;
using Sanctuary.Census.PatchData.Services;

namespace Sanctuary.Census.PatchData.Extensions;

/// <summary>
/// Extension methods for the <see cref="IServiceCollection"/> type.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services relevant to retrieving patch data to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection, so that calls may be chained.</returns>
    public static IServiceCollection AddPatchDataServices(this IServiceCollection services)
    {
        services.AddCommonServices();
        services.TryAddScoped<IPatchDataCacheService, PatchDataCacheService>();

        return services;
    }
}
