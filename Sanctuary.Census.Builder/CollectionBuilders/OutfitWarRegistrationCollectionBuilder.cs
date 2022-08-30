using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Common.Objects;
using Sanctuary.Zone.Packets.OutfitWars;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="OutfitWarRegistration"/> collection.
/// </summary>
public class OutfitWarRegistrationCollectionBuilder : ICollectionBuilder
{
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutfitWarRegistrationCollectionBuilder"/> class.
    /// </summary>
    /// <param name="serverDataCache">The server data cache.</param>
    public OutfitWarRegistrationCollectionBuilder
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
        if (_serverDataCache.RegisteredOutfits.Count == 0)
            throw new MissingCacheDataException(typeof(RegisteredOutfits));

        Dictionary<ulong, OutfitWarRegistration> builtOutfits = new();
        foreach ((ServerDefinition server, RegisteredOutfits outfits) in _serverDataCache.RegisteredOutfits)
        {
            foreach (RegisteredOutfit outfit in outfits.Outfits)
            {
                OutfitWarRegistration built = new
                (
                    outfit.OutfitID,
                    (uint)outfit.FactionID,
                    (uint)server,
                    outfit.RegistrationOrder,
                    (OutfitWarRegistration.RegistrationStatus)outfit.Status,
                    outfit.Status is RegistrationStatus.Full or RegistrationStatus.WaitingOnNextFullReg
                        ? 48
                        : outfit.MemberSignupCount
                );
                builtOutfits.Add(built.OutfitID, built);
            }
        }

        await dbContext.UpsertCollectionAsync(builtOutfits.Values, ct).ConfigureAwait(false);
    }
}
