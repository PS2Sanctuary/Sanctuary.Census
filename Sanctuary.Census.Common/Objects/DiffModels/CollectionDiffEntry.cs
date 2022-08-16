using System;

namespace Sanctuary.Census.Common.Objects.DiffModels;

/// <summary>
/// Represents a diff entry.
/// </summary>
/// <param name="DiffTime">The time that the entry was generated at.</param>
/// <param name="CollectionName">The name of the collection that this entry was generated from.</param>
/// <param name="OldObject">The old object.</param>
/// <param name="NewObject">The new object.</param>
public record CollectionDiffEntry
(
    DateTime DiffTime,
    string CollectionName,
    object? OldObject,
    object? NewObject
);
