using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Exceptions;
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
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaponAmmoSlotCollectionBuilder"/> class.
    /// </summary>
    /// <param name="serverDataCache">The server data cache.</param>
    public WeaponAmmoSlotCollectionBuilder
    (
        IServerDataCacheService serverDataCache
    )
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

        List<WeaponAmmoSlot> builtAmmoSlots = new();
        foreach (WeaponDefinition weapon in _serverDataCache.WeaponDefinitions.Definitions)
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
