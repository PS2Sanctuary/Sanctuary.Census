using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="WeaponAmmoSlot"/> collection.
/// </summary>
public class WeaponAmmoSlotCollectionBuilder : ICollectionBuilder
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

        Dictionary<uint, IReadOnlyList<WeaponAmmoSlot>> builtAmmoSlots = new();
        foreach (WeaponDefinition weapon in serverDataCache.WeaponDefinitions.Definitions)
        {
            List<WeaponAmmoSlot> map = new();

            for (uint i = 0; i < weapon.AmmoSlots.Length; i++)
            {
                WeaponDefinitionAmmoSlot slot = weapon.AmmoSlots[i];
                map.Add(new WeaponAmmoSlot
                (
                    weapon.WeaponID,
                    i,
                    slot.ClipSize,
                    slot.Capacity,
                    slot.ClipModelName.Length == 0 ? null : slot.ClipModelName
                ));
            }

            builtAmmoSlots.Add(weapon.WeaponID, map);
        }

        context.WeaponAmmoSlots = builtAmmoSlots;
    }
}
