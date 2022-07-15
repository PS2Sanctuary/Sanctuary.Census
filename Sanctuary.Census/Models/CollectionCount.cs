namespace Sanctuary.Census.Models;

/// <summary>
/// A simple Census-like model for a collection count.
/// </summary>
/// <param name="Count">The number of elements in the collection.</param>
public readonly record struct CollectionCount(int Count)
{
    /// <summary>
    /// Implicitly converts an integer value to a <see cref="CollectionCount"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static implicit operator CollectionCount(int value)
        => new(value);
}
