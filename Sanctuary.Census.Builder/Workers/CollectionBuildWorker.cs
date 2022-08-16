﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Services;
using Sanctuary.Census.PatchData.Abstractions.Services;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Census.ServerData.Internal.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.Workers;

/// <summary>
/// A service worker responsible for building the data collections.
/// </summary>
public class CollectionBuildWorker : BackgroundService
{
    private static readonly TimeSpan COLLECTION_BUILD_INTERVAL = TimeSpan.FromHours(3);

    private readonly ILogger<CollectionBuildWorker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ICollectionBuilderRepository _collectionBuilderRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionBuildWorker"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="serviceScopeFactory">The service provider.</param>
    /// <param name="collectionBuilderRepository">The collection builder repository.</param>
    public CollectionBuildWorker
    (
        ILogger<CollectionBuildWorker> logger,
        IServiceScopeFactory serviceScopeFactory,
        ICollectionBuilderRepository collectionBuilderRepository
    )
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _collectionBuilderRepository = collectionBuilderRepository;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // Ensure the DB structure is setup
        foreach (PS2Environment env in Enum.GetValues<PS2Environment>())
        {
            await using AsyncServiceScope serviceScope = _serviceScopeFactory.CreateAsyncScope();
            IServiceProvider services = serviceScope.ServiceProvider;

            services.GetRequiredService<EnvironmentContextProvider>().Environment = env;
            await services.GetRequiredService<ICollectionsContext>().ScaffoldAsync(ct).ConfigureAwait(false);
        }

        // TODO: Quick-update collections? E.g. the world collection could be updated every 5m to better represent lock state.
        int dataCacheFailureCount = 0;
        TimeSpan currentBuildInterval = COLLECTION_BUILD_INTERVAL;

        while (!ct.IsCancellationRequested)
        {
            foreach (PS2Environment env in Enum.GetValues<PS2Environment>())
            {
                if (ct.IsCancellationRequested)
                    break;

                await using AsyncServiceScope serviceScope = _serviceScopeFactory.CreateAsyncScope();
                IServiceProvider services = serviceScope.ServiceProvider;
                services.GetRequiredService<EnvironmentContextProvider>().Environment = env;

                IClientDataCacheService clientDataCache = services.GetRequiredService<IClientDataCacheService>();
                IServerDataCacheService serverDataCache = services.GetRequiredService<IServerDataCacheService>();
                ILocaleDataCacheService localeDataCache = services.GetRequiredService<ILocaleDataCacheService>();
                IPatchDataCacheService patchDataCache = services.GetRequiredService<IPatchDataCacheService>();
                IMongoContext mongoContext = services.GetRequiredService<IMongoContext>();
                ICollectionsContext collectionsContext = services.GetRequiredService<ICollectionsContext>();
                ICollectionDiffService collectionDiffService = services.GetRequiredService<ICollectionDiffService>();

                try
                {
                    _logger.LogDebug("[{Environment}] Populating client data cache...", env);
                    await clientDataCache.RepopulateAsync(ct).ConfigureAwait(false);
                    _logger.LogDebug("[{Environment}] Populating server data cache...", env);
                    await serverDataCache.RepopulateAsync(ct).ConfigureAwait(false);
                    _logger.LogDebug("[{Environment}] Populating locale data cache...", env);
                    await localeDataCache.RepopulateAsync(ct).ConfigureAwait(false);
                    _logger.LogDebug("[{Environment}] Populating patch data cache...", env);
                    await patchDataCache.RepopulateAsync(ct).ConfigureAwait(false);

                    dataCacheFailureCount = 0;
                    _logger.LogInformation("[{Environment}] Caches updated successfully", env);
                    currentBuildInterval = COLLECTION_BUILD_INTERVAL;
                }
                catch (ServerLockedException)
                {
                    // Probably down for an update. Let's get back in the game faster!
                    currentBuildInterval = TimeSpan.FromMinutes(30);
                    _logger.LogWarning("[{Environment}] Servers are locked. Collection build could not complete", env);
                    continue;
                }
                catch (NoCharactersOnAccountException ncex)
                {
                    _logger.LogError(ncex, "No characters on the account {Username}", ncex.AccountName);
                    continue;
                }
                catch (Exception ex)
                {
                    if (++dataCacheFailureCount >= 5)
                    {
                        _logger.LogCritical(ex, "Failed to cache data five times in a row! Collection builder is stopping");
                        return;
                    }

                    currentBuildInterval = COLLECTION_BUILD_INTERVAL;
                    _logger.LogError(ex, "[{Environment}] Failed to cache data!", env);
                    continue;
                }

                IReadOnlyList<ICollectionBuilder> collectionBuilders = _collectionBuilderRepository.ConstructBuilders(serviceScope);
                _logger.LogInformation("[{Environment}] Collection build starting...", env);
                foreach (ICollectionBuilder collectionBuilder in collectionBuilders)
                {
                    if (ct.IsCancellationRequested)
                        break;

                    try
                    {
                        await collectionBuilder.BuildAsync(collectionsContext,ct);
                        _logger.LogDebug("[{Environment}] Successfully ran the {CollectionBuilder}", env, collectionBuilder);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[{Environment}] Failed to run the {CollectionBuilder}", env, collectionBuilder);
                    }
                }
                _logger.LogInformation("[{Environment}] Collection build complete", env);

                await UpdateDatatypes(mongoContext, ct).ConfigureAwait(false);
                _logger.LogInformation("[{Environment}] Datatype upsert complete", env);

                await collectionDiffService.CommitAsync(ct).ConfigureAwait(false);
                _logger.LogInformation("[{Environment}] Collection diff committed", env);

                clientDataCache.Clear();
                serverDataCache.Clear();
                localeDataCache.Clear();
                patchDataCache.Clear();
            }

            await Task.Delay(currentBuildInterval, ct).ConfigureAwait(false);
        }
    }

    private static async Task UpdateDatatypes(IMongoContext database, CancellationToken ct)
    {
        SnakeCaseJsonNamingPolicy nameConv = SnakeCaseJsonNamingPolicy.Default;
        IEnumerable<Type> collTypes = typeof(CollectionAttribute).Assembly
            .GetTypes()
            .Where(t => t.IsDefined(typeof(CollectionAttribute)));

        IMongoCollection<Datatype> datatypeCollection = database.GetCollection<Datatype>();
        List<ReplaceOneModel<Datatype>> dbWriteModels = new();

        foreach (Type collType in collTypes)
        {
            string name = nameConv.ConvertName(collType.Name);
            long count = await database.GetCollection(collType)
                .CountDocumentsAsync(new BsonDocument(), cancellationToken: ct);

            Datatype type = new
            (
                name,
                collType.GetCustomAttribute<DescriptionAttribute>()?.Description,
                count,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            );

            ReplaceOneModel<Datatype> upsertModel = new
            (
                Builders<Datatype>.Filter.Eq(x => x.Name, name),
                type
            ) { IsUpsert = true };
            dbWriteModels.Add(upsertModel);
        }

        await datatypeCollection.BulkWriteAsync(dbWriteModels, null, ct);
    }
}