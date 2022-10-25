namespace Sanctuary.Census.Api.Models;

/// <summary>
/// Represents information about a collection's fields.
/// </summary>
/// <param name="Name">The name of the field.</param>
/// <param name="Type">The type of the field.</param>
/// <param name="IsNullable">Indicates whether the field is nullable.</param>
/// <param name="Description">A description of the field.</param>
public record CollectionFieldInformation
(
    string Name,
    string Type,
    bool IsNullable,
    string? Description
);
