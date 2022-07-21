using Microsoft.Extensions.DependencyInjection;
using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Services;
using Sanctuary.Census.Attributes;
using Sanctuary.Census.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sanctuary.Census.Services;

/// <inheritdoc />
public class CollectionBuilderRepository : ICollectionBuilderRepository
{
    private readonly HashSet<Type> _builders;
    private readonly Dictionary<string, HashSet<string>> _collectionInfos;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionBuilderRepository"/> class.
    /// </summary>
    public CollectionBuilderRepository()
    {
        _builders = new HashSet<Type>();
        _collectionInfos = new Dictionary<string, HashSet<string>>();

        IEnumerable<Type> collTypes = typeof(CollectionBuilderRepository).Assembly
            .GetTypes()
            .Where(t => t.IsDefined(typeof(CollectionAttribute)));

        foreach (Type collType in collTypes)
        {
            HashSet<string> propNames = new();
            _collectionInfos.Add
            (
                SnakeCaseJsonNamingPolicy.Default.ConvertName(collType.Name),
                propNames
            );

            foreach (PropertyInfo prop in collType.GetProperties())
                propNames.Add(SnakeCaseJsonNamingPolicy.Default.ConvertName(prop.Name));
        }
    }

    /// <inheritdoc />
    public void Register<TBuilder>()
        where TBuilder : class, ICollectionBuilder
        => _builders.Add(typeof(TBuilder));

    /// <inheritdoc />
    public IReadOnlyList<ICollectionBuilder> ConstructBuilders(IServiceScope scope)
        => _builders.Select(builderType => (ICollectionBuilder)scope.ServiceProvider.GetRequiredService(builderType))
            .ToList();

    /// <inheritdoc />
    public bool CheckCollectionExists(string webName)
        => _collectionInfos.ContainsKey(webName);

    /// <inheritdoc />
    public bool CheckFieldExists(string collectionWebName, string fieldWebName)
        => _collectionInfos.TryGetValue(collectionWebName, out HashSet<string>? fields)
           && fields.Contains(fieldWebName);
}
