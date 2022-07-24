using Sanctuary.Census.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Models.Collections;

/// <summary>
/// Represents experience data.
/// </summary>
/// <param name="ExperienceID">The ID of the experience point.</param>
/// <param name="AwardTypeID">The ID of the award that this experience point grants.</param>
/// <param name="Description">The description of this experience point.</param>
/// <param name="XP">The amount of XP granted by this experience point.</param>
/// <param name="IsNotableEvent">Indicates whether this experience point denotes an important player action.</param>
[Collection(PrimaryJoinField = nameof(Experience.ExperienceID))]
public record Experience
(
    [property:Key] uint ExperienceID,
    uint AwardTypeID,
    LocaleString Description,
    float XP,
    bool IsNotableEvent
);
