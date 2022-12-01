using System;
using System.Buffers;
using System.Text.Json;

namespace Sanctuary.Census.Common.Json;

/// <summary>
/// A naming policy which converts camel and pascal-case names
/// into a snake-case representation.
/// </summary>
public class SnakeCaseJsonNamingPolicy : JsonNamingPolicy
{
    private enum InputState
    {
        Start,
        Lower,
        Upper
    }

    private const int MAX_STACKALLOC_SIZE = 128;

    /// <summary>
    /// Gets a default instance of the <see cref="SnakeCaseJsonNamingPolicy"/>.
    /// </summary>
    public static readonly SnakeCaseJsonNamingPolicy Default = new();

    /// <inheritdoc />
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        int maxOutputSize = name.Length + name.Length / 2;
        string result;

        if (maxOutputSize > MAX_STACKALLOC_SIZE)
        {
            char[] output = ArrayPool<char>.Shared.Rent(maxOutputSize);
            result = ConvertCore(name, output);
            ArrayPool<char>.Shared.Return(output);
        }
        else
        {
            Span<char> output = stackalloc char[maxOutputSize];
            result = ConvertCore(name, output);
        }

        return result;
    }

    private static string ConvertCore(ReadOnlySpan<char> name, Span<char> outputBuffer)
    {
        int index = 0;
        InputState state = InputState.Start;

        for (int i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) || char.IsDigit(name[i]))
            {
                switch (state)
                {
                    case InputState.Upper:
                    {
                        bool hasNext = i + 1 < name.Length;
                        if (i > 0 && hasNext)
                        {
                            char nextChar = name[i + 1];
                            if (!char.IsUpper(nextChar) && !char.IsDigit(nextChar) && nextChar != '_')
                            {
                                outputBuffer[index] = '_';
                                index++;
                            }
                        }

                        break;
                    }
                    case InputState.Lower:
                    {
                        outputBuffer[index] = '_';
                        index++;
                        break;
                    }
                }

                char c = char.ToLowerInvariant(name[i]);

                outputBuffer[index] = c;
                index++;

                state = InputState.Upper;
            }
            else if (name[i] == '_')
            {
                outputBuffer[index] = '_';
                index++;
                state = InputState.Start;
            }
            else
            {
                outputBuffer[index] = name[i];
                index++;
                state = InputState.Lower;
            }
        }

        return new string(outputBuffer[..index]);
    }
}
