using Sanctuary.Census.Common.Abstractions;
using Sanctuary.Census.Common.Abstractions.Services;
using System;
using System.Collections.Generic;

namespace Sanctuary.Census.Common.Services;

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
    public IEnumerable<Type> GetContributors()
        => _contributors;
}
