using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents facility type data.
/// </summary>
/// <param name="FacilityTypeId">The ID of the facility type.</param>
/// <param name="Description">The description of the facility type.</param>
[Collection]
[Description("Contains information about facility types. Note that this collection is manually updated.")]
public record FacilityType
(
    [property: Key] byte FacilityTypeId,
    string Description
) : ISanctuaryCollection;
