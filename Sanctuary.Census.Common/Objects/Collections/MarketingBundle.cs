using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a purchasable item from the in-game depot.
/// </summary>
/// <param name="MarketingBundleID">The ID of the marketing bundle.</param>
/// <param name="MarketingBundleCategoryID">The ID of the category that the bundle belongs to.</param>
/// <param name="Name">The name of the bundle.</param>
/// <param name="Description">The description of the bundle.</param>
/// <param name="ImageSetID">The ID of the bundle's image set.</param>
/// <param name="StationCashPrice">The station cash price of the bundle.</param>
/// <param name="SalePrice">The sale price of the bundle, if the bundle <paramref name="IsOnSale"/>.</param>
/// <param name="MemberSalePrice">The sale price of the bundle for members.</param>
/// <param name="ReleaseTime">The time that the bundle was released.</param>
/// <param name="AvailableTillTime">The time that the bundle is available till.</param>
/// <param name="IsOnSale">Indicates whether the bundle is on sale.</param>
/// <param name="IsUnavailable">Indicates whether the bundle is unavailable.</param>
/// <param name="IsGiftable">Indicates whether the bundle is giftable.</param>
/// <param name="CreatorName">The name of the creator of the bundles contents.</param>
[Collection]
[Description("Represents a purchasable item from the in-game depot")]
public record MarketingBundle
(
    [property: Key] uint MarketingBundleID,
    [property: Key] uint MarketingBundleCategoryID,
    LocaleString Name,
    LocaleString? Description,
    [property: Key] uint ImageSetID,
    uint StationCashPrice,
    uint? SalePrice,
    uint? MemberSalePrice,
    ulong ReleaseTime,
    ulong AvailableTillTime,
    bool IsOnSale,
    bool IsUnavailable,
    bool IsGiftable,
    string? CreatorName
) : ISanctuaryCollection;
