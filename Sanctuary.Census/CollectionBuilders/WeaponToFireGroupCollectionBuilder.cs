using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="WeaponToFireGroup"/> collection.
/// </summary>
public class WeaponToFireGroupCollectionBuilder : ICollectionBuilder
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

        Dictionary<uint, IReadOnlyList<WeaponToFireGroup>> builtMaps = new();
        foreach (WeaponDefinition weapon in serverDataCache.WeaponDefinitions.Definitions)
        {
            List<WeaponToFireGroup> map = new();

            for (uint i = 0; i < weapon.FireGroups.Length; i++)
            {
                map.Add(new WeaponToFireGroup
                (
                    weapon.WeaponID,
                    weapon.FireGroups[i],
                    i
                ));
            }

            builtMaps.Add(weapon.WeaponID, map);
        }

        context.WeaponToFireGroup = builtMaps;
    }
}
