using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a loadout.
/// </summary>
/// <param name="LoadoutID">The ID of the loadout.</param>
/// <param name="ProfileID">The profile used by this loadout.</param>
/// <param name="FactionID">The faction that this loadout can be used by.</param>
/// <param name="CodeName">A descriptor for the faction and profile that this loadout targets.</param>
[Collection]
public record Loadout
(
    [property: Key] uint LoadoutID,
    [property: Key] uint ProfileID,
    [property: Key] uint FactionID,
    string? CodeName
) : ISanctuaryCollection;
