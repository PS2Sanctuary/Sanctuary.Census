using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents an item-to-weapon mapping.
/// </summary>
/// <param name="ItemId">The ID of the item.</param>
/// <param name="WeaponId">The ID of the weapon.</param>
[Collection]
public record ItemToWeapon
(
    [property: JoinKey] uint ItemId,
    [property: JoinKey] uint WeaponId
) : ISanctuaryCollection;
