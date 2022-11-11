using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sanctuary.Census.Common.Objects;
using System;
using System.Collections.Generic;
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
    private readonly ILogger<CachingManifestService> _logger;
    private readonly SemaphoreSlim _streamCacheLock;
    private readonly Dictionary<PS2Environment, MemoryStream> _manifestStreams;
    private readonly Dictionary<PS2Environment, DateTimeOffset> _streamsLastUpdated;

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
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="clientFactory">The HTTP client to use.</param>
    /// <param name="commonOptions">The configured common options.</param>
    /// <param name="fileSystem">The file system implementation to use.</param>
    public CachingManifestService
    (
        ILogger<CachingManifestService> logger,
        IHttpClientFactory clientFactory,
        IOptions<CommonOptions> commonOptions,
        IFileSystem fileSystem
    ) : base(clientFactory)
    {
        _logger = logger;
        _streamCacheLock = new SemaphoreSlim(1);
        _manifestStreams = new Dictionary<PS2Environment, MemoryStream>();
        _streamsLastUpdated = new Dictionary<PS2Environment, DateTimeOffset>();

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
        {
            _logger.LogDebug
            (
                "[{Env}] Manifest file retrieve from cache. Last write time: {Lwt}. Manifest timestamp: {Ts}",
                file.Environment,
                fileInfo.LastWriteTimeUtc,
                file.Timestamp
            );
            return FileSystem.File.OpenRead(filePath);
        }

        Stream dataStream = await base.GetFileDataAsync(file, ct).ConfigureAwait(false);
        await using Stream fs = FileSystem.File.OpenWrite(filePath);

        await dataStream.CopyToAsync(fs, ct).ConfigureAwait(false);
        dataStream.Seek(0, SeekOrigin.Begin);

        return dataStream;
    }

    /// <inheritdoc />
    protected override async Task<Stream> GetManifestStreamAsync(PS2Environment environment, CancellationToken ct)
    {
        await _streamCacheLock.WaitAsync(ct);

        bool streamNeedsUpdating = !_streamsLastUpdated.TryGetValue(environment, out DateTimeOffset lastUpdate)
            || lastUpdate.AddMinutes(5) < DateTimeOffset.UtcNow;

        bool streamNotPresent = !_manifestStreams.TryGetValue(environment, out MemoryStream? cacheStream);

        if (streamNeedsUpdating || streamNotPresent)
        {
            if (cacheStream is not null)
                await cacheStream.DisposeAsync();

            cacheStream = new MemoryStream();
            _manifestStreams[environment] = cacheStream;

            await using Stream s = await base.GetManifestStreamAsync(environment, ct);
            await s.CopyToAsync(cacheStream, ct);
            _streamsLastUpdated[environment] = DateTimeOffset.UtcNow;
        }

        cacheStream!.Seek(0, SeekOrigin.Begin);

        MemoryStream ms = new();
        await cacheStream.CopyToAsync(ms, ct);
        ms.Seek(0, SeekOrigin.Begin);

        _streamCacheLock.Release();
        return ms;
    }
}
