using System.Text.Json.Serialization;

namespace Sanctuary.Census.Models;

/// <summary>
/// Represents a globalized string model.
/// </summary>
/// <param name="English">The English translation of the string.</param>
/// <param name="German">The German translation of the string.</param>
/// <param name="French">The French translation of the string.</param>
/// <param name="Italian">The Italian translation of the string.</param>
/// <param name="Spanish">The Spanish translation of the string.</param>
/// <param name="Turkish">The Turkish translation of the string.</param>
public record GlobalizedString
(
    [property: JsonPropertyName("en")]
    string? English,
    [property: JsonPropertyName("de")]
    string? German,
    [property: JsonPropertyName("fr")]
    string? French,
    [property: JsonPropertyName("it")]
    string? Italian,
    [property: JsonPropertyName("es")]
    string? Spanish,
    [property: JsonPropertyName("tr")]
    string? Turkish
);
