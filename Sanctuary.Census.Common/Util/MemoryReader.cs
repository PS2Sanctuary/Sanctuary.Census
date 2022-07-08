using System;
using System.Runtime.CompilerServices;

namespace Sanctuary.Census.Common.Util;

/// <summary>
/// Provides methods for reading binary and text data
/// out of a <see cref="ReadOnlyMemory{T}"/> with a focus
/// on performance and minimal or zero heap allocations.
/// </summary>
/// <typeparam name="T">The type of the read-only memory.</typeparam>
public partial struct MemoryReader<T> where T : unmanaged, IEquatable<T>
{
    /// <summary>
    /// Gets the underlying <see cref="ReadOnlyMemory{T}"/> for the reader.
    /// </summary>
    public readonly ReadOnlyMemory<T> Memory;

    /// <summary>
    /// Gets the number of items that this <see cref="MemoryReader{T}"/>
    /// has consumed from the underlying <see cref="Memory"/>.
    /// </summary>
    public int Consumed { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the reader has
    /// reached the end of the <see cref="Memory"/>.
    /// </summary>
    public readonly bool End => Consumed >= Memory.Length;

    /// <summary>
    /// Gets the number of remaining <typeparamref name="T"/>'s
    /// in the reader's <see cref="Memory"/>.
    /// </summary>
    public readonly int Remaining => Memory.Length - Consumed;

    /// <summary>
    /// Creates a <see cref="MemoryReader{T}"/> over the given <see cref="ReadOnlyMemory{T}"/>
    /// </summary>
    /// <param name="memory">The <see cref="ReadOnlyMemory{T}"/> to read.</param>
    public MemoryReader(ReadOnlyMemory<T> memory)
    {
        Memory = memory;
        Consumed = 0;
    }

    /// <summary>
    /// Forms a slice out of the current <see cref="MemoryReader{T}"/>,
    /// beginning at the current <see cref="Consumed"/>.
    /// </summary>
    /// <param name="length">The desired length of the slice.</param>
    /// <returns>
    /// A <see cref="MemoryReader{T}"/> backed by a slice of the current <see cref="Memory"/>.
    /// </returns>
    public MemoryReader<T> Slice(int length)
        => new MemoryReader<T>(Memory.Slice(Consumed, length));

    /// <summary>
    /// Forms a slice out of the current <see cref="MemoryReader{T}"/>.
    /// </summary>
    /// <param name="start">The index to begin the slice at.</param>
    /// <param name="length">The desired length of the slice.</param>
    /// <returns>
    /// A <see cref="MemoryReader{T}"/> backed by a slice of the current <see cref="Memory"/>.
    /// </returns>
    public MemoryReader<T> Slice(int start, int length)
        => new MemoryReader<T>(Memory.Slice(start, length));

    /// <summary>
    /// Advances the reader by the given number of items.
    /// </summary>
    /// <remarks>
    /// If the <paramref name="count"/> would move the reader past the end,
    /// it will simply move to the end of the <see cref="Memory"/>.
    /// </remarks>
    /// <param name="count">The number of items to move ahead by.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="count"/> is negative.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must not be negative");

        if (count > Remaining)
            Consumed = Memory.Span.Length;
        else
            Consumed += count;
    }

    /// <summary>
    /// Rewinds the reader by the given number of items.
    /// </summary>
    /// <remarks>
    /// If the <paramref name="count"/> would move the reader beyond the
    /// start, it will simply move no further without raising an exception.
    /// </remarks>
    /// <param name="count">The number of items to move backwards by.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="count"/> is negative.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Rewind(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must not be negative");

        if (count > Consumed)
            Consumed = 0;
        else
            Consumed -= count;
    }

    /// <summary>
    /// Attempts to peek at the next value without advancing the reader.
    /// </summary>
    /// <param name="value">The next value, or <c>default</c> if at the <see cref="End"/></param>
    /// <returns><c>False</c> if at the end of the reader, otherwise <c>True</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryPeek(out T value)
    {
        if (End)
        {
            value = default;
            return false;
        }

        value = Memory.Span[Consumed];
        return true;
    }

    /// <summary>
    /// Attempts to peek at the value at the given offset without advancing the reader.
    /// </summary>
    /// <param name="offset">The offset from the current <see cref="Consumed"/>.</param>
    /// <param name="value">
    /// The value, or <c>default</c> if the offset is beyond the <see cref="End"/> of the reader.
    /// </param>
    /// <returns><c>False</c> if the offset is beyond the <see cref="End"/> of the reader.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="offset"/> is negative.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryPeek(int offset, out T value)
    {
        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset must not be negative");

        if (offset >= Remaining)
        {
            value = default;
            return false;
        }

        value = Memory.Span[Consumed + offset];
        return true;
    }

    /// <summary>
    /// Attempts to read the next value and advance the reader.
    /// </summary>
    /// <param name="value">The value, or <c>default</c> if at the <see cref="End"/>.</param>
    /// <returns><c>False</c> if at the end of the reader, otherwise <c>True</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRead(out T value)
    {
        if (End)
        {
            value = default;
            return false;
        }

        value = Memory.Span[Consumed++];
        return true;
    }
}
