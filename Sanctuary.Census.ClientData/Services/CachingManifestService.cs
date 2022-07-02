using Sanctuary.Census.ClientData.Objects;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.ClientData.Services;

/// <inheritdoc />
/// <remarks>This service will cache manifest files in local appdata.</remarks>
public class CachingManifestService : ManifestService
{
    private readonly string _cachePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingManifestService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use.</param>
    /// <param name="appDataPath">The path to the app data directory to use.</param>
    public CachingManifestService(HttpClient httpClient, string appDataPath)
        : base(httpClient)
    {
        _cachePath = Path.Combine(appDataPath, "ManifestCache");
    }

    /// <inheritdoc />
    public override async Task<Stream> GetFileDataAsync(ManifestFile file, CancellationToken ct = default)
    {
        if (!Directory.Exists(_cachePath))
            Directory.CreateDirectory(_cachePath);

        string filePath = Path.Combine(_cachePath, file.FileName);
        FileInfo fileInfo = new(filePath);

        if (fileInfo.Exists && fileInfo.CreationTimeUtc > file.Timestamp)
            return File.OpenRead(filePath);

        Stream dataStream = await base.GetFileDataAsync(file, ct).ConfigureAwait(false);
        await using FileStream fs = File.OpenWrite(filePath);

        await dataStream.CopyToAsync(fs, ct).ConfigureAwait(false);
        dataStream.Seek(0, SeekOrigin.Begin);

        return dataStream;
    }
}
