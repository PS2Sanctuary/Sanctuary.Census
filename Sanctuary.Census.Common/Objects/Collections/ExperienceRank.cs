using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents an in-game experience rank.
/// </summary>
/// <param name="Id">The ID of the rank.</param>
/// <param name="Rank">The in-game display level of the rank.</param>
/// <param name="PrestigeLevel">The in-game prestige level that the rank is compatible with.</param>
/// <param name="XpMax">The maximum amount of owned XP that the rank can be used with.</param>
/// <param name="VS">Display attributes of the rank when used on a VS character.</param>
/// <param name="VsImagePath">A relative path to the rank's default image, when used on a VS character.</param>
/// <param name="NC">Display attributes of the rank when used on an NC character.</param>
/// <param name="NcImagePath">A relative path to the rank's default image, when used on an NC character.</param>
/// <param name="TR">Display attributes of the rank when used on a TR character.</param>
/// <param name="TrImagePath">A relative path to the rank's default image, when used on a TR character.</param>
/// <param name="NSO">Display attributes of the rank when used on an NSO character.</param>
/// <param name="NsoImagePath">A relative path to the rank's default image, when used on an NSO character.</param>
[Collection]
public record ExperienceRank
(
    uint Id,
    int Rank,
    int PrestigeLevel,
    decimal XpMax,
    ExperienceRank.FactionInfo VS,
    string? VsImagePath,
    ExperienceRank.FactionInfo NC,
    string? NcImagePath,
    ExperienceRank.FactionInfo TR,
    string? TrImagePath,
    ExperienceRank.FactionInfo NSO,
    string? NsoImagePath
) : ISanctuaryCollection
{
    /// <summary>
    /// Represents faction-specific information about an <see cref="ExperienceRank"/>.
    /// </summary>
    /// <param name="Title">The title of the rank.</param>
    /// <param name="ImageSetId">The ID of the rank's image set.</param>
    /// <param name="ImageId">The ID of the rank's image.</param>
    [Collection(IsNestedType = true)]
    public record FactionInfo(LocaleString? Title, uint? ImageSetId, uint? ImageId);
}
