using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Sanctuary.Census.Common.Abstractions;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Services;
using System;
using System.IO.Abstractions;
using System.Net.Http;

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
        services.TryAddSingleton<IDataContributorTypeRepository>
        (
            s => s.GetRequiredService<IOptions<DataContributorTypeRepository>>().Value
        );

        services.AddHttpClient(nameof(ManifestService));
#if DEBUG
        services.TryAddSingleton<IManifestService, DebugManifestService>();
#else
        services.TryAddSingleton<IManifestService, CachingManifestService>();
#endif

        services.TryAddScoped<IContributionService, ContributionService>();
        services.TryAddScoped<ILocaleService, LocaleService>();
        services.TryAddScoped<EnvironmentContextProvider>();

        return services;
    }

    /// <summary>
    /// Adds an <see cref="IDataContributor"/> to the service collection.
    /// </summary>
    /// <typeparam name="TContributor">The contributor type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="order">
    /// The order value of the contributor. Higher-order contributors
    /// will be executed after lower-order contributors.
    /// </param>
    /// <param name="implementationFactory">A factory that can create the contributor.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance, so that calls may be chained.</returns>
    public static IServiceCollection RegisterDataContributor<TContributor>
    (
        this IServiceCollection services,
        int order = 0,
        Func<IServiceProvider, TContributor>? implementationFactory = null
    )
        where TContributor : class, IDataContributor
    {
        if (implementationFactory is null)
            services.TryAddScoped<TContributor>();
        else
            services.TryAddScoped(implementationFactory);

        services.Configure<DataContributorTypeRepository>(r => r.RegisterContributer<TContributor>(order));
        return services;
    }
}
