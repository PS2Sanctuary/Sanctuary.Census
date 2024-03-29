﻿using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MFireGroup = Sanctuary.Census.Common.Objects.Collections.FireGroup;
using SFireGroup = Sanctuary.Zone.Packets.ReferenceData.FireGroup;

namespace Sanctuary.Census.Builder.CollectionBuilders;

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
        ICollectionsContext dbContext,
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
                fireGroup.ChamberDurationMs.ToNullableUShort(),
                fireGroup.TransitionDurationMs.ToNullableUShort(),
                fireGroup.SpinUpTimeMs.ToNullableUShort(),
                fireGroup.SpoolUpTimeMs.ToNullableUShort(),
                fireGroup.SpoolUpInitialRefireMs.ToNullableUShort(),
                fireGroup.ImageSetOverride.ToNullableUInt(),
                (fireGroup.Flags & FireGroupFlags.CanChamberIronSights) != 0
            );
            builtFireGroups.TryAdd(built.FireGroupID, built);
        }

        await dbContext.UpsertCollectionAsync(builtFireGroups.Values, ct).ConfigureAwait(false);
    }
}
