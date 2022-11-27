using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents client vehicle loadout data.
/// </summary>
/// <param name="LoadoutID">The ID of the loadout.</param>
/// <param name="VehicleID">The ID of the vehicle that the loadout applies to.</param>
/// <param name="FactionID">The ID of the faction that can use this loadout.</param>
/// <param name="IsCustomizable">Whether the loadout can be customized by the user.</param>
/// <param name="IsHiddenFromLoadoutScreen">Whether the loadout is hidden in the UI.</param>
[Collection]
public record VehicleLoadout
(
    [property: JoinKey] uint LoadoutID,
    [property: JoinKey] uint VehicleID,
    [property: JoinKey] uint FactionID,
    bool IsCustomizable,
    bool IsHiddenFromLoadoutScreen
) : ISanctuaryCollection;
