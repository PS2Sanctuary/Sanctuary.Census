using System.Text.Json.Serialization;

namespace Sanctuary.Census.Common.Objects.CommonModels;

/// <summary>
/// Represents a globalized string model.
/// </summary>
/// <param name="ID">The ID of the locale string.</param>
/// <param name="English">The English translation of the string.</param>
/// <param name="German">The German translation of the string.</param>
/// <param name="French">The French translation of the string.</param>
/// <param name="Italian">The Italian translation of the string.</param>
/// <param name="Spanish">The Spanish translation of the string.</param>
/// <param name="Turkish">The Turkish translation of the string.</param>
public record LocaleString
(
    uint ID,
    [property: JsonPropertyName("en")] string? English,
    [property: JsonPropertyName("de")] string? German,
    [property: JsonPropertyName("fr")] string? French,
    [property: JsonPropertyName("it")] string? Italian,
    [property: JsonPropertyName("es")] string? Spanish,
    [property: JsonPropertyName("tr")] string? Turkish
)
{
    /// <summary>
    /// Gets the default <see cref="LocaleString"/>.
    /// </summary>
    public static LocaleString Default => new
    (
        0,
        null,
        null,
        null,
        null,
        null,
        null
    );
}
