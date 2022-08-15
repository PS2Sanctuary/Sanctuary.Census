using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MProfile = Sanctuary.Census.Common.Objects.Collections.Profile;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MProfile"/> collection.
/// </summary>
public class ProfileCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    public ProfileCollectionBuilder
    (
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache,
        IServerDataCacheService serverDataCache
    )
    {
        _clientDataCache = clientDataCache;
        _localeDataCache = localeDataCache;
        _serverDataCache = serverDataCache;
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

        if (_clientDataCache.ImageSetMappings is null)
            throw new MissingCacheDataException(typeof(ImageSetMapping));

        Dictionary<uint, FactionDefinition> profileFactionMap = new();
        foreach (ItemProfile profile in _clientDataCache.ItemProfiles)
        {
            profileFactionMap.TryAdd(profile.ProfileID, profile.FactionID);
            if (profileFactionMap[profile.ProfileID] != profile.FactionID)
                profileFactionMap[profile.ProfileID] = FactionDefinition.All;
        }

        Dictionary<uint, uint> imageSetToPrimaryImageMap = new();
        foreach (ImageSetMapping mapping in _clientDataCache.ImageSetMappings)
        {
            if (mapping.ImageType is not ImageSetType.Large)
                continue;

            imageSetToPrimaryImageMap[mapping.ImageSetID] = mapping.ImageID;
        }

        Dictionary<uint, MProfile> builtProfiles = new();
        foreach (Profile profile in _serverDataCache.ProfileDefinitions.Profiles)
        {
            _localeDataCache.TryGetLocaleString(profile.NameID, out LocaleString? name);
            _localeDataCache.TryGetLocaleString(profile.DescriptionID, out LocaleString? description);

            bool hasFaction = profileFactionMap.TryGetValue(profile.ProfileID, out FactionDefinition factionId);
            bool hasImage = imageSetToPrimaryImageMap.TryGetValue(profile.ImageSetID, out uint imageId);
            hasImage &= profile.ImageSetID != 0;

            MProfile built = new
            (
                profile.ProfileID,
                profile.ProfileTypeID,
                hasFaction ? (uint)factionId : null,
                name,
                description,
                profile.ImageSetID == 0 ? null : profile.ImageSetID,
                hasImage ? imageId : null,
                hasImage ? $"/files/ps2/images/static/{imageId}.png" : null
            );
            builtProfiles.Add(built.ProfileId, built);
        }

        await dbContext.UpsertProfilesAsync(builtProfiles.Values, ct);
    }
}
