using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents information about an outfit war.
/// </summary>
/// <param name="OutfitWarID">The ID of the outfit war.</param>
/// <param name="WorldID">The world that the war is occuring on.</param>
/// <param name="Title">The title of the outfit war.</param>
/// <param name="ImageSetID">The ID of the war's image set.</param>
/// <param name="OutfitSizeRequirement">The maximum number of players that can play in a match.</param>
/// <param name="OutfitSignupRequirement">The number of outfit members that must signup before an outfit can participate.</param>
/// <param name="StartTime">The start time of the war, as a unix seconds timestamp.</param>
/// <param name="EndTime">The end time of the war, as a unix seconds timestamp.</param>
/// <param name="Phases">The phases of the war.</param>
[Collection]
public record OutfitWar
(
    [property: Key] uint OutfitWarID,
    uint WorldID,
    LocaleString? Title,
    uint ImageSetID,
    uint OutfitSizeRequirement,
    uint OutfitSignupRequirement,
    ulong StartTime,
    ulong EndTime,
    IReadOnlyList<OutfitWar.Phase> Phases
)
{
    /// <summary>
    /// Represents information about an outfit war phase.
    /// </summary>
    /// <param name="Order">The order, within all the phases of the parent war, that this phase occurs.</param>
    /// <param name="Title">The title of the phase.</param>
    /// <param name="Description">The description of the phase.</param>
    /// <param name="StartTime">The start time of the phase, as a unix seconds timestamp.</param>
    /// <param name="EndTime">The end time of the phase, as a unix seconds timestamp.</param>
    [Collection]
    public record Phase
    (
        uint Order,
        LocaleString Title,
        LocaleString Description,
        ulong StartTime,
        ulong EndTime
    );

    /// <inheritdoc />
    public virtual bool Equals(OutfitWar? other)
        => other is not null
           && OutfitWarID.Equals(other.OutfitWarID)
           && WorldID.Equals(other.WorldID)
           && ((Title is not null && other.Title is not null && Title.Equals(other.Title)) || Title == other.Title)
           && ImageSetID.Equals(other.ImageSetID)
           && OutfitSizeRequirement.Equals(other.OutfitSizeRequirement)
           && OutfitSignupRequirement.Equals(other.OutfitSignupRequirement)
           && StartTime.Equals(other.StartTime)
           && EndTime.Equals(other.EndTime)
           && Phases.SequenceEqual(other.Phases);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hashCode = new();
        hashCode.Add(OutfitWarID);
        hashCode.Add(WorldID);
        hashCode.Add(Title);
        hashCode.Add(ImageSetID);
        hashCode.Add(OutfitSizeRequirement);
        hashCode.Add(OutfitSignupRequirement);
        hashCode.Add(StartTime);
        hashCode.Add(EndTime);
        foreach (Phase phase in Phases)
            hashCode.Add(phase);
        return hashCode.ToHashCode();
    }
}
