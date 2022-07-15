namespace Sanctuary.Census.Models.Collections;

/// <summary>
/// Represents fire group data.
/// </summary>
/// <param name="FireGroupID">The ID of the fire group.</param>
/// <param name="ChamberDurationMS">The time in milliseconds to chamber a round.</param>
/// <param name="TransitionDurationMS">The time in milliseconds to transition to this fire group.</param>
/// <param name="SpoolUpTimeMS">The time in milliseconds to spool up before firing.</param>
/// <param name="SpoolUpInitialRefireTimeMS"></param>
/// <param name="CanChamberIronsights"></param>
public record FireGroup
(
    uint FireGroupID,
    uint? ChamberDurationMS,
    ushort? TransitionDurationMS,
    ushort? SpoolUpTimeMS,
    ushort? SpoolUpInitialRefireTimeMS,
    bool CanChamberIronsights
);
