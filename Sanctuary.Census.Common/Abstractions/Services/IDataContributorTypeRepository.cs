using System;
using System.Collections.Generic;

namespace Sanctuary.Census.Common.Abstractions.Services;

/// <summary>
/// Represents a repository of <see cref="IDataContributor"/> types.
/// </summary>
public interface IDataContributorTypeRepository
{
    /// <summary>
    /// Registers a contributor to the container.
    /// </summary>
    /// <typeparam name="TContributor">The type of the contributor.</typeparam>
    /// <param name="order">
    /// The order value of the contributor. Higher-order contributors
    /// will be executed after lower-order contributors.
    /// </param>
    void RegisterContributer<TContributor>(int order)
        where TContributor : class, IDataContributor;

    /// <summary>
    /// Gets any registered contributors.
    /// </summary>
    /// <returns></returns>
    IEnumerable<Type> GetContributors();
}
