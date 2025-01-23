using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MFish = Sanctuary.Census.Common.Objects.Collections.Fish;
using MFishRarity = Sanctuary.Census.Common.Objects.Collections.FishRarity;
using MFishSizeType = Sanctuary.Census.Common.Objects.Collections.FishSizeType;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MFish"/>, <see cref="MFishRarity"/> and <see cref="MFishSizeType"/> collections.
/// </summary>
public class FishCollectionsBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IImageSetHelperService _imageSetHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="imageSetHelper">The image set helper service.</param>
    public FishCollectionsBuilder
    (
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache,
        IImageSetHelperService imageSetHelper
    )
    {
        _clientDataCache = clientDataCache;
        _localeDataCache = localeDataCache;
        _imageSetHelper = imageSetHelper;
    }

    /// <inheritdoc />
    public async Task BuildAsync
    (
        ICollectionsContext dbContext,
        CancellationToken ct
    )
    {
        await UpsertFishes(dbContext, ct);
        await UpsertFishRarities(dbContext, ct);
        await UpsertFishSizeTypes(dbContext, ct);
    }

    private async Task UpsertFishes(ICollectionsContext dbContext, CancellationToken ct)
    {
        if (_clientDataCache.Fishes is null)
            throw new MissingCacheDataException(typeof(Fish));

        List<MFish> builtFish = [];
        foreach (Fish fish in _clientDataCache.Fishes)
        {
            bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(fish.ImageSetId, out uint defaultImage);

            builtFish.Add(new MFish
            (
                fish.Id,
                _localeDataCache.GetLocaleStringOrNull(fish.NameId)!,
                _localeDataCache.GetLocaleStringOrNull(fish.DescriptionId)!,
                fish.RarityId,
                fish.ZoneSetId,
                fish.AverageSize,
                fish.SizeDeviation,
                fish.NormalSpeed,
                fish.MaximumSpeed,
                fish.Mobility,
                fish.ScanPointAmount,
                fish.SensitivityDistance,
                fish.Cost,
                fish.ImageSetId,
                hasDefaultImage ? defaultImage : null,
                hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null,
                fish.FishSizeType
            ));
        }

        await dbContext.UpsertCollectionAsync(builtFish, ct);
    }

    private async Task UpsertFishRarities(ICollectionsContext dbContext, CancellationToken ct)
    {
        if (_clientDataCache.FishRarities is null)
            throw new MissingCacheDataException(typeof(FishRarity));

        IEnumerable<MFishRarity> fishRarities = _clientDataCache.FishRarities
            .Select(x => new MFishRarity
            (
                x.Id,
                _localeDataCache.GetLocaleStringOrNull(x.NameId)!
            ));

        await dbContext.UpsertCollectionAsync(fishRarities, ct);
    }

    private async Task UpsertFishSizeTypes(ICollectionsContext dbContext, CancellationToken ct)
    {
        if (_clientDataCache.FishSizeTypes is null)
            throw new MissingCacheDataException(typeof(FishSizeType));

        IEnumerable<MFishSizeType> fishSizeTypes = _clientDataCache.FishSizeTypes
            .Select(x => new MFishSizeType
            (
                x.Id,
                _localeDataCache.GetLocaleStringOrNull(x.NameId)!
            ));

        await dbContext.UpsertCollectionAsync(fishSizeTypes, ct);
    }
}
