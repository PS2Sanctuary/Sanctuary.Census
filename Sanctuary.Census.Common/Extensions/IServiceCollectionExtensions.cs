using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Sanctuary.Census.Common.Abstractions;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Services;
using System;

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
        services.TryAddSingleton<IDataContributorTypeRepository>
        (
            s => s.GetRequiredService<IOptions<DataContributorTypeRepository>>().Value
        );

        return services;
    }

    /// <summary>
    /// Adds an <see cref="IDataContributor"/> to the service collection.
    /// </summary>
    /// <typeparam name="TContributor">The contributor type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="implementationFactory">A factory that can create the contributor.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance, so that calls may be chained.</returns>
    public static IServiceCollection RegisterDataContributor<TContributor>
    (
        this IServiceCollection services,
        Func<IServiceProvider, TContributor>? implementationFactory = null
    )
        where TContributor : class, IDataContributor
    {
        if (implementationFactory is null)
            services.TryAddTransient<TContributor>();
        else
            services.TryAddTransient(implementationFactory);

        services.Configure<DataContributorTypeRepository>(r => r.RegisterContributer<TContributor>());

        return services;
    }
}
