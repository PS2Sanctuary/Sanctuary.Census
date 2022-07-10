using Sanctuary.Census.Common.Abstractions;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using System;
using System.Collections.Generic;

namespace Sanctuary.Census.Common.Services;

/// <inheritdoc />
public class DataContributorTypeRepository : IDataContributorTypeRepository
{
    private readonly Dictionary<Type, Func<IServiceProvider, PS2Environment, IDataContributor>> _contributors;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataContributorTypeRepository"/> class.
    /// </summary>
    public DataContributorTypeRepository()
    {
        _contributors = new Dictionary<Type, Func<IServiceProvider, PS2Environment, IDataContributor>>();
    }

    /// <inheritdoc />
    public void RegisterContributer<TContributor>
    (
        Func<IServiceProvider, PS2Environment, TContributor> implementationFactory
    ) where TContributor : class, IDataContributor
        => _contributors.Add(typeof(TContributor), implementationFactory);

    /// <inheritdoc />
    public IEnumerable<Type> GetContributors()
        => _contributors.Keys;

    /// <inheritdoc />
    public IDataContributor BuildContributor
    (
        Type contributorType,
        IServiceProvider services,
        PS2Environment environment
    )
    {
        if (!_contributors.ContainsKey(contributorType))
            throw new ArgumentException($"The type {contributorType} has not been registered", nameof(contributorType));

        return _contributors[contributorType](services, environment);
    }
}
