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
    void RegisterContributer<TContributor>()
        where TContributor : class, IDataContributor;

    /// <summary>
    /// Gets any registered contributors.
    /// </summary>
    /// <returns></returns>
    IReadOnlyList<Type> GetContributors();
}
