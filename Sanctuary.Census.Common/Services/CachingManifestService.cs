using Microsoft.Extensions.Options;
using Sanctuary.Census.Common.Objects;
using System.IO;
using System.IO.Abstractions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Common.Services;

/// <inheritdoc />
/// <remarks>This service will cache manifest files in local appdata.</remarks>
public class CachingManifestService : ManifestService
{
    /// <summary>
    /// Gets the file system implementation.
    /// </summary>
    protected IFileSystem FileSystem { get; }

    /// <summary>
    /// Gets the directory under which files are cached.
    /// </summary>
    public string CacheDirectory { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingManifestService"/> class.
    /// </summary>
    /// <param name="clientFactory">The HTTP client to use.</param>
    /// <param name="commonOptions">The configured common options.</param>
    /// <param name="fileSystem">The file system implementation to use.</param>
    public CachingManifestService
    (
        IHttpClientFactory clientFactory,
        IOptions<CommonOptions> commonOptions,
        IFileSystem fileSystem
    ) : base(clientFactory)
    {
        FileSystem = fileSystem;
        CacheDirectory = FileSystem.Path.Combine(commonOptions.Value.AppDataDirectory, "ManifestCache");
    }

    /// <inheritdoc />
    public override async Task<Stream> GetFileDataAsync(ManifestFile file, CancellationToken ct = default)
    {
        string cacheDirectory = Path.Combine(CacheDirectory, file.Environment.ToString());
        if (!FileSystem.Directory.Exists(cacheDirectory))
            FileSystem.Directory.CreateDirectory(cacheDirectory);

        string filePath = FileSystem.Path.Combine(cacheDirectory, file.FileName);
        IFileInfo fileInfo = FileSystem.FileInfo.FromFileName(filePath);

        if (fileInfo.Exists && fileInfo.LastWriteTimeUtc >= file.Timestamp)
            return FileSystem.File.OpenRead(filePath);

        Stream dataStream = await base.GetFileDataAsync(file, ct).ConfigureAwait(false);
        await using Stream fs = FileSystem.File.OpenWrite(filePath);

        await dataStream.CopyToAsync(fs, ct).ConfigureAwait(false);
        dataStream.Seek(0, SeekOrigin.Begin);

        return dataStream;
    }
}
