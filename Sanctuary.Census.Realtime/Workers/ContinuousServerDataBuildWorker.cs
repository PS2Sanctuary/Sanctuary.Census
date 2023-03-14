using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Common.Services;
using Sanctuary.Census.ServerData.Internal.Objects;
using Sanctuary.Census.ServerData.Internal.Services;
using Sanctuary.Common.Objects;
using Sanctuary.Zone.Packets.MapRegion;
using Sanctuary.Zone.Packets.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FactionDefinition = Sanctuary.Census.Common.Objects.CommonModels.FactionDefinition;

namespace Sanctuary.Census.Realtime.Workers;

/// <summary>
/// A service worker responsible for building collections that rely on continuous
/// server data.
/// </summary>
public sealed class ContinuousServerDataBuildWorker : BackgroundService
{
    private readonly ILogger<ContinuousServerDataBuildWorker> _logger;
    private readonly ContinuousServerDataService _continuousServiceService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly string _factionLimitsStoragePath;

    private Dictionary<ServerDefinition, Dictionary<ZoneDefinition, ushort[]>> _factionLimits;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContinuousServerDataBuildWorker"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="commonOptions">The common options to use.</param>
    /// <param name="continuousServerService">The continuous server data service.</param>
    /// <param name="serviceScopeFactory">The service provider.</param>
    public ContinuousServerDataBuildWorker
    (
        ILogger<ContinuousServerDataBuildWorker> logger,
        IOptions<CommonOptions> commonOptions,
        ContinuousServerDataService continuousServerService,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _logger = logger;
        _continuousServiceService = continuousServerService;
        _serviceScopeFactory = serviceScopeFactory;

        _factionLimits = new Dictionary<ServerDefinition, Dictionary<ZoneDefinition, ushort[]>>();
        _factionLimitsStoragePath = Path.Combine
        (
            commonOptions.Value.AppDataDirectory,
            "continuousPopZoneFactionLimits.json"
        );
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await TryLoadFactionLimits(ct);

        using CancellationTokenSource internalCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        using Task processTask = ProcessUpdates(internalCts.Token);
        using Task runTask = _continuousServiceService.RunAsync(internalCts.Token);

        await Task.WhenAny(processTask, runTask);
        internalCts.Cancel();

        try
        {
            await Task.WhenAll(processTask, runTask);
        }
        catch (AggregateException aex)
        {
            foreach (Exception ex in aex.InnerExceptions)
                _logger.LogError(ex, "Failed to process continuous server updates");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process continuous server updates");
        }

        await TrySaveFactionLimits(CancellationToken.None);
    }

    private async Task ProcessUpdates(CancellationToken ct)
    {
        try
        {
            await Task.Yield();

            await foreach (ContinuousDataBundle bundle in _continuousServiceService.DataOutputReader.ReadAllAsync(ct))
            {
                try
                {
                    _logger.LogDebug("Received continuous data bundle for {Server}", bundle.Server);

                    await using AsyncServiceScope serviceScope = _serviceScopeFactory.CreateAsyncScope();
                    IServiceProvider services = serviceScope.ServiceProvider;
                    services.GetRequiredService<EnvironmentContextProvider>().Environment = PS2Environment.PS2;
                    IMongoContext dbContext = services.GetRequiredService<IMongoContext>();

                    await ProcessMapCaptureDataAsync(bundle.Server, bundle.MapRegionCaptureData, dbContext, ct);

                    if (bundle.ServerPopulation is not null)
                        await ProcessWorldPopulation(bundle.Server, bundle.ServerPopulation, dbContext, ct);

                    if (bundle.ContinentInfo is not null)
                        await ProcessZonePopulation(bundle.Server, bundle.ContinentInfo, dbContext, ct);
                }
                catch (OperationCanceledException)
                {
                    // This is fine
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process continuous data bundle");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // This is fine
        }
    }

    private static async Task ProcessMapCaptureDataAsync
    (
        ServerDefinition server,
        IEnumerable<CaptureDataUpdateAll> captureData,
        IMongoContext dbContext,
        CancellationToken ct
    )
    {
        long buildTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        List<ReplaceOneModel<MapState>> replacementModels = new();

        foreach (CaptureDataUpdateAll data in captureData)
        {
            foreach (CaptureDataUpdateAll_Region region in data.Regions)
            {
                bool isContested = region.ContestingFactionId is not Sanctuary.Common.Objects.FactionDefinition.None
                    || region.RemainingCaptureTimeMs > 0
                    || region.RemainingCtfFlags < region.CtfFlags;

                MapState builtState = new
                (
                    (uint)server,
                    (ushort)data.ZoneID,
                    data.InstanceID,
                    buildTime,
                    region.MapRegionId,
                    (byte)region.OwningFactionId,
                    isContested,
                    (byte)region.ContestingFactionId,
                    region.CaptureTimeMs,
                    region.RemainingCaptureTimeMs,
                    region.CtfFlags,
                    region.RemainingCtfFlags
                );

                ReplaceOneModel<MapState> replacementModel = new
                (
                    Builders<MapState>.Filter.Eq(x => x.WorldId, builtState.WorldId)
                        & Builders<MapState>.Filter.Eq(x => x.ZoneId, builtState.ZoneId)
                        & Builders<MapState>.Filter.Eq(x => x.ZoneInstance, builtState.ZoneInstance)
                        & Builders<MapState>.Filter.Eq(x => x.MapRegionId, builtState.MapRegionId),
                    builtState
                ) { IsUpsert = true };
                replacementModels.Add(replacementModel);
            }
        }

        if (replacementModels.Count > 0)
        {
            IMongoCollection<MapState> mapColl = dbContext.GetCollection<MapState>();
            await mapColl.BulkWriteAsync(replacementModels, null, ct);
        }
    }

    private static async Task ProcessWorldPopulation
    (
        ServerDefinition server,
        ServerPopulationInfo spi,
        IMongoContext dbContext,
        CancellationToken ct
    )
    {
        ValueEqualityDictionary<FactionDefinition, int> pops = new();
        int total = 0;

        for (int i = 0; i < spi.Population.Length; i++)
        {
            ushort value = spi.Population[i];
            pops[(FactionDefinition)i + 1] = value;
            total += value;
        }

        WorldPopulation worldPop = new
        (
            (uint)server,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            total,
            pops
        );

        IMongoCollection<WorldPopulation> popColl = dbContext.GetCollection<WorldPopulation>();
        await popColl.ReplaceOneAsync
        (
            Builders<WorldPopulation>.Filter.Eq(x => x.WorldId, worldPop.WorldId),
            worldPop,
            new ReplaceOptions { IsUpsert = true },
            ct
        );
    }

    private async Task ProcessZonePopulation
    (
        ServerDefinition server,
        ContinentBattleInfo cbi,
        IMongoContext dbContext,
        CancellationToken ct
    )
    {
        List<ReplaceOneModel<ZonePopulation>> replacementModels = new();

        foreach (ContinentBattleInfo_ZoneData zone in cbi.Zones)
        {
            // Only cache faction limits if there is nobody on the zone and the limits are equal to each other
            if (zone.PopulationPercent.All(x => x == 0) && zone.RemainingCharacterLimit.Distinct().Count() is 1)
            {
                _factionLimits.TryAdd(server, new Dictionary<ZoneDefinition, ushort[]>());
                _factionLimits[server][zone.ZoneID] = zone.RemainingCharacterLimit;
            }

            if (!_factionLimits.ContainsKey(server))
                continue;
            if (!_factionLimits[server].TryGetValue(zone.ZoneID, out ushort[]? zoneFactionLimits))
                continue;

            ValueEqualityDictionary<FactionDefinition, int> pops = new();
            int total = 0;

            for (int i = 0; i < 3; i++)
            {
                int value = zoneFactionLimits[i] - zone.RemainingCharacterLimit[i];
                pops[(FactionDefinition)i + 1] = value;
                total += value;
            }

            ZonePopulation zonePop = new
            (
                (uint)server,
                (ushort)zone.ZoneID,
                zone.Instance,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                total,
                pops
            );

            ReplaceOneModel<ZonePopulation> replacementModel = new
            (
                Builders<ZonePopulation>.Filter.Eq(x => x.WorldId, (uint)server)
                    & Builders<ZonePopulation>.Filter.Eq(x => x.ZoneId, (ushort)zone.ZoneID)
                    & Builders<ZonePopulation>.Filter.Eq(x => x.ZoneInstance, zone.Instance),
                zonePop
            ) { IsUpsert = true };
            replacementModels.Add(replacementModel);
        }

        if (replacementModels.Count > 0)
        {
            IMongoCollection<ZonePopulation> zonePopColl = dbContext.GetCollection<ZonePopulation>();
            await zonePopColl.BulkWriteAsync(replacementModels, null, ct);
        }
    }

    private async Task TryLoadFactionLimits(CancellationToken ct)
    {
        if (!File.Exists(_factionLimitsStoragePath))
            return;

        try
        {
            await using FileStream fs = new(_factionLimitsStoragePath, FileMode.Open, FileAccess.Read);
            _factionLimits = await JsonSerializer.DeserializeAsync<Dictionary<ServerDefinition, Dictionary<ZoneDefinition, ushort[]>>>
            (
                fs,
                cancellationToken: ct
            ) ?? new Dictionary<ServerDefinition, Dictionary<ZoneDefinition, ushort[]>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load faction limits");
        }
    }

    private async Task TrySaveFactionLimits(CancellationToken ct)
    {
        try
        {
            await using FileStream fs = new(_factionLimitsStoragePath, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(fs, _factionLimits, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save faction limits");
        }
    }
}
