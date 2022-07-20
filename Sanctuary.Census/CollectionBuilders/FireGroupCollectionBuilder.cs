using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MFireGroup = Sanctuary.Census.Models.Collections.FireGroup;
using SFireGroup = Sanctuary.Zone.Packets.ReferenceData.FireGroup;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MFireGroup"/> collection.
/// </summary>
public class FireGroupCollectionBuilder : ICollectionBuilder
{
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="FireGroupCollectionBuilder"/> class.
    /// </summary>
    /// <param name="serverDataCache">The server data cache.</param>
    public FireGroupCollectionBuilder
    (
        IServerDataCacheService serverDataCache
    )
    {
        _serverDataCache = serverDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync
    (
        IMongoContext dbContext,
        CancellationToken ct
    )
    {
        if (_serverDataCache.WeaponDefinitions is null)
            throw new MissingCacheDataException(typeof(WeaponDefinitions));

        Dictionary<uint, MFireGroup> builtFireGroups = new();
        foreach (SFireGroup fireGroup in _serverDataCache.WeaponDefinitions.FireGroups)
        {
            MFireGroup built = new
            (
                fireGroup.FireGroupID,
                fireGroup.ChamberDurationMs == 0 ? null : fireGroup.ChamberDurationMs,
                fireGroup.TransitionDurationMs == 0 ? null : fireGroup.TransitionDurationMs,
                fireGroup.SpoolUpTimeMs == 0 ? null : fireGroup.SpoolUpTimeMs,
                fireGroup.SpoolUpInitialRefireMs == 0 ? null : fireGroup.SpoolUpInitialRefireMs,
                (fireGroup.Flags & FireGroupFlags.CanChamberIronSights) != 0
            );
            builtFireGroups.TryAdd(built.FireGroupID, built);
        }

        await dbContext.UpsertFireGroupsAsync(builtFireGroups.Values, ct);
    }
}
