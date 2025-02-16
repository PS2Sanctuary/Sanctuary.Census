using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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
    /// <param name="builder">The host builder.</param>
    /// <returns>The host builder, so that calls may be chained.</returns>
    public static IHostApplicationBuilder AddPatchDataServices(this IHostApplicationBuilder builder)
    {
        builder.AddCommonServices();

        builder.Services.TryAddScoped<IPatchDataCacheService, PatchDataCacheService>();

        return builder;
    }
}
