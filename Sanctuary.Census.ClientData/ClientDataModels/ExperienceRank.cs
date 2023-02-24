using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client experience rank data.
/// </summary>
/// <param name="Id">The ID of the experience rank.</param>
/// <param name="Rank">The in-game display rank.</param>
/// <param name="XpMax">The maximum amount of owned XP that the rank can be used with.</param>
/// <param name="Title01">The name of the rank when displayed on a VS character.</param>
/// <param name="Title02">The name of the rank when displayed on an NC character.</param>
/// <param name="Title03">The name of the rank when displayed on a TR character.</param>
/// <param name="Title04">The name of the rank when displayed on an NSO character.</param>
/// <param name="RewardSet01">The reward set granted by attaining the rank on a VS character.</param>
/// <param name="RewardSet02">The reward set granted by attaining the rank on an NC character.</param>
/// <param name="RewardSet03">The reward set granted by attaining the rank on a TR character.</param>
/// <param name="RewardSet04">The reward set granted by attaining the rank on an NSO character.</param>
/// <param name="ImageSetIdVS">The image set of the rank when displayed on a VS character.</param>
/// <param name="ImageSetIdTR">The image set of the rank when displayed on a TR character.</param>
/// <param name="ImageSetIdNC">The image set of the rank when displayed on an NC character.</param>
/// <param name="ImageSetIdNSO">The image set of the rank when displayed on an NSO character.</param>
[Datasheet]
public partial record ExperienceRank
(
    uint Id,
    int Rank,
    decimal XpMax,
    uint Title01,
    uint Title02,
    uint Title03,
    uint Title04,
    uint RewardSet01,
    uint RewardSet02,
    uint RewardSet03,
    uint RewardSet04,
    uint ImageSetIdVS,
    uint ImageSetIdTR,
    uint ImageSetIdNC,
    uint ImageSetIdNSO
);
