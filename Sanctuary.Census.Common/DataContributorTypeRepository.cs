using Sanctuary.Census.Common.Abstractions;
using System;
using System.Collections.Generic;

namespace Sanctuary.Census.Common;

/// <inheritdoc />
public class DataContributorTypeRepository : IDataContributorTypeRepository
{
    private readonly List<Type> _contributors;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataContributorTypeRepository"/> class.
    /// </summary>
    public DataContributorTypeRepository()
    {
        _contributors = new List<Type>();
    }

    /// <inheritdoc />
    public void RegisterContributer<TContributor>()
        where TContributor : class, IDataContributor
        => _contributors.Add(typeof(TContributor));

    /// <inheritdoc />
    public IReadOnlyList<Type> GetContributors()
        => _contributors;
}
