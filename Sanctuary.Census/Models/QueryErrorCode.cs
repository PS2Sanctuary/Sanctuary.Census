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
    InvalidFilterTerm = 3,

    /// <summary>
    /// An invalid namespace was provided.
    /// </summary>
    InvalidNamespace = 4,

    /// <summary>
    /// An internal server error occurred.
    /// </summary>
    ServerError = 5,

    /// <summary>
    /// A required key of a command is missing.
    /// </summary>
    MissingRequiredKey = 6,

    /// <summary>
    /// An invalid value was provided to a command.
    /// </summary>
    InvalidCommandValue = 7,

    /// <summary>
    /// A field needed to be specified for either the 'on' or 'to' join key.
    /// </summary>
    JoinFieldRequired = 8,

    /// <summary>
    /// The collection to join to was not specified in a join command.
    /// </summary>
    JoinCollectionRequired = 9
}
