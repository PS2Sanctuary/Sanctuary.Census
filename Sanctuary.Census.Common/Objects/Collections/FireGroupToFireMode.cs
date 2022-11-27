using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a mapping between a <see cref="FireGroup"/> and a <see cref="FireMode2"/>.
/// </summary>
/// <param name="FireGroupId">The ID of the fire group.</param>
/// <param name="FireModeId">The ID of the fire mode.</param>
/// <param name="FireModeIndex">The index of the fire mode within the mapping list.</param>
[Collection]
[Description("Links a fire_group to the fire_mode_2 entries it uses.")]
public record FireGroupToFireMode
(
    [property: JoinKey] uint FireGroupId,
    [property: JoinKey] uint FireModeId,
    uint FireModeIndex
) : ISanctuaryCollection;
