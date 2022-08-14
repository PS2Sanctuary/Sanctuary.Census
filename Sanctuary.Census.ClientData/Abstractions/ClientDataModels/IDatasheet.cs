using System;
using System.Collections.Generic;

namespace Sanctuary.Census.ClientData.Abstractions.ClientDataModels;

/// <summary>
/// Represents a datasheet model.
/// </summary>
/// <typeparam name="T">The type of the model.</typeparam>
public interface IDatasheet<T> where T : class
{
    /// <summary>
    /// Deserializes the datasheet from the given buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <returns>The list of deserialized models.</returns>
    abstract static List<T> Deserialize(ReadOnlySpan<byte> buffer);
}
