using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client vehicle loadout slot data.
/// </summary>
/// <param name="LoadoutID">The ID of the loadout that this slot belongs to.</param>
/// <param name="SlotID">The ID of the slot.</param>
/// <param name="NameID">The locale string ID of the slot's name.</param>
/// <param name="DescriptionID">The locale string ID of the slot's description.</param>
/// <param name="IconID">The ID of the slot's image set. Appears unused.</param>
/// <param name="ItemLineID">Unknown.</param>
/// <param name="TintItemID">Unknown.</param>
/// <param name="TintItemSlotID">Unknown.</param>
/// <param name="FlagAutoEquip">Whether this slot is automatically equipped.</param>
/// <param name="FlagRequired">Whether this slot is required to be equipped.</param>
/// <param name="FlagUseBodyTintItem">Unknown.</param>
/// <param name="FlagUseDecalItem">Unknown.</param>
/// <param name="FlagCanCustomize">Unknown.</param>
/// <param name="FlagIsVisible">Whether this slot is visible to the user.</param>
/// <param name="ReqSetID">The ID of the requirement set that must be met to use this slot.</param>
/// <param name="EquipSlotID">The equipment slot ID that this slot uses.</param>
/// <param name="SlotUITag">The UI section that this slot appears under.</param>
/// <param name="UnlockHintStringID">Unknown.</param>
[Datasheet]
public partial record VehicleLoadoutSlot
(
    uint LoadoutID,
    uint SlotID,
    uint NameID,
    uint DescriptionID,
    uint IconID,
    uint ItemLineID,
    uint TintItemID,
    uint TintItemSlotID,
    bool FlagAutoEquip,
    bool FlagRequired,
    bool FlagUseBodyTintItem,
    bool FlagUseDecalItem,
    bool FlagCanCustomize,
    bool FlagIsVisible,
    uint ReqSetID,
    uint EquipSlotID,
    string? SlotUITag,
    uint UnlockHintStringID
);
