using Microsoft.Extensions.DependencyInjection;
using Sanctuary.Census.Abstractions.CollectionBuilders;
using System.Collections.Generic;

namespace Sanctuary.Census.Abstractions.Services;

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

    /// <summary>
    /// Checks that a collection exists.
    /// </summary>
    /// <param name="webName">The name of the collection, formatted as it would be in a query.</param>
    /// <returns><c>True</c> if the collection exists, else <c>False</c>.</returns>
    bool CheckCollectionExists(string webName);

    /// <summary>
    /// Checks that a field on a collection exists.
    /// </summary>
    /// <param name="collectionWebName">The name of the collection, formatted as it would be in a query.</param>
    /// <param name="fieldWebName">The name of the field, formatted as it would be in a query.</param>
    /// <returns><c>True</c> if the field exists, else <c>False</c>.</returns>
    bool CheckFieldExists(string collectionWebName, string fieldWebName);
}
