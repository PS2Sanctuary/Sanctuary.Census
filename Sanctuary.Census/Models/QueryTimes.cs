using System;

namespace Sanctuary.Census.Models;

/// <summary>
/// Represents various times relevant to a query.
/// </summary>
/// <param name="TotalMS">The total time in milliseconds spent performing the database query.</param>
/// <param name="CollectionLastUpdated">The time that the queried collection was last updated.</param>
public record QueryTimes
(
    int TotalMS,
    long? CollectionLastUpdated
);
