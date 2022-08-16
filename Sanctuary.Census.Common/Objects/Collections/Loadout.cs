using Sanctuary.Census.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a loadout.
/// </summary>
/// <param name="LoadoutID">The ID of the loadout.</param>
/// <param name="ProfileID">The profile used by this loadout.</param>
/// <param name="FactionID">The faction that this loadout can be used by.</param>
[Collection]
public record Loadout
(
    [property: Key] uint LoadoutID,
    uint ProfileID,
    uint FactionID
);
