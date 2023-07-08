namespace Sanctuary.Census.RealtimeHub.Objects.Control;

/// <summary>
/// Represents a base message.
/// </summary>
/// <param name="Service">The service that the message originated from.</param>
/// <param name="Type">The meta type of the payload.</param>
public abstract record MessageBase(string Service, string Type);
