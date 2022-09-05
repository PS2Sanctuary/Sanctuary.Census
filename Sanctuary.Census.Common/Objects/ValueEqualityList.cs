using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanctuary.Census.Common.Objects;

/// <summary>
/// Represents a list that uses value equality.
/// </summary>
/// <typeparam name="T">The underlying type of the list.</typeparam>
public class ValueEqualityList<T> : List<T>
{
    /// <inheritdoc />
    public ValueEqualityList()
        : base()
    {
    }

    /// <inheritdoc />
    public ValueEqualityList(IEnumerable<T> collection)
        : base(collection)
    {
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is ValueEqualityList<T> list
           && list.SequenceEqual(this);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (T element in this)
            hash.Add(element);

        return hash.ToHashCode();
    }
}
