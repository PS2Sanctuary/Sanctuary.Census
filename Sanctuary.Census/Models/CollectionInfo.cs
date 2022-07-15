namespace Sanctuary.Census.Models;

/// <summary>
/// Represents information about a collection.
/// </summary>
/// <param name="Name">The name of the collection.</param>
/// <param name="Count">The number of elements in the collection.</param>
public record CollectionInfo
(
    string Name,
    int Count
);
