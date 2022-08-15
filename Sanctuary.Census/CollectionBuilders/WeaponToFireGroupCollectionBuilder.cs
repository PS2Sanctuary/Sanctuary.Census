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
/// Builds the <see cref="WeaponToFireGroup"/> collection.
/// </summary>
public class WeaponToFireGroupCollectionBuilder : ICollectionBuilder
{
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaponToFireGroupCollectionBuilder"/> class.
    /// </summary>
    /// <param name="serverDataCache">The server data cache.</param>
    public WeaponToFireGroupCollectionBuilder(IServerDataCacheService serverDataCache)
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

        List<WeaponToFireGroup> builtMaps = new();
        foreach (WeaponDefinition weapon in _serverDataCache.WeaponDefinitions.Definitions)
        {
            for (uint i = 0; i < weapon.FireGroups.Length; i++)
            {
                builtMaps.Add(new WeaponToFireGroup
                (
                    weapon.WeaponID,
                    weapon.FireGroups[i],
                    i
                ));
            }
        }

        await dbContext.UpsertWeaponsToFireGroupsAsync(builtMaps, ct);
    }
}
