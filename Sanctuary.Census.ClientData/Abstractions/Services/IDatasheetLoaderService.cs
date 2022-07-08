using CommunityToolkit.HighPerformance.Buffers;
using Sanctuary.Census.ClientData.Objects;
using Sanctuary.Census.Common.Objects;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.ClientData.Abstractions.Services;

/// <summary>
/// Represents an interface for loading datasheet files.
/// </summary>
public interface IDatasheetLoaderService
{
    /// <summary>
    /// Loads every record from a datasheet.
    /// </summary>
    /// <typeparam name="TDataType">The model type of the datasheet.</typeparam>
    /// <param name="datasheet">The datasheet to load.</param>
    /// <param name="environment">The environment to load the datasheet from.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>
    /// An enumerable of the <typeparamref name="TDataType"/> objects representing the records in the datasheet.
    /// </returns>
    Task<IEnumerable<TDataType>> LoadDatasheetDataAsync<TDataType>
    (
        PackedFileInfo datasheet,
        PS2Environment environment,
        CancellationToken ct = default
    );

    /// <summary>
    /// Gets a datasheet's byte representation.
    /// </summary>
    /// <param name="datasheet">The datasheet to load.</param>
    /// <param name="environment">The environment to load the datasheet from.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The raw data.</returns>
    Task<MemoryOwner<byte>> GetRawDatasheetAsync
    (
        PackedFileInfo datasheet,
        PS2Environment environment,
        CancellationToken ct = default
    );
}
