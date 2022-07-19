using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.CollectionBuilders;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Common.Services;
using Sanctuary.Census.Json;
using Sanctuary.Census.Models.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Workers;

/// <summary>
/// A service worker responsible for building the data collections.
/// </summary>
public class CollectionBuildWorker : BackgroundService
{
    private static readonly JsonNamingPolicy DbNameConverter = new SnakeCaseJsonNamingPolicy();

    private readonly ILogger<CollectionBuildWorker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly CommonOptions _options;
    private readonly CollectionsContext _collectionsContext;
    private readonly MongoClient _mongoClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionBuildWorker"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="serviceScopeFactory">The service provider.</param>
    /// <param name="options">The configured common options.</param>
    /// <param name="collectionsContext">The collections context.</param>
    /// <param name="mongoClient">The Mongo DB driver client.</param>
    public CollectionBuildWorker
    (
        ILogger<CollectionBuildWorker> logger,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<CommonOptions> options,
        CollectionsContext collectionsContext,
        MongoClient mongoClient
    )
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _options = options.Value;
        _collectionsContext = collectionsContext;
        _mongoClient = mongoClient;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // TODO: Quick-update collections? E.g. the world collection could be updated every 5m to better represent lock state.
        int dataCacheFailureCount = 0;

        while (!ct.IsCancellationRequested)
        {
            await using AsyncServiceScope serviceScope = _serviceScopeFactory.CreateAsyncScope();
            IServiceProvider services = serviceScope.ServiceProvider;
            services.GetRequiredService<EnvironmentContextProvider>().Environment = _options.DataSourceEnvironment;

            IClientDataCacheService _clientDataCache = services.GetRequiredService<IClientDataCacheService>();
            IServerDataCacheService _serverDataCache = services.GetRequiredService<IServerDataCacheService>();
            ILocaleDataCacheService localeDataCache = services.GetRequiredService<ILocaleDataCacheService>();

            try
            {
                _logger.LogDebug("Populating client data cache...");
                await _clientDataCache.RepopulateAsync(ct).ConfigureAwait(false);
                _logger.LogDebug("Populating server data cache...");
                await _serverDataCache.RepopulateAsync(ct).ConfigureAwait(false);
                _logger.LogDebug("Populating locale data cache...");
                await localeDataCache.RepopulateAsync(ct).ConfigureAwait(false);
                dataCacheFailureCount = 0;
                _logger.LogInformation("Caches updated successfully!");
            }
            catch (Exception ex)
            {
                if (++dataCacheFailureCount >= 5)
                {
                    _logger.LogCritical(ex, "Failed to cache data five times in a row! Collection builder is stopping");
                    return;
                }

                _logger.LogError(ex, "Failed to cache data. Will retry in 15s...");
                await Task.Delay(TimeSpan.FromSeconds(15), ct).ConfigureAwait(false);
                continue;
            }

            ICollectionBuilder[] collectionBuilders =
            {
                new CurrencyCollectionBuilder(),
                new ExperienceCollectionBuilder(),
                new FactionCollectionBuilder(),
                new FireGroupCollectionBuilder(),
                new FireGroupToFireModeCollectionBuilder(),
                new FireModeCollectionBuilder(),
                new FireModeToProjectileCollectionBuilder(),
                new ItemCollectionBuilder(),
                new ItemCategoryCollectionBuilder(),
                new ItemToWeaponCollectionBuilder(),
                new PlayerStateGroup2CollectionBuilder(),
                new ProfileCollectionBuilder(),
                new ProjectileCollectionBuilder(),
                new WeaponCollectionBuilder(),
                new WeaponAmmoSlotCollectionBuilder(),
                new WeaponToFireGroupCollectionBuilder(),
                new WorldCollectionBuilder()
            };

            foreach (ICollectionBuilder collectionBuilder in collectionBuilders)
            {
                try
                {
                    collectionBuilder.Build(_clientDataCache, _serverDataCache, localeDataCache, _collectionsContext);
                    _logger.LogDebug("Successfully ran the {CollectionBuilder}", collectionBuilder);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to run the {CollectionBuilder}", collectionBuilder);
                }
            }
            _collectionsContext.BuildCollectionInfos();
            _logger.LogInformation("Collections build complete");

            SnakeCaseJsonNamingPolicy namePolicy = new();
            void AutoMap<T>(BsonClassMap<T> cm)
            {
                foreach (PropertyInfo prop in typeof(T).GetProperties())
                    cm.MapProperty(prop.Name).SetElementName(namePolicy.ConvertName(prop.Name));
            }

            BsonClassMap.RegisterClassMap<LocaleString>(cm =>
            {
                cm.UnmapProperty(x => x.ID);
                cm.MapProperty(x => x.German).SetElementName("de");
                cm.MapProperty(x => x.English).SetElementName("en");
                cm.MapProperty(x => x.Spanish).SetElementName("es");
                cm.MapProperty(x => x.French).SetElementName("fr");
                cm.MapProperty(x => x.Italian).SetElementName("it");
                cm.MapProperty(x => x.Korean).SetElementName("ko");
                cm.MapProperty(x => x.Portuguese).SetElementName("pt");
                cm.MapProperty(x => x.Russian).SetElementName("ru");
                cm.MapProperty(x => x.Turkish).SetElementName("tr");
                cm.MapProperty(x => x.Chinese).SetElementName("zh");
            });

            IMongoDatabase db = _mongoClient.GetDatabase(_options.DataSourceEnvironment + "-collections");

            BsonClassMap.RegisterClassMap<Currency>(AutoMap);
            await UpdateDbCollectionAsync
            (
                db,
                _collectionsContext.Currencies.Values,
                e => Builders<Currency>.Filter.Where(x => x.CurrencyID == e.CurrencyID),
                ct,
                new CreateIndexModel<Currency>
                (
                    Builders<Currency>.IndexKeys.Ascending(x => x.CurrencyID),
                    new CreateIndexOptions { Unique = true }
                )
            );

            BsonClassMap.RegisterClassMap<FireGroup>(AutoMap);
            await UpdateDbCollectionAsync
            (
                db,
                _collectionsContext.FireGroups.Values,
                e => Builders<FireGroup>.Filter.Where(x => x.FireGroupID == e.FireGroupID),
                ct,
                new CreateIndexModel<FireGroup>
                (
                    Builders<FireGroup>.IndexKeys.Ascending(x => x.FireGroupID),
                    new CreateIndexOptions { Unique = true }
                )
            );

            BsonClassMap.RegisterClassMap<FireGroupToFireMode>(AutoMap);
            await UpdateDbCollectionAsync
            (
                db,
                _collectionsContext.FireGroupsToFireModes.Values.SelectMany(x => x),
                e => Builders<FireGroupToFireMode>.Filter.Where(x => x.FireGroupId == e.FireGroupId && x.FireModeId == e.FireModeId),
                ct,
                new [] {
                    new CreateIndexModel<FireGroupToFireMode>(Builders<FireGroupToFireMode>.IndexKeys.Ascending(x => x.FireGroupId)),
                    new CreateIndexModel<FireGroupToFireMode>(Builders<FireGroupToFireMode>.IndexKeys.Ascending(x => x.FireModeId))
                }
            );

            _clientDataCache.Clear();
            _serverDataCache.Clear();
            localeDataCache.Clear();
            _logger.LogDebug("Data caches cleared");

            await Task.Delay(TimeSpan.FromHours(1), ct).ConfigureAwait(false);
        }
    }

    private static async Task UpdateDbCollectionAsync<T>
    (
        IMongoDatabase database,
        IEnumerable<T> data,
        Func<T, FilterDefinition<T>> elementFilter,
        CancellationToken ct,
        CreateIndexModel<T>? indexModel = null
    ) => await UpdateDbCollectionAsync
    (
        database,
        data,
        elementFilter,
        ct,
        indexModel is null
            ? null
            : new[] { indexModel }
    );

    private static async Task UpdateDbCollectionAsync<T>
    (
        IMongoDatabase database,
        IEnumerable<T> data,
        Func<T, FilterDefinition<T>> elementFilter,
        CancellationToken ct,
        IEnumerable<CreateIndexModel<T>>? indexModels = null
    )
    {
        IMongoCollection<T> collection = database.GetCollection<T>(DbNameConverter.ConvertName(typeof(T).Name));

        if (indexModels is not null)
            await collection.Indexes.CreateManyAsync(indexModels, ct);

        List<WriteModel<T>> upserts = new();
        foreach (T element in data)
        {
            ReplaceOneModel<T> upsertModel = new(elementFilter(element), element)
            {
                IsUpsert = true
            };
            upserts.Add(upsertModel);
        }

        await collection.BulkWriteAsync(upserts, null, ct);
    }
}
