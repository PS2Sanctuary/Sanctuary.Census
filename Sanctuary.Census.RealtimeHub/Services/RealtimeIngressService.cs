using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Common.Objects.RealtimeEvents;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sanctuary.Census.RealtimeHub.Services;

/// <summary>
/// A service that handles gRPC calls for updating realtime data.
/// </summary>
[Authorize]
public class RealtimeIngressService : RealtimeIngress.RealtimeIngressBase
{
    private static readonly List<MapState> _mapStates;
    private static readonly Dictionary<uint, WorldPopulation> _worldPopulations;

    private readonly ILogger<RealtimeIngressService> _logger;
    private readonly EventStreamSocketManager _essManager;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    static RealtimeIngressService()
    {
        _mapStates = new List<MapState>();
        _worldPopulations = new Dictionary<uint, WorldPopulation>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RealtimeIngressService"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="essManager">The event stream manager.</param>
    /// <param name="serviceScopeFactory">The service scope factory.</param>
    public RealtimeIngressService
    (
        ILogger<RealtimeIngressService> logger,
        EventStreamSocketManager essManager,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _logger = logger;
        _essManager = essManager;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// Processes map state updates.
    /// </summary>
    /// <param name="request">The update request.</param>
    /// <param name="context">The context.</param>
    /// <returns>The result of the update.</returns>
    public override async Task<Ok> SubmitMapStateUpdate(MapStateUpdate request, ServerCallContext context)
    {
        _logger.LogDebug
        (
            "Received map state update for {ZoneId}:{ZoneInstance} on {WorldId} update with {RegionCount} regions",
            request.ZoneId,
            request.ZoneInstance,
            request.WorldId,
            request.Regions.Count
        );

        List<ReplaceOneModel<MapState>> replacementModels = new();
        List<MapState> changedStates = new();

        foreach (MapRegionState region in request.Regions)
        {
            bool isContested = region.ContestingFactionId is not (int)FactionDefinition.None
                || region.RemainingCaptureTimeMs > 0
                || region.RemainingCtfFlags < region.CtfFlags;

            ValueEqualityDictionary<FactionDefinition, int> popUpperBounds = new();
            ValueEqualityDictionary<FactionDefinition, byte> popPercentages = new();

            foreach ((int faction, int value) in region.FactionPopulationUpperBound)
                popUpperBounds.Add((FactionDefinition)faction, value);

            foreach ((int faction, int value) in region.FactionPopulationPercentage)
                popPercentages.Add((FactionDefinition)faction, (byte)value);

            MapState builtState = new
            (
                request.WorldId,
                (ushort)request.ZoneId,
                (ushort)request.ZoneInstance,
                request.Timestamp.Seconds,
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

            MapState? existingState = _mapStates.FirstOrDefault
            (
                ms => ms.WorldId == builtState.WorldId
                    && ms.ZoneId == builtState.ZoneId
                    && ms.ZoneInstance == builtState.ZoneInstance
                    && ms.MapRegionId == builtState.MapRegionId
            );

            MapState checkState = builtState with { Timestamp = 0 };
            if (existingState is null)
            {
                _mapStates.Add(checkState);
                changedStates.Add(builtState);
            }
            else if (!existingState.Equals(checkState))
            {
                _mapStates.Remove(existingState);
                _mapStates.Add(checkState);
                changedStates.Add(builtState);
            }
        }

        _logger.LogDebug("ESS Updating {Count} map states", changedStates.Count);
        foreach (MapState changedState in changedStates)
            _essManager.SubmitEvent(changedState);

        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        IMongoContext dbContext = scope.ServiceProvider.GetRequiredService<IMongoContext>();

        IMongoCollection<MapState> mapColl = dbContext.GetCollection<MapState>();
        await mapColl.BulkWriteAsync(replacementModels, null, context.CancellationToken);

        return new Ok();
    }

    /// <summary>
    /// Processes world population updates.
    /// </summary>
    /// <param name="request">The update request.</param>
    /// <param name="context">The context.</param>
    /// <returns>The result of the update.</returns>
    public override async Task<Ok> SubmitWorldPopulationUpdate(WorldPopulationUpdate request, ServerCallContext context)
    {
        _logger.LogDebug("Received world population update for {WorldId}", request.WorldId);

        int total = 0;
        ValueEqualityDictionary<FactionDefinition, int> pops = new();

        foreach ((int faction, int value) in request.FactionPopulations)
        {
            pops.Add((FactionDefinition)faction, value);
            total += value;
        }

        WorldPopulation worldPop = new
        (
            request.WorldId,
            request.Timestamp.Seconds,
            total,
            pops
        );
        WorldPopulation check = worldPop with { Timestamp = 0 };

        bool needsUpdate = !_worldPopulations.TryGetValue(worldPop.WorldId, out WorldPopulation? existingPop)
            || existingPop != check;
        if (needsUpdate)
        {
            _worldPopulations[worldPop.WorldId] = check;
            _essManager.SubmitEvent(worldPop);
        }

        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        IMongoContext dbContext = scope.ServiceProvider.GetRequiredService<IMongoContext>();

        IMongoCollection<WorldPopulation> popColl = dbContext.GetCollection<WorldPopulation>();
        await popColl.ReplaceOneAsync
        (
            Builders<WorldPopulation>.Filter.Eq(x => x.WorldId, worldPop.WorldId),
            worldPop,
            new ReplaceOptions { IsUpsert = true },
            context.CancellationToken
        );

        return new Ok();
    }
}
