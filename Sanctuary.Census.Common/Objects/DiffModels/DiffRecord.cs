using System;
using System.Collections.Generic;

namespace Sanctuary.Census.Common.Objects.DiffModels;

/// <summary>
/// Represents a diff operation.
/// </summary>
/// <param name="GeneratedAt">The time that the diff was generated at.</param>
/// <param name="CollectionChangeCounts">The number of changes made to each modified collection.</param>
public record DiffRecord
(
    DateTime GeneratedAt,
    IReadOnlyDictionary<string, uint> CollectionChangeCounts
);
