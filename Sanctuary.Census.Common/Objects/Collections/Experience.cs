﻿using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents experience data.
/// </summary>
/// <param name="ExperienceID">The ID of the experience point.</param>
/// <param name="AwardTypeID">The ID of the award that this experience point grants.</param>
/// <param name="Description">The description of this experience point.</param>
/// <param name="LocalizedDescription">The description of this experience point.</param>
/// <param name="XP">The amount of XP granted by this experience point.</param>
/// <param name="IsNotableEvent">Indicates whether this experience point denotes an important player action.</param>
[Collection]
public record Experience
(
    [property: JoinKey] uint ExperienceID,
    uint AwardTypeID,
    string? Description,
    LocaleString? LocalizedDescription,
    decimal XP,
    bool IsNotableEvent
) : ISanctuaryCollection;
