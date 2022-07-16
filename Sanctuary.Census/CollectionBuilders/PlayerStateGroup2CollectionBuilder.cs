using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="PlayerStateGroup2"/> collection.
/// </summary>
public class PlayerStateGroup2CollectionBuilder : ICollectionBuilder
{
    /// <inheritdoc />
    public void Build
    (
        IClientDataCacheService clientDataCache,
        IServerDataCacheService serverDataCache,
        ILocaleDataCacheService localeDataCache,
        CollectionsContext context
    )
    {
        if (serverDataCache.WeaponDefinitions is null)
            throw new MissingCacheDataException(typeof(WeaponDefinitions));

        Dictionary<uint, IReadOnlyList<PlayerStateGroup2>> builtStateGroups = new();
        foreach (PlayerStateGroup stateGroup in serverDataCache.WeaponDefinitions.PlayerStateGroups)
        {
            List<PlayerStateGroup2> groups = new();

            for (uint i = 0; i < stateGroup.States.Length; i++)
            {
                PlayerState state = stateGroup.States[i];
                groups.Add(new PlayerStateGroup2
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

            builtStateGroups.Add(stateGroup.PlayerStateGroupID, groups);
        }

        context.PlayerStateGroups = builtStateGroups;
    }
}
