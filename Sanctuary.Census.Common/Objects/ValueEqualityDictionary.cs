using System;
using System.Collections.Generic;

namespace Sanctuary.Census.Common.Objects;

/// <summary>
/// Represents a dictionary that uses value equality.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
public class ValueEqualityDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    where TKey : notnull
{
    /// <inheritdoc />
    public ValueEqualityDictionary()
        : base()
    {
    }

    /// <inheritdoc />
    public ValueEqualityDictionary(IDictionary<TKey, TValue> dictionary)
        : base(dictionary)
    {
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not Dictionary<TKey, TValue> dict)
            return false;

        foreach (TKey key in Keys)
        {
            if (!dict.ContainsKey(key))
                return false;

            if (this[key] is null && dict[key] is null)
                continue;

            if (this[key]?.Equals(dict[key]) == false)
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();

        foreach ((TKey key, TValue value) in this)
        {
            hash.Add(key);
            hash.Add(value);
        }

        return hash.ToHashCode();
    }
}
