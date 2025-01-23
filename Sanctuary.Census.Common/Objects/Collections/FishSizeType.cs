using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a fish size level.
/// </summary>
/// <param name="ID">The ID of the fish size level.</param>
/// <param name="Name">The name of the size type.</param>
[Collection]
public record FishSizeType
(
    [property: JoinKey] uint ID,
    LocaleString Name
) : ISanctuaryCollection;
