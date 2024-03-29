﻿using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents information about an outfit war.
/// </summary>
/// <param name="OutfitWarID">The ID of the outfit war.</param>
/// <param name="WorldID">The world that the war is occuring on.</param>
/// <param name="PrimaryRoundID">The primary round ID of the war.</param>
/// <param name="Title">The title of the outfit war.</param>
/// <param name="OutfitSizeRequirement">The maximum number of players that can play in a match.</param>
/// <param name="OutfitSignupRequirement">The number of outfit members that must signup before an outfit can participate.</param>
/// <param name="StartTime">The start time of the war, as a unix seconds timestamp.</param>
/// <param name="EndTime">The end time of the war, as a unix seconds timestamp.</param>
/// <param name="ImageSetID">The ID of the war's image set.</param>
/// <param name="ImageID">The ID of the directive's default image.</param>
/// <param name="ImagePath">The relative path to the directive's default image.</param>
/// <param name="Phases">The phases of the war.</param>
[Collection]
[Description("Contains basic descriptive information about an outfit war")]
public record OutfitWar
(
    [property: JoinKey] uint OutfitWarID,
    [property: JoinKey] uint WorldID,
    ulong PrimaryRoundID,
    LocaleString? Title,
    uint OutfitSizeRequirement,
    uint OutfitSignupRequirement,
    ulong StartTime,
    ulong EndTime,
    [property: JoinKey] uint ImageSetID,
    [property: JoinKey] uint? ImageID,
    string? ImagePath,
    ValueEqualityList<OutfitWar.Phase> Phases
) : ISanctuaryCollection
{
    /// <summary>
    /// Represents information about an outfit war phase.
    /// </summary>
    /// <param name="Order">The order, within all the phases of the parent war, that this phase occurs.</param>
    /// <param name="Title">The title of the phase.</param>
    /// <param name="Description">The description of the phase.</param>
    /// <param name="StartTime">The start time of the phase, as a unix seconds timestamp.</param>
    /// <param name="EndTime">The end time of the phase, as a unix seconds timestamp.</param>
    [Collection(IsNestedType = true)]
    public record Phase
    (
        uint Order,
        LocaleString Title,
        LocaleString Description,
        ulong StartTime,
        ulong EndTime
    );
}
