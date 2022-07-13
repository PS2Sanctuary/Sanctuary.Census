using System;

namespace Sanctuary.Census.Exceptions;

/// <summary>
/// Indicates that data was missing from a cache.
/// </summary>
public class MissingCacheDataException : Exception
{
    /// <summary>
    /// Gets the type that was missing from the cache.
    /// </summary>
    public Type MissingType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingCacheDataException"/> class.
    /// </summary>
    /// <param name="missingType">The type that was missing from the cache.</param>
    public MissingCacheDataException(Type missingType)
        : base($"The {missingType} type was not present in the cache")
    {
        MissingType = missingType;
    }
}
