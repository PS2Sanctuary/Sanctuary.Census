using System.Diagnostics.CodeAnalysis;

namespace Sanctuary.Census.Builder.Abstractions.Services;

/// <summary>
/// Contains helpers for retrieving requirements (and expressions) data.
/// </summary>
public interface IRequirementsHelperService
{
    /// <summary>
    /// Attempts to find a client requirement expression.
    /// </summary>
    /// <param name="id">The ID of the expression.</param>
    /// <param name="expression">
    /// The expression, or <c>null</c> if none were found for the given <paramref name="id"/>.
    /// </param>
    /// <returns>
    /// <c>True</c> if an expression with the given <paramref name="id"/> exists, else <c>false</c>.
    /// </returns>
    bool TryGetClientExpression(uint id, [NotNullWhen(true)] out string? expression);

    /*
     * Yet to implement:
     * TryGetServerExpression
     * TryGetClientRequirement
     * TryGetServerRequirement
     */
}
