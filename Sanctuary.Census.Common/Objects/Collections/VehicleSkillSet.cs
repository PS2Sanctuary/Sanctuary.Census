using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents vehicle skill set data.
/// </summary>
/// <param name="VehicleID">The ID of the vehicle.</param>
/// <param name="SkillSetID">The ID of the skill set.</param>
/// <param name="FactionID">The ID of the faction that the skill set is applied to the vehicle on.</param>
/// <param name="DisplayIndex">The display index.</param>
[Collection]
public record VehicleSkillSet
(
    [property: Key] uint VehicleID,
    [property: Key] uint SkillSetID,
    [property: Key] uint FactionID,
    byte DisplayIndex
) : ISanctuaryCollection;
