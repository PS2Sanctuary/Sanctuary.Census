using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MProfile = Sanctuary.Census.Common.Objects.Collections.Profile;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MProfile"/> collection.
/// </summary>
public class ProfileCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;
    private readonly IImageSetHelperService _imageSetHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    /// <param name="imageSetHelper">The image set helper service.</param>
    public ProfileCollectionBuilder
    (
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache,
        IServerDataCacheService serverDataCache,
        IImageSetHelperService imageSetHelper
    )
    {
        _clientDataCache = clientDataCache;
        _localeDataCache = localeDataCache;
        _serverDataCache = serverDataCache;
        _imageSetHelper = imageSetHelper;
    }

    /// <inheritdoc />
    public async Task BuildAsync
    (
        ICollectionsContext dbContext,
        CancellationToken ct
    )
    {
        if (_serverDataCache.ProfileDefinitions is null)
            throw new MissingCacheDataException(typeof(ProfileDefinitions));

        if (_clientDataCache.ItemProfiles is null)
            throw new MissingCacheDataException(typeof(ItemProfile));

        Dictionary<uint, FactionDefinition> profileFactionMap = new();
        foreach (ItemProfile profile in _clientDataCache.ItemProfiles)
        {
            profileFactionMap.TryAdd(profile.ProfileID, profile.FactionID);
            if (profileFactionMap[profile.ProfileID] != profile.FactionID)
                profileFactionMap[profile.ProfileID] = FactionDefinition.All;
        }

        Dictionary<uint, MProfile> builtProfiles = new();
        foreach (Profile profile in _serverDataCache.ProfileDefinitions.Profiles)
        {
            _localeDataCache.TryGetLocaleString(profile.NameID, out LocaleString? name);
            _localeDataCache.TryGetLocaleString(profile.DescriptionID, out LocaleString? description);

            bool hasFaction = profileFactionMap.TryGetValue(profile.ProfileId_1, out FactionDefinition factionId);
            bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(profile.ImageSetID, out uint defaultImage);

            MProfile built = new
            (
                profile.ProfileId_1,
                profile.ProfileTypeID,
                hasFaction ? (uint)factionId : null,
                name,
                description,
                profile.ImageSetID == 0 ? null : profile.ImageSetID,
                hasDefaultImage ? defaultImage : null,
                hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null
            );
            builtProfiles.Add(built.ProfileId, built);
        }

        await dbContext.UpsertCollectionAsync(builtProfiles.Values, ct).ConfigureAwait(false);
    }
}
