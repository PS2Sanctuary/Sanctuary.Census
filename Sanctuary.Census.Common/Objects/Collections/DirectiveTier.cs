using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a tier of a <see cref="DirectiveTree"/>.
/// </summary>
/// <param name="DirectiveTreeID">The ID of the tree that the tier belongs to.</param>
/// <param name="DirectiveTierID">The ID of the tier.</param>
/// <param name="Name">The name of the tier.</param>
/// <param name="RewardSetID">The ID of the reward set granted by the tier.</param>
/// <param name="DirectivePoints">The number of directive points granted upon completing the tier.</param>
/// <param name="CompletionCount">The number of <see cref="Directive"/>s that must be completed to clear the tier.</param>
/// <param name="ImageSetID">The ID of the tier's image set.</param>
/// <param name="ImageID">The ID of the tier's default image.</param>
/// <param name="ImagePath">The relative path to the tier's default image.</param>
[Collection]
[Description("Represents a stage of a directive; e.g. the 'Novice' (bronze) tier. Join to " +
    "directive_tier_reward_set (and subsequently directive_tier_reward) for reward information")]
public record DirectiveTier
(
    [property: JoinKey] uint DirectiveTreeID,
    [property: JoinKey] uint DirectiveTierID,
    LocaleString Name,
    [property: JoinKey] uint? RewardSetID,
    uint DirectivePoints,
    uint CompletionCount,
    [property: JoinKey] uint ImageSetID,
    [property: JoinKey] uint? ImageID,
    string? ImagePath
) : ISanctuaryCollection;
