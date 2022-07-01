using Sanctuary.Census.Objects.CommonModels;

namespace Sanctuary.Census.Objects.ClientDataModels;

/// <summary>
/// Represents client item definitions.
/// </summary>
/// <param name="ID">The item ID.</param>
/// <param name="CodeFactoryName">The name of the code factory that produces the item stats.</param>
/// <param name="NameID">The locale ID of the item name.</param>
/// <param name="DescriptionID">The locale ID of the item description.</param>
/// <param name="ImageSetID">The ID of the item's image set.</param>
/// <param name="ActivatableAbilityID">The ID of the item's activatable ability.</param>
/// <param name="PassiveAbilityID">The ID of the item's passive ability.</param>
/// <param name="Cost">The cost of the item. This field appears unused.</param>
/// <param name="ItemClass">The class of the item.</param>
/// <param name="MaxStackSize">The maximum number of this item that can be equipped in a given slot at any time.</param>
/// <param name="ProfileOverride">Overrides the loadout? profile with the given profile.</param>
/// <param name="Slot">The loadout slot that this item can be equipped in.</param>
/// <param name="SlotKeyOverride">Unknown.</param>
/// <param name="NoTrade">Indicates whether this item cannot be traded.</param>
/// <param name="ModelName">The filename of the model that this item uses.</param>
/// <param name="GenderUsage">The character gender that this item is restricted to.</param>
/// <param name="TextureAlias">The name of the texture that this item uses.</param>
/// <param name="TintID">The ID of the tint that this item uses.</param>
/// <param name="CategoryID">The ID of the category that this item belongs to.</param>
/// <param name="MembersOnly">Indicates whether only members can use this item.</param>
/// <param name="NonMiniGame">Unknown.</param>
/// <param name="Param1">Unknown.</param>
/// <param name="Param2">Unknown.</param>
/// <param name="Param3">Unknown.</param>
/// <param name="NoSale">Indicates whether this item cannot be sold.</param>
/// <param name="WeaponTrailEffectID">The ID of the trail effect that this item emits.</param>
/// <param name="UseRequirementID">The client must be permitted this requirement before using the item.</param>
/// <param name="ClientUseRequirementID">The client must be permitted this requirement before using the item.</param>
/// <param name="CompositeEffectID">The ID of the composite effect that this item uses.</param>
/// <param name="PowerRating">Unknown.</param>
/// <param name="MinProfileRank">The minimum battle rank that a character must be to use this item.</param>
/// <param name="Rarity">Unknown.</param>
/// <param name="ContentID">Unknown.</param>
/// <param name="NoLiveGamer">Unknown.</param>
/// <param name="TintAlias">The name of the tint that this item uses.</param>
/// <param name="CombatOnly">Unknown.</param>
/// <param name="TintGroupID">The ID of the tint group that this item belongs to.</param>
/// <param name="ForceDisablePreview">Unknown.</param>
/// <param name="MemberDiscount">Indicates whether the member depot discount applies to this item.</param>
/// <param name="RaceSetID">Unknown.</param>
/// <param name="PersistProfileSwitch">Unknown.</param>
/// <param name="FlagQuickUse">Whether this item can be bound to the quick-use command.</param>
/// <param name="FlagConsumeOnUse">Indicates whether this item is consumed upon use.</param>
/// <param name="FlagRemoveOnUse">Indicates whether this item is removed upon use.</param>
/// <param name="FlagCanEquip">Indicates whether this item can be equipped in a loadout.</param>
/// <param name="FlagShowOnWheel">Unknown.</param>
/// <param name="FlagAccountScope">Indicates whether this item is available to every character on an account once purchased.</param>
/// <param name="UIModelCameraID">The ID of the UI camera that this item uses when equipped.</param>
/// <param name="EquipCountMax">The maximum number of times this item can be equipped across a loadout.</param>
/// <param name="CurrencyType">The currency that this item can be purchased with.</param>
/// <param name="DatasheetID">The ID of the datasheet that contains addition information about this item.</param>
/// <param name="ItemType">The type ID of the item.</param>
/// <param name="SkillSetID">The skillset ID of the item.</param>
/// <param name="OverlayTexture">The name of the overlay texture that is displayed when this item is equipped.</param>
/// <param name="DecalSlot">The decal slot that this item can be applied to.</param>
/// <param name="OverlayAdjustment">Unknown.</param>
/// <param name="TrialDurationSec">The time in seconds that this item can be trialed for, or <c>0</c> for the default time.</param>
/// <param name="NextTrialDelaySec">
/// The time in seconds that the ability to trial this item will be on cooldown after use, or <c>0</c> for the default time.
/// </param>
/// <param name="PassiveAbilitySetID">The ID of the ability set that this item passively uses.</param>
/// <param name="HudImageSetID">The ID of the HUD image set that this item uses.</param>
/// <param name="OverrideAppearance">Unknown.</param>
/// <param name="OverrideCameraID">Unknown.</param>
/// <param name="PlayerStudioDisplayName">Unknown.</param>
/// <param name="SoundID">The ID of the sound that this item plays.</param>
/// <param name="IsAllowedAsGuildDecal">Indicates whether this item can be used as an outfit decal.</param>
/// <param name="ClientDisplayRequirementID">The client must be permitted this requirement before displaying the item.</param>
/// <param name="ResourceType">Gets the ID of the resource type that this item consumes.</param>
/// <param name="ResourceCost">Gets the amount of the given resource type that this item consumes.</param>
/// <param name="NoGift">Indicates whether this item can be gifted.</param>
public record ClientItemDefinition
(
    uint ID,
    string CodeFactoryName,
    int NameID,
    int DescriptionID,
    int ImageSetID,
    uint ActivatableAbilityID,
    uint PassiveAbilityID,
    int Cost,
    uint ItemClass,
    int MaxStackSize,
    uint ProfileOverride,
    uint Slot,
    uint SlotKeyOverride,
    bool NoTrade,
    string? ModelName,
    GenderDefinition GenderUsage,
    string? TextureAlias,
    uint TintID,
    uint CategoryID,
    bool MembersOnly,
    bool NonMiniGame,
    int Param1,
    int Param2,
    int Param3,
    bool NoSale,
    uint WeaponTrailEffectID,
    uint UseRequirementID,
    uint ClientUseRequirementID,
    uint CompositeEffectID,
    int PowerRating,
    ushort MinProfileRank,
    int Rarity,
    uint ContentID,
    bool NoLiveGamer,
    string? TintAlias,
    bool CombatOnly,
    uint TintGroupID,
    bool ForceDisablePreview,
    bool MemberDiscount,
    uint RaceSetID,
    bool PersistProfileSwitch,
    bool FlagQuickUse,
    bool FlagConsumeOnUse,
    bool FlagRemoveOnUse,
    bool FlagCanEquip,
    bool FlagShowOnWheel,
    bool FlagAccountScope,
    uint UIModelCameraID,
    ushort EquipCountMax,
    CurrencyDefinition CurrencyType,
    uint DatasheetID,
    uint ItemType,
    uint SkillSetID,
    string? OverlayTexture,
    string? DecalSlot,
    int OverlayAdjustment,
    uint TrialDurationSec,
    uint NextTrialDelaySec,
    uint PassiveAbilitySetID,
    uint HudImageSetID,
    string? OverrideAppearance,
    uint OverrideCameraID,
    string? PlayerStudioDisplayName,
    uint SoundID,
    bool IsAllowedAsGuildDecal,
    uint ClientDisplayRequirementID,
    uint ResourceType,
    int ResourceCost,
    bool NoGift
);
