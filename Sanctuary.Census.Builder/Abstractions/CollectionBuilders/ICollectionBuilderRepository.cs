using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Sanctuary.Census.Builder.Abstractions.CollectionBuilders;

/// <summary>
/// Represents a repository of <see cref="ICollectionBuilder"/> types,
/// and stores information about collections.
/// </summary>
public interface ICollectionBuilderRepository
{
    /// <summary>
    /// Registers a collection builder to the repository.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    void Register<TBuilder>() where TBuilder : class, ICollectionBuilder;

    /// <summary>
    /// Constructs the registered collection builders.
    /// </summary>
    /// <param name="scope"></param>
    /// <returns></returns>
    IReadOnlyList<ICollectionBuilder> ConstructBuilders(IServiceScope scope);
}
