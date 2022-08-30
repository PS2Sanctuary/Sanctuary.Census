using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

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
[Description("Information about outfit signups to the active outfit war")]
public record OutfitWarRegistration
(
    [property: Key] ulong OutfitID,
    uint FactionID,
    uint WorldID,
    uint RegistrationOrder,
    OutfitWarRegistration.RegistrationStatus Status,
    uint MemberSignupCount
) : ISanctuaryCollection
{
    /// <summary>
    /// Enumerates the possible registration status' of an outfit.
    /// </summary>
    public enum RegistrationStatus : byte
    {
        /// <summary>
        /// The outfit is fully signed up, and participating in the war.
        /// </summary>
        Full = 1,

        /// <summary>
        /// The outfit has met the member signup requirements, but is waiting for another
        /// outfit to do so before they can both become fully registered, in order to
        /// achieve an even number of registered outfits.
        /// </summary>
        WaitingOnNextFullReg = 2,

        /// <summary>
        /// The outfit is registered, but is yet to meet the member requirement.
        /// </summary>
        Partial = 5
    }
}
