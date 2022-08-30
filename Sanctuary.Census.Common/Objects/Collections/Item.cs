using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents an item definition.
/// </summary>
/// <param name="ItemID">The ID of the item.</param>
/// <param name="ItemTypeID">The type ID of the item</param>
/// <param name="ItemCategoryID">The ID of the category that the item belongs to.</param>
/// <param name="FactionID">The faction that the item may be used on.</param>
/// <param name="Name">The name of the item.</param>
/// <param name="Description">The description of the item.</param>
/// <param name="ActivatableAbilityID">The ID of the item's activatable ability.</param>
/// <param name="PassiveAbilityID">The ID of the item's passive ability.</param>
/// <param name="PassiveAbilitySetID">The ID of the item's passive ability set.</param>
/// <param name="SkillSetID">The ID of the skill set that the item uses.</param>
/// <param name="MaxStackSize">The maximum number of this item that can be equipped in a given slot at any time.</param>
/// <param name="ImageSetID">The ID of the item's image set.</param>
/// <param name="ImageID">The ID of the item's default image.</param>
/// <param name="ImagePath">The relative path to the item's default image.</param>
/// <param name="HudImageSetID">The ID of the item's HUD display image.</param>
/// <param name="IsAccountScoped">Indicates whether this item is available to every character on an account once purchased.</param>
[Collection]
public record Item
(
    [property:Key] uint ItemID,
    uint ItemTypeID,
    uint? ItemCategoryID,
    uint FactionID,
    LocaleString? Name,
    LocaleString? Description,
    uint? ActivatableAbilityID,
    uint? PassiveAbilityID,
    uint? PassiveAbilitySetID,
    uint? SkillSetID,
    uint? ImageSetID,
    uint? ImageID,
    string? ImagePath,
    uint? HudImageSetID,
    int MaxStackSize,
    bool IsAccountScoped
) : ISanctuaryCollection;
