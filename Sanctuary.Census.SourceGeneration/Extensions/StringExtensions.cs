// ReSharper disable once CheckNamespace
namespace System;

public static class StringExtensions
{
    public static string ToSafeLowerCamel(this string value)
        => "@" + char.ToLowerInvariant(value[0]) + value.Substring(1);

    public static string CleanGeneratorString(this string value)
        => value.TrimEnd(' ', ',', '\r', '\n');
}
