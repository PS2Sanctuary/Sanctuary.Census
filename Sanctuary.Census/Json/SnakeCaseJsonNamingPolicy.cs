using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Sanctuary.Census.Json;

/// <summary>
/// A naming policy which converts camel and pascal-case names
/// into a snake-case representation.
/// </summary>
public class SnakeCaseJsonNamingPolicy : JsonNamingPolicy
{
    /// <inheritdoc />
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        StringBuilder sb = new();
        Queue<int> wordBoundaries = new();

        char? previousLetter = null;
        int lastAddedBoundary = 0;
        for (int i = 0; i < name.Length; i++)
        {
            char letter = name[i];

            if (previousLetter is not null && char.IsUpper(previousLetter.Value) && char.IsLower(letter) && i - 1 != lastAddedBoundary)
            {
                lastAddedBoundary = i - 1;
                wordBoundaries.Enqueue(lastAddedBoundary);
            }

            if (previousLetter is not null && char.IsLower(previousLetter.Value) && char.IsUpper(letter) && i != lastAddedBoundary)
            {
                lastAddedBoundary = i;
                wordBoundaries.Enqueue(lastAddedBoundary);
            }

            // Split on the first of a sequence of digits
            if (letter is >= '1' and <= '9' && previousLetter is <= '1' or >= '9')
            {
                lastAddedBoundary = i;
                wordBoundaries.Enqueue(lastAddedBoundary);
            }

            previousLetter = letter;
        }

        wordBoundaries.TryDequeue(out int nextWordBoundary);
        for (int i = 0; i < name.Length; i++)
        {
            char letter = name[i];

            if (i == nextWordBoundary)
            {
                wordBoundaries.TryDequeue(out nextWordBoundary);

                if (i != 0)
                    sb.Append('_');
            }

            sb.Append(char.ToLowerInvariant(letter));
        }

        return sb.ToString();
    }
}
