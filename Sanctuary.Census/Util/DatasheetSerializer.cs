using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sanctuary.Census.Util;

/// <summary>
/// A reflection-based helper for serializing types to and from
/// the datasheet text format.
/// </summary>
public static class DatasheetSerializer
{
    private static readonly byte[] HeaderIdentifier = { (byte)'#', (byte)'*' };
    private static readonly Dictionary<Type, Func<string, object>[]> TypeCtorParamValueConverters = new();
    private static readonly Dictionary<Type, string[]> TypeCtorLowerCaseParamNames = new();

    /// <summary>
    /// Deserializes a UTF8 datasheet to the given type.
    /// </summary>
    /// <param name="datasheet"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static List<T> Deserialize<T>(ReadOnlySpan<byte> datasheet)
    {
        List<T> objects = new();
        SpanReader<byte> reader = new(datasheet);

        IReadOnlyList<string?> header = GetLineContents(ref reader, out bool hadHeaderIndicator);
        if (!hadHeaderIndicator)
            throw new FormatException("No header found");

        IReadOnlyDictionary<int, int> positionMap = MapHeaderToConstructor<T>(header);
        string?[] values = new string?[positionMap.Count];
        int linePosition = 0;

        while (!reader.End)
        {
            reader.IsNext((byte)'\r', true);
            if (reader.IsNext((byte)'\n', true))
            {
                objects.Add(ParseToObject<T>(values));
                linePosition = 0;
                values = new string?[positionMap.Count];
            }

            if (!reader.TryReadTo(out ReadOnlySpan<byte> element, (byte)'^'))
                break;

            if (positionMap.TryGetValue(linePosition++, out int ctorIndex))
            {
                values[ctorIndex] = element.Length == 0
                    ? null
                    : Encoding.UTF8.GetString(element);
            }
        }

        return objects;
    }

    private static IReadOnlyDictionary<int, int> MapHeaderToConstructor<T>(IReadOnlyList<string?> header)
    {
        Dictionary<int, int> map = new();
        Type tType = typeof(T);

        if (!TypeCtorLowerCaseParamNames.TryGetValue(tType, out string[]? ctorParamLowerNames))
        {
            ParameterInfo[] ctorParams = typeof(T).GetConstructors()[0]
                .GetParameters()
                .Where(p => p.Name is not null)
                .OrderBy(p => p.Position)
                .ToArray();

            ctorParamLowerNames = ctorParams.Select(p => p.Name!.ToLower())
                .ToArray();

            TypeCtorParamValueConverters.Add(tType, GetValueConverters(ctorParams));
            TypeCtorLowerCaseParamNames.Add(tType, ctorParamLowerNames);
        }

        for (int i = 0; i < header.Count; i++)
        {
            if (header[i] is null)
                continue;

            string cleaned = header[i]!.Replace("_", string.Empty).ToLower();
            int ctorIndex = Array.IndexOf(ctorParamLowerNames, cleaned);

            if (ctorIndex > -1)
                map[i] = ctorIndex;
        }

        return map;
    }

    private static Func<string, object>[] GetValueConverters(IReadOnlyList<ParameterInfo> parameters)
    {
        Func<string, object> GetConverter(Type pType)
        {
            if (pType == typeof(bool))
                return ParseBoolean;
            else if (pType == typeof(byte))
                return s => byte.Parse(s);
            else if (pType == typeof(sbyte))
                return s => sbyte.Parse(s);
            else if (pType == typeof(ushort))
                return s => ushort.Parse(s);
            else if (pType == typeof(short))
                return s => short.Parse(s);
            else if (pType == typeof(uint))
                return s => uint.Parse(s);
            else if (pType == typeof(int))
                return s => int.Parse(s);
            else if (pType == typeof(ulong))
                return s => ulong.Parse(s);
            else if (pType == typeof(long))
                return s => long.Parse(s);
            else if (pType == typeof(float))
                return s => float.Parse(s);
            else if (pType == typeof(double))
                return s => double.Parse(s);
            else if (pType == typeof(decimal))
                return s => decimal.Parse(s);
            else if (pType == typeof(string))
                return s => s;
            else if (pType.IsEnum)
                return s => Enum.Parse(pType, s);
            else if (Nullable.GetUnderlyingType(pType) != null)
                return GetConverter(Nullable.GetUnderlyingType(pType)!);
            else
                throw new InvalidOperationException($"Parameters of type {pType} are not supported");
        }

        Func<string, object>[] converters = new Func<string, object>[parameters.Count];

        for (int i = 0; i < parameters.Count; i++)
            converters[i] = GetConverter(parameters[i].ParameterType);

        return converters;
    }

    private static IReadOnlyList<string?> GetLineContents(ref SpanReader<byte> reader, out bool hadHeaderIndicator)
    {
        List<string?> elements = new();
        hadHeaderIndicator = false;

        if (reader.IsNext(HeaderIdentifier))
        {
            hadHeaderIndicator = true;
            reader.Advance(HeaderIdentifier.Length);
        }

        while (reader.TryReadTo(out ReadOnlySpan<byte> element, (byte) '^'))
        {
            if (element.Length == 0)
                elements.Add(null);
            else
                elements.Add(Encoding.UTF8.GetString(element));

            reader.IsNext((byte)'\r', true);
            if (reader.IsNext((byte)'\n', true))
                return elements;
        }

        return elements;
    }

    private static T ParseToObject<T>(IReadOnlyList<string?> ctorParamValues)
    {
        Func<string, object>[] converters = TypeCtorParamValueConverters[typeof(T)];
        if (converters.Length != ctorParamValues.Count)
            throw new Exception("Converter count is not equal to param values count. This shouldn't have occured!");

        object?[] ctorParams = new object[ctorParamValues.Count];
        for (int i = 0; i < ctorParamValues.Count; i++)
        {
            try
            {
                ctorParams[i] = ctorParamValues[i] is null
                    ? null
                    : converters[i](ctorParamValues[i]!);
            }
            catch (Exception fex)
            {
                string paramName = TypeCtorLowerCaseParamNames[typeof(T)][i];
                throw new FormatException
                (
                    $"Failed to convert value {ctorParamValues[i]} for param {paramName}. " +
                    "See inner exception for more details.",
                    fex
                );
            }
        }

        T? value = (T?)Activator.CreateInstance(typeof(T), ctorParams);
        return value ?? throw new Exception("Failed to create instance of T");
    }

    private static object ParseBoolean(string value)
        => value switch
        {
            "0" => false,
            "1" => true,
            _ => bool.Parse(value)
        };
}
