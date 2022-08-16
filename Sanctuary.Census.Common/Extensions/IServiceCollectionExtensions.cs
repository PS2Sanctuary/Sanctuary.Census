using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Common.Objects.DiffModels;
using Sanctuary.Census.Common.Services;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;

namespace Sanctuary.Census.Common.Extensions;

/// <summary>
/// Extension methods for the <see cref="IServiceCollection"/> type.
/// </summary>
public static class IServiceCollectionExtensions
{
    private static bool _classMapsRegistered;

    /// <summary>
    /// Adds common Sanctuary.Census services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance, so that calls may be chained.</returns>
    public static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IFileSystem, FileSystem>();
        services.TryAddScoped<EnvironmentContextProvider>();

        services.TryAddSingleton(new MongoClient("mongodb://localhost:27017"));
        services.TryAddScoped<IMongoContext, MongoContext>();
        RegisterCollectionClassMaps();

        services.AddHttpClient(nameof(ManifestService));
#if DEBUG
        services.TryAddSingleton<IManifestService, DebugManifestService>();
#else
        services.TryAddSingleton<IManifestService, CachingManifestService>();
#endif

        return services;
    }

    private static void RegisterCollectionClassMaps()
    {
        if (_classMapsRegistered)
            return;

        BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));

        IEnumerable<Type> collTypes = typeof(CollectionAttribute).Assembly
            .GetTypes()
            .Where(t => t.IsDefined(typeof(CollectionAttribute)));

        foreach (Type collType in collTypes)
            BsonClassMap.RegisterClassMap(MongoContext.AutoClassMap(collType));

        BsonClassMap.RegisterClassMap(MongoContext.AutoClassMap<CollectionDiffEntry>());
        BsonClassMap.RegisterClassMap(MongoContext.AutoClassMap<Datatype>());
        BsonClassMap.RegisterClassMap(MongoContext.AutoClassMap<DiffRecord>());
        BsonClassMap.RegisterClassMap(MongoContext.AutoClassMap<LocaleString>());
        BsonClassMap.RegisterClassMap(MongoContext.AutoClassMap<NewCollection>());

        _classMapsRegistered = true;
    }
}
