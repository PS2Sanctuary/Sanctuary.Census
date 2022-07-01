using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.Objects;
using Sanctuary.Census.Common.Objects;

namespace Sanctuary.Census.ClientData.Services;

/// <inheritdoc />
/// <remarks>This service retrieves manifest data from a local directory.</remarks>
public class LocalManifestService : IManifestService
{
    private readonly DebugOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalManifestService"/> class.
    /// </summary>
    /// <param name="options">The debug options.</param>
    public LocalManifestService(IOptions<DebugOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public Task<ManifestFile> GetFileAsync(string fileName, PS2Environment ps2Environment, CancellationToken ct = default)
    {
        string filePath = Path.Combine(_options.LocalManifestFilePath, fileName);
        FileInfo info = new(filePath);
        if (!info.Exists)
            throw new KeyNotFoundException();

        return Task.FromResult(new ManifestFile
        (
            fileName,
            (int) info.Length,
            (int) info.Length,
            0,
            info.LastWriteTimeUtc,
            string.Empty
        ));
    }

    /// <inheritdoc />
    public Task<Stream> GetFileDataAsync(ManifestFile file, CancellationToken ct = default)
        => Task.FromResult<Stream>(new FileStream
        (
            Path.Combine(_options.LocalManifestFilePath, file.FileName),
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read
        ));
}
