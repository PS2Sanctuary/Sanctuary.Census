namespace Sanctuary.Census.Models;

/// <summary>
/// Represents information about a new collection, intended for
/// use within a diff entry.
/// </summary>
/// <param name="CollectionName">The name of the collection.</param>
/// <param name="Path">The path to the collection.</param>
public record NewCollection
(
    string CollectionName,
    string Path
);
