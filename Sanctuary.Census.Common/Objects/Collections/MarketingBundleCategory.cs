using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a depot category.
/// </summary>
/// <param name="MarketingBundleCategoryID">The ID of the category.</param>
/// <param name="Name">The name of the category.</param>
/// <param name="DisplayIndex">The index that the category is displayed at in the in-game depot.</param>
/// <param name="ImageSetID">The ID of the category's image set.</param>
/// <param name="ImageID">The ID of the directive's default image.</param>
/// <param name="ImagePath">The relative path to the directive's default image.</param>
[Collection]
[Description("Represents a category from the in-game depot")]
public record MarketingBundleCategory
(
    [property: Key] uint MarketingBundleCategoryID,
    LocaleString Name,
    uint DisplayIndex,
    [property: Key] uint ImageSetID,
    [property: Key] uint? ImageID,
    string? ImagePath
) : ISanctuaryCollection;
