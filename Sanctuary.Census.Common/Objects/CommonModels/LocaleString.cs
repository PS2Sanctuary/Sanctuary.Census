using System.Text.Json.Serialization;

namespace Sanctuary.Census.Common.Objects.CommonModels;

/// <summary>
/// Represents a globalized string model.
/// </summary>
/// <param name="ID">The ID of the locale string.</param>
/// <param name="German">The German translation of the string.</param>
/// <param name="English">The English translation of the string.</param>
/// <param name="Spanish">The Spanish translation of the string.</param>
/// <param name="French">The French translation of the string.</param>
/// <param name="Italian">The Italian translation of the string.</param>
/// <param name="Korean">The Korean translation of the string.</param>
/// <param name="Portuguese">The Portuguese translation of the string.</param>
/// <param name="Russian">The Russian translation of the string.</param>
/// <param name="Turkish">The Turkish translation of the string.</param>
/// <param name="Chinese">The Chinese translation of the string.</param>
public record LocaleString
(
    uint ID,
    [property: JsonPropertyName("de")] string? German,
    [property: JsonPropertyName("en")] string? English,
    [property: JsonPropertyName("es")] string? Spanish,
    [property: JsonPropertyName("fr")] string? French,
    [property: JsonPropertyName("it")] string? Italian,
    [property: JsonPropertyName("ko")] string? Korean,
    [property: JsonPropertyName("pt")] string? Portuguese,
    [property: JsonPropertyName("ru")] string? Russian,
    [property: JsonPropertyName("tr")] string? Turkish,
    [property: JsonPropertyName("zh")] string? Chinese
);
