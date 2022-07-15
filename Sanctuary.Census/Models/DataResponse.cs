using System.Collections.Generic;

namespace Sanctuary.Census.Models;

/// <summary>
/// Represents a data response.
/// </summary>
/// <typeparam name="TDataType">The type of the data.</typeparam>
/// <param name="Data">The data.</param>
/// <param name="Returned">The number of elements returned in the response.</param>
/// <param name="DataTypeName">The name of the data type.</param>
public record DataResponse<TDataType>
(
    IEnumerable<TDataType> Data,
    int Returned,
    string DataTypeName
)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataResponse{TDataType}"/> record.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="dataTypeName">The name of the data type.</param>
    public DataResponse(IReadOnlyCollection<TDataType> data, string dataTypeName)
        : this(data, data.Count, dataTypeName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataResponse{TDataType}"/> record.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="dataTypeName">The name of the data type.</param>
    public DataResponse(TDataType data, string dataTypeName)
        : this(new[] { data }, 1, dataTypeName)
    {
    }
}
