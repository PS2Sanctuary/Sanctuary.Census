using System.Collections.Generic;

namespace Sanctuary.Census.RealtimeHub.Objects.Control;

/// <summary>
/// Represents an ESS heartbeat, intended to convey state information and keep connections alive.
/// </summary>
/// <param name="Online">A map of endpoint names to their online status.</param>
/// <param name="Timestamp">The time at which the heartbeat was generated.</param>
public record Heartbeat(IReadOnlyDictionary<string, bool> Online, long Timestamp)
    : MessageBase("event", "heartbeat");
