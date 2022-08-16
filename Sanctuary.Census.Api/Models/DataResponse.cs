using System.Collections.Generic;

namespace Sanctuary.Census.Api.Models;

/// <summary>
/// Represents a data response.
/// </summary>
/// <typeparam name="TDataType">The type of the data.</typeparam>
/// <param name="Data">The data.</param>
/// <param name="Returned">The number of elements returned in the response.</param>
/// <param name="DataTypeName">The name of the data type.</param>
/// <param name="Timing">The time taken to generate the response.</param>
public record DataResponse<TDataType>
(
    IEnumerable<TDataType> Data,
    int Returned,
    string DataTypeName,
    QueryTimes? Timing
)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataResponse{TDataType}"/> record.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="dataTypeName">The name of the data type.</param>
    /// <param name="timing">The time taken to generate the response.</param>
    public DataResponse(IReadOnlyCollection<TDataType> data, string dataTypeName, QueryTimes? timing)
        : this(data, data.Count, dataTypeName, timing)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataResponse{TDataType}"/> record.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="dataTypeName">The name of the data type.</param>
    /// <param name="timing">The time taken to generate the response.</param>
    public DataResponse(TDataType data, string dataTypeName, QueryTimes? timing)
        : this(new[] { data }, 1, dataTypeName, timing)
    {
    }
}
