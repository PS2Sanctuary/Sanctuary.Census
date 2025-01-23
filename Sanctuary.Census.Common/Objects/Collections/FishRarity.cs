using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a fish rarity level.
/// </summary>
/// <param name="ID">The ID of the fish rarity level.</param>
/// <param name="Name">The rarity level's name.</param>
[Collection]
public record FishRarity
(
    [property: JoinKey] uint ID,
    LocaleString Name
) : ISanctuaryCollection;
