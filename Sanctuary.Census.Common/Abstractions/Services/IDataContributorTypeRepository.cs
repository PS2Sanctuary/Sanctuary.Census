using Sanctuary.Census.Common.Objects;
using System;
using System.Collections.Generic;

namespace Sanctuary.Census.Common.Abstractions.Services;

/// <summary>
/// Represents a repository of <see cref="IDataContributor{TContributeFrom}"/> types.
/// </summary>
public interface IDataContributorTypeRepository
{
    /// <summary>
    /// Registers a contributor to the container.
    /// </summary>
    /// <typeparam name="TContributor">The type of the contributor.</typeparam>
    /// <param name="implementationFactory">A factory that can create the contributor.</param>
    void RegisterContributer<TContributor>
    (
        Func<IServiceProvider, PS2Environment, TContributor> implementationFactory
    ) where TContributor : class, IDataContributor;

    /// <summary>
    /// Gets any registered contributors.
    /// </summary>
    /// <returns></returns>
    IEnumerable<Type> GetContributors();

    /// <summary>
    /// Builds a registered contributor.
    /// </summary>
    /// <param name="contributorType">The type of the contributor to build.</param>
    /// <param name="services">The service provider.</param>
    /// <param name="environment">The environment to build the contributor for.</param>
    /// <returns>The built contributor.</returns>
    IDataContributor BuildContributor
    (
        Type contributorType,
        IServiceProvider services,
        PS2Environment environment
    );
}
