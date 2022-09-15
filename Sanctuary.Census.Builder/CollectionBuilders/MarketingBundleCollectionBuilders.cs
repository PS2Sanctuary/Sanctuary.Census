using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.InGamePurchase;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MarketingBundle"/>, <see cref="MarketingBundleCategory"/> and <see cref="MarketingBundleItem"/> collections.
/// </summary>
public class MarketingBundleCollectionBuilders : ICollectionBuilder
{
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;
    private readonly IImageSetHelperService _imageSetHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarketingBundleCollectionBuilders"/> class.
    /// </summary>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    /// <param name="imageSetHelper">The image set helper service.</param>
    public MarketingBundleCollectionBuilders
    (
        ILocaleDataCacheService localeDataCache,
        IServerDataCacheService serverDataCache,
        IImageSetHelperService imageSetHelper
    )
    {
        _localeDataCache = localeDataCache;
        _serverDataCache = serverDataCache;
        _imageSetHelper = imageSetHelper;
    }

    /// <inheritdoc />
    public async Task BuildAsync(ICollectionsContext dbContext, CancellationToken ct = default)
    {
        if (_serverDataCache.StoreBundles is null)
            throw new MissingCacheDataException(typeof(StoreBundles));

        if (_serverDataCache.StoreBundleCategories is null)
            throw new MissingCacheDataException(typeof(StoreBundleCategories));

        Dictionary<uint, MarketingBundle> builtBundles = new();
        List<MarketingBundleItem> builtItems = new();
        foreach (StoreBundles_Bundle bundle in _serverDataCache.StoreBundles.Bundles)
        {
            _localeDataCache.TryGetLocaleString(bundle.NameID, out LocaleString? name);
            _localeDataCache.TryGetLocaleString(bundle.DescriptionID, out LocaleString? description);
            uint imageSetID = uint.Parse(bundle.BundleImage.ImageSetID);
            bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(imageSetID, out uint defaultImage);

            MarketingBundle builtBundle = new
            (
                bundle.BundleID_1,
                bundle.CategoryID,
                name!,
                description,
                bundle.StationCashPrice,
                bundle.SalePrice1 == 0 ? null : bundle.SalePrice1,
                bundle.MemberSalePrice == 0 ? null : bundle.MemberSalePrice,
                bundle.Times1.ReleaseTime,
                bundle.Times1.AvailableTill,
                bundle.IsOnSale,
                bundle.IsUnavailable,
                bundle.IsGiftable,
                bundle.CreatorName.Length == 0 ? null : bundle.CreatorName,
                imageSetID,
                hasDefaultImage ? defaultImage : null,
                hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null
            );
            builtBundles.TryAdd(builtBundle.MarketingBundleID, builtBundle);

            foreach (StoreBundles_ItemListDetail item in bundle.ItemListDetails)
            {
                MarketingBundleItem builtItem = new
                (
                    bundle.BundleID_1,
                    item.ItemID,
                    item.Quantity,
                    bundle.Times1.ReleaseTime
                );
                builtItems.Add(builtItem);
            }
        }
        await dbContext.UpsertCollectionAsync(builtBundles.Values, ct).ConfigureAwait(false);
        await dbContext.UpsertCollectionAsync(builtItems, ct).ConfigureAwait(false);

        Dictionary<uint, MarketingBundleCategory> builtCategories = new();
        foreach (StoreBundleCategories_Category category in _serverDataCache.StoreBundleCategories.Categories)
        {
            _localeDataCache.TryGetLocaleString(category.NameID, out LocaleString? name);
            uint imageSetID = uint.Parse(category.ImageSetID);
            bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(imageSetID, out uint defaultImage);

            MarketingBundleCategory builtCategory = new
            (
                category.CategoryID_1,
                name!,
                category.DisplayIndex,
                imageSetID,
                hasDefaultImage ? defaultImage : null,
                hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null
            );
            builtCategories.TryAdd(builtCategory.MarketingBundleCategoryID, builtCategory);
        }
        await dbContext.UpsertCollectionAsync(builtCategories.Values, ct).ConfigureAwait(false);
    }
}
