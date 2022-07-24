using Sanctuary.Census.Attributes;

namespace Sanctuary.Census.Models.Collections;

/// <summary>
/// Represents a mapping between a <see cref="FireGroup"/> and a <see cref="FireMode2"/>.
/// </summary>
/// <param name="FireGroupId">The ID of the fire group.</param>
/// <param name="FireModeId">The ID of the fire mode.</param>
/// <param name="FireModeIndex">The index of the fire mode within the mapping list.</param>
[Collection(PrimaryJoinField = nameof(FireGroupToFireMode.FireGroupId))]
public record FireGroupToFireMode
(
    uint FireGroupId,
    uint FireModeId,
    uint FireModeIndex
);
