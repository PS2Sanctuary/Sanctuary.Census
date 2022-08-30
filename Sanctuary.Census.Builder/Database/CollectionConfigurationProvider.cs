using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sanctuary.Census.Builder.Database;

/// <summary>
/// Represents a provider for <see cref="ICollectionDbConfiguration{TCollection}"/> instances.
/// </summary>
public class CollectionConfigurationProvider
{
    private readonly Dictionary<Type, ICollectionDbConfiguration<ISanctuaryCollection>> _configs;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionConfigurationProvider"/> class.
    /// </summary>
    public CollectionConfigurationProvider()
    {
        _configs = new Dictionary<Type, ICollectionDbConfiguration<ISanctuaryCollection>>();
    }

    /// <summary>
    /// Registers a configuration for the given collection type.
    /// This will override any previous registrations.
    /// </summary>
    /// <typeparam name="TCollection">The type of the collection to configure.</typeparam>
    /// <returns>The configuration.</returns>
    public ICollectionDbConfiguration<TCollection> Register<TCollection>()
        where TCollection : ISanctuaryCollection
    {
        CollectionDbConfiguration<TCollection> configuration = new();
        _configs[typeof(TCollection)] = Unsafe.As<ICollectionDbConfiguration<ISanctuaryCollection>>(configuration);
        return configuration;
    }

    /// <summary>
    /// Gets a configuration for the given collection type.
    /// </summary>
    /// <typeparam name="TCollection">The type of the collection.</typeparam>
    /// <returns>The registered configuration.</returns>
    /// <exception cref="ArgumentException">Thrown if a configuration has not been registered for the given type.</exception>
    public ICollectionDbConfiguration<TCollection> GetConfiguration<TCollection>()
        where TCollection : ISanctuaryCollection
    {
        if (!_configs.TryGetValue(typeof(TCollection), out ICollectionDbConfiguration<ISanctuaryCollection>? configuration))
            throw new ArgumentException("A configuration has not been registered for the given collection type");

        return Unsafe.As<ICollectionDbConfiguration<TCollection>>(configuration);
    }

    /// <summary>
    /// Gets all the registered configurations.
    /// </summary>
    /// <returns>The registered configurations.</returns>
    public IReadOnlyDictionary<Type, ICollectionDbConfiguration<ISanctuaryCollection>> GetAll()
        => _configs;
}
