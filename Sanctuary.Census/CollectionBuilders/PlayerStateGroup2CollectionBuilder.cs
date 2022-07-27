using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="PlayerStateGroup2"/> collection.
/// </summary>
public class PlayerStateGroup2CollectionBuilder : ICollectionBuilder
{
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerStateGroup2CollectionBuilder"/> class.
    /// </summary>
    /// <param name="serverDataCache">The server data cache.</param>
    public PlayerStateGroup2CollectionBuilder(IServerDataCacheService serverDataCache)
    {
        _serverDataCache = serverDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync
    (
        ICollectionsContext dbContext,
        CancellationToken ct
    )
    {
        if (_serverDataCache.WeaponDefinitions is null)
            throw new MissingCacheDataException(typeof(WeaponDefinitions));

        List<PlayerStateGroup2> builtStateGroups = new();
        foreach (PlayerStateGroup stateGroup in _serverDataCache.WeaponDefinitions.PlayerStateGroups)
        {
            for (uint i = 0; i < stateGroup.States.Length; i++)
            {
                PlayerState state = stateGroup.States[i];
                builtStateGroups.Add(new PlayerStateGroup2
                (
                    stateGroup.PlayerStateGroupID,
                    state.PlayerStateID,
                    state.CanIronSight,
                    state.CofGrowRate,
                    state.CofMax,
                    state.CofMin,
                    state.CofRecoveryDelayMs,
                    state.CofRecoveryRate,
                    state.CofShotsBeforePenalty,
                    state.CofRecoveryDelayThreshold,
                    state.CofTurnPenalty
                ));
            }
        }

        await dbContext.UpsertPlayerStateGroup2Async(builtStateGroups, ct);
    }
}
