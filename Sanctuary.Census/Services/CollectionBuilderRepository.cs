using Microsoft.Extensions.DependencyInjection;
using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanctuary.Census.Services;

/// <inheritdoc />
public class CollectionBuilderRepository : ICollectionBuilderRepository
{
    private readonly HashSet<Type> _builders;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionBuilderRepository"/> class.
    /// </summary>
    public CollectionBuilderRepository()
    {
        _builders = new HashSet<Type>();
    }

    /// <inheritdoc />
    public void Register<TBuilder>()
        where TBuilder : class, ICollectionBuilder
        => _builders.Add(typeof(TBuilder));

    /// <inheritdoc />
    public IReadOnlyList<ICollectionBuilder> ConstructBuilders(IServiceScope scope)
        => _builders.Select(builderType => (ICollectionBuilder)scope.ServiceProvider.GetRequiredService(builderType))
            .ToList();
}
