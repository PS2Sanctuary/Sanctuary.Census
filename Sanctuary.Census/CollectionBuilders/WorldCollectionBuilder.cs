using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Login.Packets;
using System.Collections.Generic;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="World"/> collection.
/// </summary>
public class WorldCollectionBuilder : ICollectionBuilder
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
        if (serverDataCache.ServerListResponse is null)
            throw new MissingCacheDataException(typeof(ServerListResponse));

        Dictionary<uint, World> builtWorlds = new();
        foreach (ServerUpdate server in serverDataCache.ServerListResponse.Servers)
        {
            localeDataCache.TryGetLocaleString(server.NameID, out LocaleString? name);

            World built = new
            (
                (uint)server.ServerID,
                name!,
                server.Islocked,
                server.AllowedAccess
            );
            builtWorlds.TryAdd(built.WorldID, built);
        }

        context.Worlds = builtWorlds;
    }
}
