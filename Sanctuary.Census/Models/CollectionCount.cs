namespace Sanctuary.Census.Api.Models;

/// <summary>
/// A simple Census-like model for a collection count.
/// </summary>
/// <param name="Count">The number of elements in the collection.</param>
public readonly record struct CollectionCount(ulong Count)
{
    /// <summary>
    /// Implicitly converts an int value to a <see cref="CollectionCount"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static implicit operator CollectionCount(int value)
        => new((ulong)value);

    /// <summary>
    /// Implicitly converts a long value to a <see cref="CollectionCount"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static implicit operator CollectionCount(long value)
        => new((ulong)value);
}
