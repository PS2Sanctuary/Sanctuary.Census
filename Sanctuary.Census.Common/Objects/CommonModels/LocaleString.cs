namespace Sanctuary.Census.Common.Objects.CommonModels;

/// <summary>
/// Represents a globalized string model.
/// </summary>
/// <param name="De">The German translation of the string.</param>
/// <param name="En">The English translation of the string.</param>
/// <param name="Es">The Spanish translation of the string.</param>
/// <param name="Fr">The French translation of the string.</param>
/// <param name="It">The Italian translation of the string.</param>
/// <param name="Ko">The Korean translation of the string.</param>
/// <param name="Pt">The Portuguese translation of the string.</param>
/// <param name="Ru">The Russian translation of the string.</param>
/// <param name="Tr">The Turkish translation of the string.</param>
/// <param name="Zh">The Chinese translation of the string.</param>
public record LocaleString
(
    string? De,
    string? En,
    string? Es,
    string? Fr,
    string? It,
    string? Ko,
    string? Pt,
    string? Ru,
    string? Tr,
    string? Zh
);
