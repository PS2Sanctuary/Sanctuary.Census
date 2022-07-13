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
    /// <param name="httpClient">The HTTP client to use.</param>
    /// <param name="appDataDirectory">The path to the app data directory to use.</param>
    public CachingManifestService(HttpClient httpClient, string appDataDirectory)
        : this(httpClient, appDataDirectory, new FileSystem())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingManifestService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use.</param>
    /// <param name="appDataDirectory">The path to the app data directory to use.</param>
    /// <param name="fileSystem">The file system implementation to use.</param>
    public CachingManifestService(HttpClient httpClient, string appDataDirectory, IFileSystem fileSystem)
        : base(httpClient)
    {
        FileSystem = fileSystem;
        CacheDirectory = FileSystem.Path.Combine(appDataDirectory, "ManifestCache");
    }

    /// <inheritdoc />
    public override async Task<Stream> GetFileDataAsync(ManifestFile file, CancellationToken ct = default)
    {
        if (!FileSystem.Directory.Exists(CacheDirectory))
            FileSystem.Directory.CreateDirectory(CacheDirectory);

        string filePath = FileSystem.Path.Combine(CacheDirectory, file.FileName);
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
