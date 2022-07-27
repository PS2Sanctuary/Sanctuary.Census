namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client vehicle loadout data.
/// </summary>
/// <param name="ID">The ID of the loadout.</param>
/// <param name="VehicleID">The ID of the vehicle that the loadout applies to.</param>
/// <param name="FactionID">The ID of the faction that can use this loadout.</param>
/// <param name="DisplayIndex">The UI display index of the loadout.</param>
/// <param name="FlagCanCustomize">Whether the loadout can be customized by the user.</param>
/// <param name="BodyTintItemID">Unknown.</param>
/// <param name="GuildTintItemID">Unknown.</param>
/// <param name="DecalItemID">Unknown.</param>
/// <param name="HideFromLoadoutScreen">Whether the loadout is hidden in the UI.</param>
public record VehicleLoadout
(
    uint ID,
    uint VehicleID,
    uint FactionID,
    int DisplayIndex,
    bool FlagCanCustomize,
    uint BodyTintItemID,
    uint GuildTintItemID,
    uint DecalItemID,
    bool HideFromLoadoutScreen
);
