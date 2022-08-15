using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents information about an outfit war.
/// </summary>
/// <param name="OutfitWarID">The ID of the outfit war.</param>
/// <param name="WorldID">The world that the war is occuring on.</param>
/// <param name="Title">The title of the outfit war.</param>
/// <param name="ImageSetID">The ID of the war's image set.</param>
/// <param name="TeamSizeLimit">The maximum number of players that can play in a match.</param>
/// <param name="TeamSignupRequirement">The number of outfit members that must signup before an outfit can participate.</param>
/// <param name="StartTime">The start time of the war.</param>
/// <param name="EndTime">The end time of the war.</param>
/// <param name="Phases">The phases of the war.</param>
[Collection]
public record OutfitWar
(
    [property: Key] uint OutfitWarID,
    uint WorldID,
    LocaleString Title,
    uint ImageSetID,
    uint TeamSizeLimit,
    uint TeamSignupRequirement,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    IReadOnlyList<OutfitWar.Phase> Phases
)
{
    /// <summary>
    /// Represents information about an outfit war phase.
    /// </summary>
    /// <param name="Order">The order, within all the phases of the parent war, that this phase occurs.</param>
    /// <param name="Title">The title of the phase.</param>
    /// <param name="Description">The description of the phase.</param>
    /// <param name="StartTime">The start time of the phase.</param>
    /// <param name="EndTime">The end time of the phase.</param>
    public record Phase
    (
        uint Order,
        LocaleString Title,
        LocaleString Description,
        DateTimeOffset StartTime,
        DateTimeOffset EndTime
    );
}
