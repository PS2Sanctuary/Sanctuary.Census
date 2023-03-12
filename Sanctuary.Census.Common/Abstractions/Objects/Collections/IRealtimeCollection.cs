namespace Sanctuary.Census.Common.Abstractions.Objects.Collections;

/// <summary>
/// Represents a collection that contains 'realtime' data.
/// </summary>
public interface IRealtimeCollection : ISanctuaryCollection
{
    /// <summary>
    /// The ID of the world on which the realtime entry was generated.
    /// </summary>
    uint WorldId { get; }

    /// <summary>
    /// The time at which the realtime entry was generated.
    /// </summary>
    long Timestamp { get; }
}
