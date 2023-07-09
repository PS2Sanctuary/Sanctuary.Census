using OneOf;
using System.Collections.Generic;

namespace Sanctuary.Census.RealtimeHub.Objects.Commands;

/// <summary>
/// Represents a subscribe command, for defining what events a client wishes to receive.
/// </summary>
/// <param name="EventNames">The event types that the client wishes to receive.</param>
/// <param name="Worlds">The worlds that the client wishes to receive events for.</param>
public record Subscribe
(
    IEnumerable<string>? EventNames = default,
    OneOf<All, IEnumerable<uint>>? Worlds = default
);
