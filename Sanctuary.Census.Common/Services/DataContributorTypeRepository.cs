using Sanctuary.Census.Common.Abstractions;
using Sanctuary.Census.Common.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanctuary.Census.Common.Services;

/// <inheritdoc />
public class DataContributorTypeRepository : IDataContributorTypeRepository
{
    private readonly Dictionary<int, List<Type>> _contributors;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataContributorTypeRepository"/> class.
    /// </summary>
    public DataContributorTypeRepository()
    {
        _contributors = new Dictionary<int, List<Type>>();
    }

    /// <inheritdoc />
    public void RegisterContributer<TContributor>(int order)
        where TContributor : class, IDataContributor
    {
        if (!_contributors.ContainsKey(order))
            _contributors.Add(order, new List<Type>());

        _contributors[order].Add(typeof(TContributor));
    }

    /// <inheritdoc />
    public IEnumerable<Type> GetContributors()
    {
        IEnumerable<int> orders = _contributors.Keys.OrderBy(o => o);
        return orders.SelectMany(o => _contributors[o]);
    }
}
