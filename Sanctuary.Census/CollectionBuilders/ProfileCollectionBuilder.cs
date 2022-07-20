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
using MProfile = Sanctuary.Census.Models.Collections.Profile;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MProfile"/> collection.
/// </summary>
public class ProfileCollectionBuilder : ICollectionBuilder
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
        if (serverDataCache.ProfileDefinitions is null)
            throw new MissingCacheDataException(typeof(ProfileDefinitions));

        if (clientDataCache.ItemProfiles.Count == 0)
            throw new MissingCacheDataException(typeof(ItemProfile));

        Dictionary<uint, FactionDefinition> profileFactionMap = new();
        foreach (ItemProfile profile in clientDataCache.ItemProfiles)
        {
            profileFactionMap.TryAdd(profile.ProfileID, profile.FactionID);
            if (profileFactionMap[profile.ProfileID] != profile.FactionID)
                profileFactionMap[profile.ProfileID] = FactionDefinition.All;
        }

        Dictionary<uint, uint> imageSetToPrimaryImageMap = new();
        foreach (ImageSetMapping mapping in clientDataCache.ImageSetMappings)
        {
            if (mapping.ImageType is not ImageSetType.Large)
                continue;

            imageSetToPrimaryImageMap[mapping.ImageSetID] = mapping.ImageID;
        }

        Dictionary<uint, MProfile> builtProfiles = new();
        foreach (Profile profile in serverDataCache.ProfileDefinitions.Profiles)
        {
            localeDataCache.TryGetLocaleString(profile.NameID, out LocaleString? name);
            localeDataCache.TryGetLocaleString(profile.DescriptionID, out LocaleString? description);

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
