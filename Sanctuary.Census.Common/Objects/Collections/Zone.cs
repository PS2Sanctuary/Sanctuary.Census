using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents information about a zone.
/// </summary>
/// <param name="ZoneID">The ID of the zone.</param>
/// <param name="Code">The zone's internal name.</param>
/// <param name="HexSize">The size of the zone's hexes.</param>
/// <param name="Name">The localized name.</param>
/// <param name="Description">The localized description.</param>
/// <param name="GeometryId">The geometry ('terrain') ID of the zone. Used to identify non-static zones.</param>
/// <param name="Type">The type of the zone.</param>
/// <param name="Dynamic">Indicates whether the zone is dynamic.</param>
[Collection]
public record Zone
(
    [property: JoinKey] uint ZoneID,
    string Code,
    decimal HexSize,
    LocaleString Name,
    LocaleString Description,
    uint GeometryId,
    string Type,
    bool Dynamic
) : ISanctuaryCollection;
