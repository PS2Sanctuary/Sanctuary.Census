﻿using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a reward of a directive tier.
/// </summary>
/// <param name="RewardSetID">The ID of the reward set.</param>
/// <param name="FactionId">The ID of the faction that this reward set is offered on.</param>
/// <param name="Name">The name of the reward set.</param>
/// <param name="ImageSetID">The ID of the reward set's image set.</param>
/// <param name="ImageID">The ID of the reward set's default image.</param>
/// <param name="ImagePath">The relative path to the reward set's default image.</param>
[Collection]
[Description("Contains rewards obtained upon completion of a directive_tier")]
public record DirectiveTierRewardSet
(
    [property: JoinKey] uint RewardSetID,
    [property: JoinKey] byte FactionId,
    LocaleString? Name,
    [property: JoinKey] uint ImageSetID,
    [property: JoinKey] uint? ImageID,
    string? ImagePath
) : ISanctuaryCollection;
