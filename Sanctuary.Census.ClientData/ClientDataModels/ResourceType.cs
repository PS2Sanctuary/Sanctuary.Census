namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents a client resource type.
/// </summary>
/// <param name="TypeID">The ID of the resource type.</param>
/// <param name="TypeName">The name of the resource type.</param>
/// <param name="ImageSet">The ID of the resource type's image set.</param>
/// <param name="NameID">The locale ID of the resource type.</param>
public record ResourceType
(
    uint TypeID,
    string? TypeName,
    uint ImageSet,
    uint NameID
);
