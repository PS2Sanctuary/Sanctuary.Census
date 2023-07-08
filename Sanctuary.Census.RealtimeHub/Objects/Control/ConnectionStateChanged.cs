namespace Sanctuary.Census.RealtimeHub.Objects.Control;

/// <summary>
/// Represents a change in the connection state of a client.
/// </summary>
/// <param name="Connected">Whether the client is connected to the event service.</param>
public record ConnectionStateChanged(bool Connected)
    : MessageBase("push", "connectionStateChanged");
