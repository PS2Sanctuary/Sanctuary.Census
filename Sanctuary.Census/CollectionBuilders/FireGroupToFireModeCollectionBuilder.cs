using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FireGroup = Sanctuary.Zone.Packets.ReferenceData.FireGroup;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="FireGroupToFireMode"/> collection.
/// </summary>
public class FireGroupToFireModeCollectionBuilder : ICollectionBuilder
{
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="FireGroupToFireModeCollectionBuilder"/> class.
    /// </summary>
    /// <param name="serverDataCache">The server data cache.</param>
    public FireGroupToFireModeCollectionBuilder
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

        List<FireGroupToFireMode> builtMaps = new();
        foreach (FireGroup fireGroup in _serverDataCache.WeaponDefinitions.FireGroups)
        {
            for (uint i = 0; i < fireGroup.FireModes.Length; i++)
            {
                builtMaps.Add(new FireGroupToFireMode
                (
                    fireGroup.FireGroupID,
                    fireGroup.FireModes[i],
                    i
                ));
            }
        }

        await dbContext.UpsertFireGroupsToFireModesAsync(builtMaps, ct);
    }
}
