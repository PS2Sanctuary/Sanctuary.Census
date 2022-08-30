using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sanctuary.Census.Builder.Database;

/// <summary>
/// Represents a provider for <see cref="CollectionDbConfiguration{TCollection}"/> instances.
/// </summary>
public class CollectionConfigurationProvider
{
    private readonly Dictionary<Type, CollectionDbConfiguration<ISanctuaryCollection>> _configs;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionConfigurationProvider"/> class.
    /// </summary>
    public CollectionConfigurationProvider()
    {
        _configs = new Dictionary<Type, CollectionDbConfiguration<ISanctuaryCollection>>();
    }

    /// <summary>
    /// Registers a configuration for the given collection type.
    /// This will override any previous registrations.
    /// </summary>
    /// <typeparam name="TCollection">The type of the collection to configure.</typeparam>
    /// <returns>The configuration.</returns>
    public CollectionDbConfiguration<TCollection> Register<TCollection>()
        where TCollection : ISanctuaryCollection
    {
        CollectionDbConfiguration<TCollection> configuration = new();
        _configs[typeof(TCollection)] = Unsafe.As<CollectionDbConfiguration<ISanctuaryCollection>>(configuration);
        return configuration;
    }

    /// <summary>
    /// Gets a configuration for the given collection type.
    /// </summary>
    /// <typeparam name="TCollection">The type of the collection.</typeparam>
    /// <returns>The registered configuration.</returns>
    /// <exception cref="ArgumentException">Thrown if a configuration has not been registered for the given type.</exception>
    public CollectionDbConfiguration<TCollection> GetConfiguration<TCollection>()
        where TCollection : ISanctuaryCollection
    {
        if (!_configs.TryGetValue(typeof(TCollection), out CollectionDbConfiguration<ISanctuaryCollection>? configuration))
            throw new ArgumentException("A configuration has not been registered for the given collection type");

        return Unsafe.As<CollectionDbConfiguration<TCollection>>(configuration);
    }

    /// <summary>
    /// Gets all the registered configurations.
    /// </summary>
    /// <returns>The registered configurations.</returns>
    public IReadOnlyDictionary<Type, CollectionDbConfiguration<ISanctuaryCollection>> GetAll()
        => _configs;
}
