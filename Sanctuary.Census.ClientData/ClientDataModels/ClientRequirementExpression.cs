using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents a client requirement expression.
/// </summary>
/// <param name="ID">The ID of the expression.</param>
/// <param name="Expression">The expression.</param>
[Datasheet]
public partial record ClientRequirementExpression
(
    uint ID,
    string Expression
);
