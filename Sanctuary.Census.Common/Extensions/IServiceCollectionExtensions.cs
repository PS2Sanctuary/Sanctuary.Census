using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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
    private static bool _registrationComplete;

    /// <summary>
    /// Adds common Sanctuary.Census services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance, so that calls may be chained.</returns>
    /// <param name="hostEnvironment">The current host environment.</param>
    public static IServiceCollection AddCommonServices(this IServiceCollection services, IHostEnvironment hostEnvironment)
    {
        if (_registrationComplete)
            return services;

        services.AddMemoryCache();

        services.TryAddSingleton<IFileSystem, FileSystem>();
        services.TryAddScoped<EnvironmentContextProvider>();

        MongoClientSettings mongoSettings = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
        services.TryAddSingleton(new MongoClient(mongoSettings));
        services.TryAddScoped<IMongoContext, MongoContext>();
        RegisterCollectionClassMaps();

        services.AddHttpClient<Mandible.Abstractions.Manifest.IManifestService, Mandible.Manifest.ManifestService>();
        if (hostEnvironment.IsProduction())
            services.TryAddScoped<IManifestService, CachingManifestService>();
        else
            services.TryAddScoped<IManifestService, DebugManifestService>();

        _registrationComplete = true;
        return services;
    }

    private static void RegisterCollectionClassMaps()
    {
        BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));
        BsonSerializer.RegisterSerializer(new EnumSerializer<FactionDefinition>(BsonType.String));

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
    }
}
