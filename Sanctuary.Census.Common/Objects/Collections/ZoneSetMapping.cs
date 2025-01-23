using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a zone set mapping.
/// </summary>
/// <param name="ID">The ID of the mapping entry.</param>
/// <param name="ZoneSetID">The ID of the zone set.</param>
/// <param name="ZoneType">The type of the zone.</param>
/// <param name="ZoneID">The ID of the zone that is part of the set.</param>
[Collection]
public partial record ZoneSetMapping
(
    [property: JoinKey] uint ID,
    [property: JoinKey] uint ZoneSetID,
    uint ZoneType,
    [property: JoinKey] uint ZoneID
) : ISanctuaryCollection;
