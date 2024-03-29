﻿using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a resistance to a damage source.
/// </summary>
/// <param name="ResistInfoId">The ID of the resistance info entry.</param>
/// <param name="ResistTypeId">The type of damage that this entry resists.</param>
/// <param name="ResistPercent">The percentage of damage that is negated.</param>
/// <param name="ResistAmount">A flat damage reduction applied after the <paramref name="ResistPercent"/> is applied.</param>
/// <param name="MultiplierWhenHeadshot">The multiplier that this resistance has against headshots.</param>
/// <param name="ImageSetId">The ID of the image set that the resistance uses. This may be shown on the HUD.</param>
/// <param name="ImageId">The ID of the resistance's default image.</param>
/// <param name="ImagePath">The relative path to the resistance's default image.</param>
[Collection]
public record ResistInfo
(
    [property: JoinKey] uint ResistInfoId,
    [property: JoinKey] ushort ResistTypeId,
    decimal ResistPercent,
    int? ResistAmount,
    decimal MultiplierWhenHeadshot,
    [property: JoinKey] uint? ImageSetId,
    [property: JoinKey] uint? ImageId,
    string? ImagePath
) : ISanctuaryCollection;
