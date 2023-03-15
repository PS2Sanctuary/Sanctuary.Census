using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a record of a fuck up.
/// </summary>
/// <param name="OopsId">The ID of the oops record.</param>
/// <param name="FuckUpId">The ID of the fuck up.</param>
/// <param name="Cost">The resulting cost of the fuck up.</param>
/// <param name="CurrencyTypeId">The currency used to pay off the <paramref name="Cost"/>.</param>
/// <param name="CurrencyType">Describes the currency used to pay off the <paramref name="Cost"/>.</param>
/// <param name="EventMagnitude">The magnitude of the fuck up.</param>
/// <param name="CulpritCharacterId">The character who performed the fuck up.</param>
/// <param name="Notes">Notes about the fuck up.</param>
[Collection]
public record Oops
(
    uint OopsId,
    uint FuckUpId,
    long Cost,
    uint CurrencyTypeId,
    string CurrencyType,
    string EventMagnitude,
    ulong CulpritCharacterId,
    string Notes
) : ISanctuaryCollection;
