using Sanctuary.Census.Common.Objects;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Common.Abstractions.Services;

/// <summary>
/// Represents a service for retrieving PlanetSide 2 patch manifest data.
/// </summary>
public interface IManifestService
{
    /// <summary>
    /// Retrieves information about a manifest file.
    /// </summary>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="ps2Environment">The environment to retrieve manifest data from.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The retrieved manifest file.</returns>
    public Task<ManifestFile> GetFileAsync(string fileName, PS2Environment ps2Environment, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the data of a manifest file.
    /// </summary>
    /// <param name="file">The file to retrieve.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A stream containing the file data.</returns>
    Task<Stream> GetFileDataAsync(ManifestFile file, CancellationToken ct = default);
}
