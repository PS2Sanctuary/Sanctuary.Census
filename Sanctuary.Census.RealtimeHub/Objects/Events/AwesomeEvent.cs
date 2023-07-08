using Sanctuary.Census.Common.Abstractions.Objects.RealtimeEvents;
using System;

namespace Sanctuary.Census.RealtimeHub.Objects.Events;

/// <summary>
/// An event stating how awesome someone is.
/// </summary>
/// <param name="AwesomenessRating">The awesomeness rating.</param>
public record AwesomeEvent(string AwesomenessRating) : IRealtimeEvent
{
    /// <inheritdoc />
    public string EventName => "Awesomeness";

    /// <inheritdoc />
    public long Timestamp => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    /// <inheritdoc />
    public uint WorldId => 0;
}
