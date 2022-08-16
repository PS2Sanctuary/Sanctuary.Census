using System.Text.Json.Serialization;

namespace Sanctuary.Census.Api.Models;

/// <summary>
/// Represents an error response.
/// </summary>
/// <param name="ErrorCode">The error code.</param>
/// <param name="ErrorMessage">The error message.</param>
public record ErrorResponse
(
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    QueryErrorCode ErrorCode,
    string ErrorMessage
);
