﻿using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a mapping between a <see cref="Weapon"/> and a <see cref="FireGroup"/>.
/// </summary>
/// <param name="WeaponId">The ID of the weapon.</param>
/// <param name="FireGroupId">The ID of the fire group.</param>
/// <param name="FireGroupIndex">The index of the fire group within the mapping list.</param>
[Collection]
public record WeaponToFireGroup
(
    [property: JoinKey] uint WeaponId,
    [property: JoinKey] uint FireGroupId,
    uint FireGroupIndex
) : ISanctuaryCollection;
