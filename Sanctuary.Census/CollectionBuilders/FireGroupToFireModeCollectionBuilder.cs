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
using FireGroup = Sanctuary.Zone.Packets.ReferenceData.FireGroup;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="FireGroupToFireMode"/> collection.
/// </summary>
public class FireGroupToFireModeCollectionBuilder : ICollectionBuilder
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

        List<FireGroupToFireMode> builtMaps = new();
        foreach (FireGroup fireGroup in serverDataCache.WeaponDefinitions.FireGroups)
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
