using Sanctuary.Census.Attributes;
using Sanctuary.Zone.Packets.OutfitWars;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Models.Collections;

/// <summary>
/// Represents an outfit wars registration.
/// </summary>
/// <param name="OutfitID">The ID of the outfit.</param>
/// <param name="FactionID">The ID of the outfit's faction.</param>
/// <param name="WorldID">The world that the outfit is registered on.</param>
/// <param name="RegistrationOrder">The order in which the outfit was registered.</param>
/// <param name="Status">The registration status of the outfit.</param>
/// <param name="MemberSignupCount">The number of members who have signed up for the war.</param>
[Collection]
public record OutfitWarRegistration
(
    [property:Key] ulong OutfitID,
    uint FactionID,
    uint WorldID,
    uint RegistrationOrder,
    RegistrationStatus Status,
    uint MemberSignupCount
);
