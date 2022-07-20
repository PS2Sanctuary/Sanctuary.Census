using Sanctuary.Census.Models;
using System;

namespace Sanctuary.Census.Exceptions;

/// <summary>
/// Represents an exception that occurs as part of a query.
/// </summary>
public class QueryException : Exception
{
    /// <summary>
    /// Gets the error code.
    /// </summary>
    public QueryErrorCode ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryException"/> class.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="message">The error message.</param>
    public QueryException(QueryErrorCode errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
