using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client zone set mapping data.
/// </summary>
/// <param name="Id">The ID of the mapping entry.</param>
/// <param name="ZoneSet">The ID of the zone set.</param>
/// <param name="ZoneType">The type of the zone.</param>
/// <param name="ZoneId">The ID of the zone that is part of the set.</param>
[Datasheet]
public partial record ZoneSetMapping
(
    uint Id,
    uint ZoneSet,
    uint ZoneType,
    uint ZoneId
);
