using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents data pertaining to the capture status of a zone map.
/// </summary>
/// <param name="WorldId">The ID of the world.</param>
/// <param name="ZoneId">The ID of the zone.</param>
/// <param name="ZoneInstance">The instance of the zone.</param>
/// <param name="Timestamp">The unix seconds timestamp at which the map data was generated.</param>
/// <param name="MapRegionId">The ID of the map region.</param>
/// <param name="OwningFactionId">The ID of the faction that owns the facility.</param>
/// <param name="IsContested">A value indicating whether the facility is currently being contested.</param>
/// <param name="ContestingFactionId">The ID of the faction that is contesting the facility.</param>
/// <param name="CaptureTimeMs">
/// The full time that the facility currently takes to be captured. May be <c>-1</c> if the facility
/// is not currently contested, or the full capture time as of when the facility was last captured.
/// </param>
/// <param name="RemainingCaptureTimeMs">
/// The current amount of time left in the capture. This value is decremented
/// as the capture proceeds. <c>-1</c> if the facility is not currently contested.
/// </param>
/// <param name="CtfFlags">The number of CTF flag deposits required to capture the facility.</param>
/// <param name="RemainingCtfFlags">The remaining CTF flags that must be deposited to capture the facility.</param>
[Collection]
public record MapState
(
    [property: JoinKey] uint WorldId,
    [property: JoinKey] ushort ZoneId,
    ushort ZoneInstance,
    long Timestamp,
    [property: JoinKey] uint MapRegionId,
    byte OwningFactionId,
    bool IsContested,
    byte ContestingFactionId,
    int CaptureTimeMs,
    int RemainingCaptureTimeMs,
    int CtfFlags,
    int RemainingCtfFlags
) : IRealtimeCollection;
