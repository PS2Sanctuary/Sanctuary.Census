// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// Contains extension methods for primitive types.
/// </summary>
public static class PrimitiveExtensions
{
    /// <summary>
    /// Converts a floating-point value to a nullable decimal value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="nullValue">The value to consider as null.</param>
    /// <returns>The converted value.</returns>
    public static decimal? ToNullableDecimal(this float value, float nullValue = 0)
        => Math.Abs(value - nullValue) < 0.001
            ? null
            : new decimal(value);

    /// <summary>
    /// Checks whether a uint matches a nullable constraint.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="nullValue">The value to consider as null.</param>
    /// <returns>The converted, possibly null value.</returns>
    public static uint? ToNullableUInt(this uint value, uint nullValue = 0)
        => value == nullValue
            ? null
            : value;

    /// <summary>
    /// Checks whether an int matches a nullable constraint.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="nullValue">The value to consider as null.</param>
    /// <returns>The converted, possibly null value.</returns>
    public static int? ToNullableInt(this int value, int nullValue = 0)
        => value == nullValue
            ? null
            : value;

    /// <summary>
    /// Checks whether a ushort matches a nullable constraint.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="nullValue">The value to consider as null.</param>
    /// <returns>The converted, possibly null value.</returns>
    public static ushort? ToNullableUShort(this ushort value, ushort nullValue = 0)
        => value == nullValue
            ? null
            : value;

    /// <summary>
    /// Checks whether a short matches a nullable constraint.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="nullValue">The value to consider as null.</param>
    /// <returns>The converted, possibly null value.</returns>
    public static short? ToNullableShort(this short value, short nullValue = 0)
        => value == nullValue
            ? null
            : value;

    /// <summary>
    /// Checks whether a byte matches a nullable constraint.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="nullValue">The value to consider as null.</param>
    /// <returns>The converted, possibly null value.</returns>
    public static byte? ToNullableByte(this byte value, byte nullValue = 0)
        => value == nullValue
            ? null
            : value;
}
