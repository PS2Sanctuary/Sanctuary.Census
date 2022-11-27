using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents fire group data.
/// </summary>
/// <param name="FireGroupID">The ID of the fire group.</param>
/// <param name="ChamberDurationMS">The time in milliseconds to chamber a round.</param>
/// <param name="TransitionDurationMS">The time in milliseconds to transition to this fire group.</param>
/// <param name="SpinUpTimeMS"></param>
/// <param name="SpoolUpTimeMS">The time in milliseconds to spool up before firing.</param>
/// <param name="SpoolUpInitialRefireTimeMS"></param>
/// <param name="ImageSetOverrideId">The image set override.</param>
/// <param name="CanChamberIronsights"></param>
[Collection]
[Description("Common properties of a weapon's firing characteristics. Link to more specific properties via fire_group_to_fire_mode")]
public record FireGroup
(
    [property: JoinKey] uint FireGroupID,
    ushort? ChamberDurationMS,
    ushort? TransitionDurationMS,
    ushort? SpinUpTimeMS,
    ushort? SpoolUpTimeMS,
    ushort? SpoolUpInitialRefireTimeMS,
    uint? ImageSetOverrideId,
    bool CanChamberIronsights
) : ISanctuaryCollection;
