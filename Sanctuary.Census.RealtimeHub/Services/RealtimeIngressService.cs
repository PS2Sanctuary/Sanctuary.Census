using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Common.Objects.RealtimeEvents;
using Sanctuary.Census.RealtimeHub.Objects;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MOutfitWarRegistration = Sanctuary.Census.Common.Objects.Collections.OutfitWarRegistration;

namespace Sanctuary.Census.RealtimeHub.Services;

/// <summary>
/// A service that handles gRPC calls for updating realtime data.
/// </summary>
[Authorize]
public class RealtimeIngressService : RealtimeIngress.RealtimeIngressBase
{
    private readonly SemaphoreSlim _mapStateUpdateLock;
    private static readonly Dictionary<object, MapState> _mapStates;
    private static readonly ConcurrentDictionary<uint, WorldPopulation> _worldPopulations;

    private readonly ILogger<RealtimeIngressService> _logger;
    private readonly IOptionsMonitor<ZoneConnectionOptions> _zoneConnectionOptions;
    private readonly EventStreamSocketManager _essManager;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    static RealtimeIngressService()
    {
        _mapStates = new Dictionary<object, MapState>();
        _worldPopulations = new ConcurrentDictionary<uint, WorldPopulation>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RealtimeIngressService"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="zoneConnectionOptions">The zone connection options.</param>
    /// <param name="essManager">The event stream manager.</param>
    /// <param name="serviceScopeFactory">The service scope factory.</param>
    public RealtimeIngressService
    (
        ILogger<RealtimeIngressService> logger,
        IOptionsMonitor<ZoneConnectionOptions> zoneConnectionOptions,
        EventStreamSocketManager essManager,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _logger = logger;
        _zoneConnectionOptions = zoneConnectionOptions;
        _essManager = essManager;
        _serviceScopeFactory = serviceScopeFactory;

        _mapStateUpdateLock = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    /// Processes a zone connection request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="context">The context.</param>
    /// <returns>A zone connection response.</returns>
    public override Task<ZoneConnectionResponse> RequestZoneConnection
    (
        ZoneConnectionRequest request,
        ServerCallContext context
    )
    {
        ZoneConnectionOptions options = _zoneConnectionOptions.CurrentValue;

        return Task.FromResult(new ZoneConnectionResponse
        {
            ClientVersion = options.ClientVersion,
            ClientProtocolString = options.ClientProtocolString
        });
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

        await _mapStateUpdateLock.WaitAsync(context.CancellationToken);
        List<ReplaceOneModel<MapState>> replacementModels = [];
        List<MapState> changedStates = [];

        foreach (MapRegionState region in request.Regions)
        {
            bool isContested = region.ContestingFactionId is not (int)FactionDefinition.None
                || region.RemainingCaptureTimeMs > 0
                || region.RemainingCtfFlags < region.CtfFlags;

            ValueEqualityDictionary<FactionDefinition, int> popUpperBounds = new();
            ValueEqualityDictionary<FactionDefinition, float> popPercentages = new();

            foreach ((int faction, int value) in region.FactionPopulationUpperBound)
                popUpperBounds.Add((FactionDefinition)faction, value);

            foreach ((int faction, float value) in region.FactionPopulationPercentage)
                popPercentages.Add((FactionDefinition)faction, value);

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

            // The timestamp is ALWAYS going to change, but we want to do a record comparison.
            // Hence, we'll just store and compare on a state without the timestamp.
            MapState checkState = builtState with { Timestamp = 0 };
            (uint, ushort, ushort, uint) key = (builtState.WorldId, builtState.ZoneId, builtState.ZoneInstance, builtState.MapRegionId);

            // TODO: The comparison here isn't working
            if (_mapStates.TryGetValue(key, out MapState? existingState) && !existingState.Equals(checkState))
            {
                _mapStates[key] = checkState;
                changedStates.Add(builtState);
            }
            else
            {
                _mapStates[key] = checkState;
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

        _mapStateUpdateLock.Release();
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

    /// <summary>
    /// Processes outfit war registration updates.
    /// </summary>
    /// <param name="request">The update request.</param>
    /// <param name="context">The context.</param>
    /// <returns>The result of the update.</returns>
    public override async Task<Ok> SubmitOutfitWarRegistrationsUpdate
    (
        OutfitWarRegistrationsUpdate request,
        ServerCallContext context
    )
    {
        List<ReplaceOneModel<MOutfitWarRegistration>> replacementModels = new();

        foreach (OutfitWarRegistration reg in request.Registrations)
        {
            MOutfitWarRegistration builtReg = new
            (
                reg.OutfitId,
                reg.FactionId,
                request.WorldId,
                request.OutfitWarId,
                reg.RegistrationOrder,
                (Common.Objects.Collections.OutfitWarRegistration.RegistrationStatus)reg.Status,
                reg.MemberSignupCount
            );

            ReplaceOneModel<MOutfitWarRegistration> replacementModel = new
            (
                Builders<MOutfitWarRegistration>.Filter.Eq(x => x.OutfitID, builtReg.OutfitID),
                builtReg
            ) { IsUpsert = true };
            replacementModels.Add(replacementModel);
        }

        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        IMongoContext dbContext = scope.ServiceProvider.GetRequiredService<IMongoContext>();

        IMongoCollection<MOutfitWarRegistration> mapColl = dbContext.GetCollection<MOutfitWarRegistration>();
        await mapColl.BulkWriteAsync(replacementModels, null, context.CancellationToken);

        return new Ok();
    }
}
