using Sanctuary.Census.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Models.Collections;

/// <summary>
/// Represents an outfit wars registration.
/// </summary>
/// <param name="OutfitID">The ID of the outfit.</param>
/// <param name="FactionID">The ID of the outfit's faction.</param>
/// <param name="RegistrationOrder">The order in which the outfit was registered.</param>
/// <param name="MemberSignupCount">The number of members who have signed up for the war.</param>
[Collection]
public record OutfitWarRegistration
(
    [property:Key] ulong OutfitID,
    uint FactionID,
    uint RegistrationOrder,
    uint MemberSignupCount
);
