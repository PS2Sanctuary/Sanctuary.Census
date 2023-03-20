using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
using System.Linq;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="ContinuousServerDataBuildWorker"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="continuousServerService">The continuous server data service.</param>
    /// <param name="serviceScopeFactory">The service provider.</param>
    public ContinuousServerDataBuildWorker
    (
        ILogger<ContinuousServerDataBuildWorker> logger,
        ContinuousServerDataService continuousServerService,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _logger = logger;
        _continuousServiceService = continuousServerService;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
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
    }

    private async Task ProcessUpdates(CancellationToken ct)
    {
        try
        {
            await Task.Yield();
            MapRegionPopulation? lastPopulation = null;

            await foreach (RealtimeDataWrapper data in _continuousServiceService.DataOutputReader.ReadAllAsync(ct))
            {
                try
                {
                    _logger.LogDebug("Received continuous data bundle for {Server}", data.Server);

                    await using AsyncServiceScope serviceScope = _serviceScopeFactory.CreateAsyncScope();
                    IServiceProvider services = serviceScope.ServiceProvider;
                    services.GetRequiredService<EnvironmentContextProvider>().Environment = PS2Environment.PS2;
                    IMongoContext dbContext = services.GetRequiredService<IMongoContext>();

                    // NOTE: Population is always sent before capture data

                    switch (data.Data)
                    {
                        case MapRegionPopulation mrp:
                        {
                            lastPopulation = mrp;
                            break;
                        }
                        case CaptureDataUpdateAll cdua when lastPopulation is not null:
                        {
                            if (cdua.ZoneID == lastPopulation.ZoneID && cdua.InstanceID == lastPopulation.InstanceID)
                                await ProcessMapCaptureDataAsync(data.Server, cdua, lastPopulation, dbContext, ct);
                            lastPopulation = null;
                            break;
                        }
                        case ServerPopulationInfo spi:
                        {
                            await ProcessWorldPopulation(data.Server, spi, dbContext, ct);
                            break;
                        }
                    }
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
        CaptureDataUpdateAll captureData,
        MapRegionPopulation populationData,
        IMongoContext dbContext,
        CancellationToken ct
    )
    {
        long buildTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        List<ReplaceOneModel<MapState>> replacementModels = new();

        Dictionary<uint, MapRegionPopulation_Region> popRegions = populationData.Regions
            .ToDictionary(x => x.RegionID, x => x);

        foreach (CaptureDataUpdateAll_Region region in captureData.Regions)
        {
            bool isContested = region.ContestingFactionId is not Sanctuary.Common.Objects.FactionDefinition.None
                || region.RemainingCaptureTimeMs > 0
                || region.RemainingCtfFlags < region.CtfFlags;

            ValueEqualityDictionary<FactionDefinition, int> popUpperBounds = new();
            ValueEqualityDictionary<FactionDefinition, byte> popPercentages = new();

            if (popRegions.TryGetValue(region.MapRegionId, out MapRegionPopulation_Region? population))
            {
                for (int i = 0; i < population.FactionPopTiers.Length; i++)
                {
                    FactionDefinition faction = (FactionDefinition)(i + 1);
                    byte tier = population.FactionPopTiers[i];

                    if (tier is 0)
                        popUpperBounds[faction] = 0;
                    else
                        popUpperBounds[faction] = (int)(12 * Math.Pow(2, tier - 1));
                }

                for (int i = 0; i < population.FactionPercentages.Length; i++)
                {
                    double percentage = population.FactionPercentages[i] / (double)byte.MaxValue;
                    popPercentages[(FactionDefinition)(i + 1)] = (byte)(percentage * 100);
                }
            }

            MapState builtState = new
            (
                (uint)server,
                (ushort)captureData.ZoneID,
                captureData.InstanceID,
                buildTime,
                region.MapRegionId,
                (byte)region.OwningFactionId,
                isContested,
                (byte)region.ContestingFactionId,
                region.CaptureTimeMs,
                region.RemainingCaptureTimeMs,
                region.CtfFlags,
                region.RemainingCtfFlags,
                popUpperBounds,
                popPercentages
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
}
