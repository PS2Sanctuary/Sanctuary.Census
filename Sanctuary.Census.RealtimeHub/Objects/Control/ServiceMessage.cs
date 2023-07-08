namespace Sanctuary.Census.RealtimeHub.Objects.Control;

/// <summary>
/// Represents a service message - the base unit for event messages sent over the event stream.
/// </summary>
/// <typeparam name="TPayload">The type of the payload.</typeparam>
/// <param name="Payload">The payload.</param>
public record ServiceMessage<TPayload>(TPayload Payload)
    : MessageBase("event", "serviceMessage");
