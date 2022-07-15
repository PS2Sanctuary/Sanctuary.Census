using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;
using FireGroup = Sanctuary.Zone.Packets.ReferenceData.FireGroup;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="FireGroupToFireMode"/> collection.
/// </summary>
public class FireGroupToFireModeCollectionBuilder : ICollectionBuilder
{
    /// <inheritdoc />
    public void Build
    (
        IClientDataCacheService clientDataCache,
        IServerDataCacheService serverDataCache,
        ILocaleService localeService,
        CollectionsContext context
    )
    {
        if (serverDataCache.WeaponDefinitions is null)
            throw new MissingCacheDataException(typeof(WeaponDefinitions));

        Dictionary<uint, List<FireGroupToFireMode>> builtMaps = new();
        foreach (FireGroup fireGroup in serverDataCache.WeaponDefinitions.FireGroups)
        {
            List<FireGroupToFireMode> map = new();

            for (uint i = 0; i < fireGroup.FireModes.Length; i++)
            {
                map.Add(new FireGroupToFireMode
                (
                    fireGroup.FireGroupID,
                    fireGroup.FireModes[i],
                    i
                ));
            }

            builtMaps.Add(fireGroup.FireGroupID, map);
        }

        context.FireGroupsToFireModes = builtMaps;
    }
}
