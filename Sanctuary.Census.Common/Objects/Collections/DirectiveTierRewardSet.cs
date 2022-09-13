using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a reward of a directive tier.
/// </summary>
/// <param name="RewardSetID">The ID of the reward set.</param>
/// <param name="Name">The name of the reward set.</param>
/// <param name="ImageSetID">The ID of the reward set's image set.</param>
/// <param name="ImageID">The ID of the reward set's default image.</param>
/// <param name="ImagePath">The relative path to the reward set's default image.</param>
[Collection]
[Description("Contains rewards obtained upon completion of a directive_tier")]
public record DirectiveTierRewardSet
(
    [property: Key] uint RewardSetID,
    LocaleString? Name,
    [property: Key] uint ImageSetID,
    [property: Key] uint? ImageID,
    string? ImagePath
) : ISanctuaryCollection;
