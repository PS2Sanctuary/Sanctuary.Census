using Microsoft.Extensions.Logging;
using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Census.ServerData.Internal.Objects;
using Sanctuary.Zone.Packets.ClientUpdate;
using Sanctuary.Zone.Packets.ReferenceData;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MProfile = Sanctuary.Census.Common.Objects.Collections.Profile;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MProfile"/> collection.
/// </summary>
public class ProfileCollectionBuilder : ICollectionBuilder
{
    private readonly ILogger<ProfileCollectionBuilder> _logger;
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;
    private readonly IImageSetHelperService _imageSetHelper;

    private Dictionary<uint, Dictionary<string, CensusStatUpdate>>? _profileStatUpdates;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileCollectionBuilder"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    /// <param name="imageSetHelper">The image set helper service.</param>
    public ProfileCollectionBuilder
    (
        ILogger<ProfileCollectionBuilder> logger,
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache,
        IServerDataCacheService serverDataCache,
        IImageSetHelperService imageSetHelper
    )
    {
        _logger = logger;
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

        if (_serverDataCache.CharacterStatUpdatesByProfileType.Count == 0)
            throw new MissingCacheDataException(typeof(UpdateStats));

        if (_clientDataCache.ItemProfiles is null)
            throw new MissingCacheDataException(typeof(ItemProfile));

        Dictionary<uint, FactionDefinition> profileFactionMap = new();
        foreach (ItemProfile profile in _clientDataCache.ItemProfiles)
        {
            profileFactionMap.TryAdd(profile.ProfileID, profile.FactionID);
            if (profileFactionMap[profile.ProfileID] != profile.FactionID)
                profileFactionMap[profile.ProfileID] = FactionDefinition.None;
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
                profile.CameraHeight.ToNullableDecimal(),
                profile.CrouchCameraHeight.ToNullableDecimal(),
                profile.ImageSetID.ToNullableUInt(),
                hasDefaultImage ? defaultImage : null,
                hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            built = AppendCharacterStats(built);
            builtProfiles.Add(built.ProfileId, built);
        }

        await dbContext.UpsertCollectionAsync(builtProfiles.Values, ct).ConfigureAwait(false);
    }

    private MProfile AppendCharacterStats(MProfile profile)
    {
        EnsureProfileStatUpdates();

        if (!_profileStatUpdates.TryGetValue(profile.ProfileTypeId, out Dictionary<string, CensusStatUpdate>? updates))
            return profile;

        try
        {
            return profile with
            {
                MovementSpeed = updates["StatId.MaxMovementSpeed"].BaseFloatValue!.Value.ToNullableDecimal(),
                BackpedalSpeedMultiplier = updates["StatId.BackpedalSpeedModifier"].BaseFloatValue!.Value.ToNullableDecimal(),
                CrouchSpeedMultiplier = updates["StatId.CrouchSpeedModifier"].BaseFloatValue!.Value.ToNullableDecimal(),
                SprintSpeedMultiplier = updates["StatId.SprintSpeedModifier"].BaseFloatValue!.Value.ToNullableDecimal(),
                StrafeSpeedMultiplier = updates["StatId.StrafeSpeedModifier"].BaseFloatValue!.Value.ToNullableDecimal(),
                SprintAccelerationTimeSec = updates["StatId.SprintAccelerationTime"].BaseFloatValue!.Value.ToNullableDecimal(),
                SprintDecelerationTimeSec = updates["StatId.SprintDecelerationTime"].BaseFloatValue!.Value.ToNullableDecimal(),
                ForwardAccelerationTimeSec = updates["StatId.ForwardAccelerationTime"].BaseFloatValue!.Value.ToNullableDecimal(),
                ForwardDecelerationTimeSec = updates["StatId.ForwardDecelerationTime"].BaseFloatValue!.Value.ToNullableDecimal(),
                BackAccelerationTimeSec = updates["StatId.BackAccelerationTime"].BaseFloatValue!.Value.ToNullableDecimal(),
                BackDecelerationTimeSec = updates["StatId.BackDecelerationTime"].BaseFloatValue!.Value.ToNullableDecimal(),
                StrafeAccelerationTimeSec = updates["StatId.StrafeAccelerationTime"].BaseFloatValue!.Value.ToNullableDecimal(),
                StrafeDecelerationTimeSec = updates["StatId.StrafeDecelerationTime"].BaseFloatValue!.Value.ToNullableDecimal(),
                MaxWaterSpeedMultiplier = updates["StatId.MaxWaterSpeedModifier"].BaseFloatValue!.Value.ToNullableDecimal()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add character stats to profile");
            return profile;
        }
    }


    [MemberNotNull(nameof(_profileStatUpdates))]
    private void EnsureProfileStatUpdates()
    {
        if (_profileStatUpdates is not null)
            return;

        IEnumerable<(uint, List<CensusStatUpdate>?)> orderedUpdates = _serverDataCache
            .CharacterStatUpdatesByProfileType
            .OrderBy(x => x.Value.Item1)
            .Select(x => (x.Key, x.Value.Item2));

        _profileStatUpdates = new Dictionary<uint, Dictionary<string, CensusStatUpdate>>();
        Dictionary<string, CensusStatUpdate> lastBundle = new();

        foreach ((uint profileType, List<CensusStatUpdate>? stats) in orderedUpdates)
        {
            if (stats is not null)
            {
                foreach (CensusStatUpdate update in stats)
                    lastBundle[update.StatName] = update;
            }

            Dictionary<string, CensusStatUpdate> clone = new(lastBundle);
            _profileStatUpdates[profileType] = clone;
        }
    }
}
