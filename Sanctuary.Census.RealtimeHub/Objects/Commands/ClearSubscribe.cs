using OneOf;
using System.Collections.Generic;

namespace Sanctuary.Census.RealtimeHub.Objects.Commands;

/// <summary>
/// Represents a clear subscription command, for defining a set of events that the client
/// no longer wishes to be subscribed to.
/// </summary>
/// <param name="All">A value indicating whether or not all subscriptions will be cleared.</param>
/// <param name="EventNames">The event subscriptions to clear.</param>
/// <param name="Worlds">The world subscriptions to clear.</param>
public record ClearSubscribe
(
    bool All = false,
    IEnumerable<string>? EventNames = default,
    OneOf<All, IEnumerable<uint>>? Worlds = default
);
