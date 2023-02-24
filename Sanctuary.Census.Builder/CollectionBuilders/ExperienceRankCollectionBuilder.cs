using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.CommonModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MExperienceRank = Sanctuary.Census.Common.Objects.Collections.ExperienceRank;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MExperienceRank"/> collection.
/// </summary>
public class ExperienceRankCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IImageSetHelperService _imageSetHelperService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExperienceRankCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="imageSetHelperService">The image set helper service.</param>
    public ExperienceRankCollectionBuilder
    (
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache,
        IImageSetHelperService imageSetHelperService
    )
    {
        _clientDataCache = clientDataCache;
        _localeDataCache = localeDataCache;
        _imageSetHelperService = imageSetHelperService;
    }

    /// <inheritdoc />
    public async Task BuildAsync(ICollectionsContext dbContext, CancellationToken ct = default)
    {
        if (_clientDataCache.ExperienceRanks is null)
            throw new MissingCacheDataException(typeof(ExperienceRank));

        Dictionary<uint, int> experienceRankToPrestigeLevel = GetExperienceRankToPrestigeLevel();
        Dictionary<uint, MExperienceRank> builtExperienceRanks = new();

        foreach (ExperienceRank rank in _clientDataCache.ExperienceRanks.OrderBy(x => x.Id).ThenBy(x => x.Rank))
        {
            _localeDataCache.TryGetLocaleString(rank.Title01, out LocaleString? vsName);
            _localeDataCache.TryGetLocaleString(rank.Title02, out LocaleString? ncName);
            _localeDataCache.TryGetLocaleString(rank.Title03, out LocaleString? trName);
            _localeDataCache.TryGetLocaleString(rank.Title04, out LocaleString? nsoName);

            bool hasDefaultVSImage = _imageSetHelperService.TryGetDefaultImage(rank.ImageSetIdVS, out uint vsImage);
            bool hasDefaultNCImage = _imageSetHelperService.TryGetDefaultImage(rank.ImageSetIdNC, out uint ncImage);
            bool hasDefaultTRImage = _imageSetHelperService.TryGetDefaultImage(rank.ImageSetIdTR, out uint trImage);
            bool hasDefaultNSOImage = _imageSetHelperService.TryGetDefaultImage(rank.ImageSetIdNSO, out uint nsoImage);

            builtExperienceRanks.TryAdd(rank.Id, new MExperienceRank
            (
                rank.Id,
                rank.Rank,
                experienceRankToPrestigeLevel.TryGetValue(rank.Id, out int level)
                    ? level
                    : -1,
                rank.XpMax,
                new MExperienceRank.FactionInfo
                (
                    vsName,
                    rank.ImageSetIdVS.ToNullableUInt(),
                    vsImage.ToNullableUInt()
                ),
                hasDefaultVSImage ? _imageSetHelperService.GetRelativeImagePath(vsImage) : null,
                new MExperienceRank.FactionInfo
                (
                    ncName,
                    rank.ImageSetIdNC.ToNullableUInt(),
                    ncImage.ToNullableUInt()
                ),
                hasDefaultNCImage ? _imageSetHelperService.GetRelativeImagePath(ncImage) : null,
                new MExperienceRank.FactionInfo
                (
                    trName,
                    rank.ImageSetIdTR.ToNullableUInt(),
                    trImage.ToNullableUInt()
                ),
                hasDefaultTRImage ? _imageSetHelperService.GetRelativeImagePath(trImage) : null,
                new MExperienceRank.FactionInfo
                (
                    nsoName,
                    rank.ImageSetIdNSO.ToNullableUInt(),
                    nsoImage.ToNullableUInt()
                ),
                hasDefaultNSOImage ? _imageSetHelperService.GetRelativeImagePath(nsoImage) : null
            ));
        }

        await dbContext.UpsertCollectionAsync(builtExperienceRanks.Values, ct);
    }

    private Dictionary<uint, int> GetExperienceRankToPrestigeLevel()
    {
        if (_clientDataCache.ClientPrestigeLevels is null)
            throw new MissingCacheDataException(typeof(ClientPrestigeLevel));

        if (_clientDataCache.PrestigeLevelRankSets is null)
            throw new MissingCacheDataException(typeof(PrestigeLevelRankSet));

        if (_clientDataCache.PrestigeRankSetMappings is null)
            throw new MissingCacheDataException(typeof(PrestigeRankSetMapping));

        Dictionary<uint, int> _prestigeLevels = _clientDataCache.ClientPrestigeLevels
            .ToDictionary
            (
                prestigeLevel => prestigeLevel.Id,
                prestigeLevel => prestigeLevel.PrestigeLevel
            );

        Dictionary<uint, int> prestigeRankSetToLevel =  _clientDataCache.PrestigeLevelRankSets
            .ToDictionary
            (
                x => x.Id,
                x => _prestigeLevels.TryGetValue(x.PrestigeLevelId, out int level)
                    ? level
                    : -1
            );

        return _clientDataCache.PrestigeRankSetMappings
            .ToDictionary
            (
                x => x.ExperienceRankId,
                x => prestigeRankSetToLevel.TryGetValue(x.PrestigeLevelRankSetId, out int level)
                    ? level
                    : -1
            );
    }
}
