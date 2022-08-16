using System;

namespace Sanctuary.Census.Common.Objects;

/// <summary>
/// Represents information about a collection.
/// </summary>
/// <param name="Name">The name of the collection.</param>
/// <param name="Description">A description of the collection.</param>
/// <param name="Count">The number of elements in the collection.</param>
/// <param name="LastUpdated">The time that the collection was last updated, as a unix seconds timestamp.</param>
public record Datatype
(
    string Name,
    string? Description,
    long Count,
    long LastUpdated
);
