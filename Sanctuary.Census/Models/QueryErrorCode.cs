namespace Sanctuary.Census.Models;

/// <summary>
/// Defines the possible query error codes.
/// </summary>
public enum QueryErrorCode
{
    /// <summary>
    /// The query was malformed.
    /// </summary>
    Malformed = 0,

    /// <summary>
    /// An unknown collection has been requested.
    /// </summary>
    UnknownCollection = 1,

    /// <summary>
    /// A query was made on an unknown field.
    /// </summary>
    UnknownField = 2,

    /// <summary>
    /// An invalid filter term was provided.
    /// </summary>
    InvalidFilterTerm = 3
}
