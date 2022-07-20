using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="WeaponAmmoSlot"/> collection.
/// </summary>
public class WeaponAmmoSlotCollectionBuilder : ICollectionBuilder
{
    /// <inheritdoc />
    public async Task BuildAsync
    (
        IClientDataCacheService clientDataCache,
        IServerDataCacheService serverDataCache,
        ILocaleDataCacheService localeDataCache,
        IMongoContext dbContext,
        CancellationToken ct
    )
    {
        if (serverDataCache.WeaponDefinitions is null)
            throw new MissingCacheDataException(typeof(WeaponDefinitions));

        List<WeaponAmmoSlot> builtAmmoSlots = new();
        foreach (WeaponDefinition weapon in serverDataCache.WeaponDefinitions.Definitions)
        {
            for (uint i = 0; i < weapon.AmmoSlots.Length; i++)
            {
                WeaponDefinitionAmmoSlot slot = weapon.AmmoSlots[i];
                builtAmmoSlots.Add(new WeaponAmmoSlot
                (
                    weapon.WeaponID,
                    i,
                    slot.ClipSize,
                    slot.Capacity,
                    slot.ClipModelName.Length == 0 ? null : slot.ClipModelName
                ));
            }
        }

        await dbContext.UpsertWeaponAmmoSlotsAsync(builtAmmoSlots, ct);
    }
}
