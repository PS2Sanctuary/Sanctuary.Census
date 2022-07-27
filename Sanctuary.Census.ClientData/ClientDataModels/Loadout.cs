namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client loadout data.
/// </summary>
/// <param name="LoadoutID">The ID of the loadout.</param>
/// <param name="ProfileID">The profile used by this loadout.</param>
/// <param name="FactionID">The faction that this loadout can be used by.</param>
/// <param name="DisplayIndex">The UI display index of the loadout.</param>
public record Loadout
(
    uint LoadoutID,
    uint ProfileID,
    uint FactionID,
    int DisplayIndex
);
