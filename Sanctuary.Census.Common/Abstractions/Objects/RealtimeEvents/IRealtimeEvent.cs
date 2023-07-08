namespace Sanctuary.Census.Common.Abstractions.Objects.RealtimeEvents;

/// <summary>
/// Represents an event-stream event.
/// </summary>
public interface IRealtimeEvent
{
    /// <summary>
    /// Gets the name of the event.
    /// </summary>
    public string EventName { get; }

    /// <summary>
    /// The ID of the world on which the realtime entry was generated.
    /// </summary>
    uint WorldId { get; }

    /// <summary>
    /// The time at which the realtime entry was generated.
    /// </summary>
    long Timestamp { get; }
}
