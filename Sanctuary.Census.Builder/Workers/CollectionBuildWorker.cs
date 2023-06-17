using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Objects;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Json;
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
    private readonly ILogger<CollectionBuildWorker> _logger;
    private readonly IOptionsMonitor<BuildOptions> _buildOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ICollectionBuilderRepository _collectionBuilderRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionBuildWorker"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="buildOptions">The build options to use.</param>
    /// <param name="serviceScopeFactory">The service provider.</param>
    /// <param name="collectionBuilderRepository">The collection builder repository.</param>
    public CollectionBuildWorker
    (
        ILogger<CollectionBuildWorker> logger,
        IOptionsMonitor<BuildOptions> buildOptions,
        IServiceScopeFactory serviceScopeFactory,
        ICollectionBuilderRepository collectionBuilderRepository
    )
    {
        _logger = logger;
        _buildOptions = buildOptions;
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

        while (!ct.IsCancellationRequested)
        {
            TimeSpan currentBuildInterval = TimeSpan.FromSeconds(_buildOptions.CurrentValue.BuildIntervalSeconds);

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
                ICollectionDiffService diffService = services.GetRequiredService<ICollectionDiffService>();

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
                }
                catch (LoginException lex)
                {
                    _logger.LogWarning("Login failure: {Message}", lex.Message);
                }
                catch (Exception ex)
                {
                    if (++dataCacheFailureCount >= 5)
                    {
                        _logger.LogCritical(ex, "Failed to cache data five times in a row! Collection builder is stopping");
                        return;
                    }

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

                await UpdateDatatypes(mongoContext, _buildOptions.CurrentValue.BuildIntervalSeconds, ct).ConfigureAwait(false);
                _logger.LogInformation("[{Environment}] Datatype upsert complete", env);

                try
                {
                    await diffService.CommitAsync(ct).ConfigureAwait(false);
                    _logger.LogInformation("[{Environment}] Collection diff committed", env);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[{Environment}] Failed to run the diff service", env);
                }

                clientDataCache.Clear();
                serverDataCache.Clear();
                localeDataCache.Clear();
                patchDataCache.Clear();
            }

            await Task.Delay(currentBuildInterval, ct).ConfigureAwait(false);
        }
    }

    private static async Task UpdateDatatypes(IMongoContext database, int updateIntervalSec, CancellationToken ct)
    {
        SnakeCaseJsonNamingPolicy nameConv = SnakeCaseJsonNamingPolicy.Default;
        IEnumerable<Type> collTypes = typeof(CollectionAttribute).Assembly
            .GetTypes()
            .Where(t => t.GetCustomAttribute<CollectionAttribute>()?.IsNestedType == false);

        IMongoCollection<Datatype> datatypeCollection = database.GetCollection<Datatype>();
        List<ReplaceOneModel<Datatype>> dbWriteModels = new();

        foreach (Type collType in collTypes)
        {
            string name = nameConv.ConvertName(collType.Name);
            long count = await database.GetCollection(collType)
                .CountDocumentsAsync(new BsonDocument(), cancellationToken: ct);

            long lastUpdated = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            int updateInterval = updateIntervalSec;

            if (collType.IsAssignableTo(typeof(IRealtimeCollection)))
            {
                lastUpdated = 0;
                updateInterval = 0;
            }

            Datatype type = new
            (
                name,
                collType.GetCustomAttribute<DescriptionAttribute>()?.Description,
                count,
                lastUpdated,
                updateInterval
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
