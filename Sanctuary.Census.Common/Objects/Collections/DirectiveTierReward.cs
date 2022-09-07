using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a reward of a <see cref="DirectiveTierRewardSet"/>.
/// </summary>
/// <param name="RewardSetID">The ID of the reward set that the reward belongs to.</param>
/// <param name="ItemID">The ID of the item given by the reward.</param>
/// <param name="Name">The name of the reward.</param>
/// <param name="Quantity">The number of items awarded.</param>
/// <param name="ImageSetID">The ID of the reward's image set.</param>
/// <param name="ImageID">The ID of the reward's default image.</param>
/// <param name="ImagePath">The relative path of the reward's default image.</param>
[Collection]
[Description("Contains rewards from a directive_tier_reward_set")]
public record DirectiveTierReward
(
    [property: Key] uint RewardSetID,
    [property: Key] uint ItemID,
    LocaleString? Name,
    uint Quantity,
    uint ImageSetID,
    uint? ImageID,
    string? ImagePath
) : ISanctuaryCollection;
