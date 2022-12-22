using Mandible.Manifest;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Common.Services;

/// <inheritdoc />
/// <remarks>This service will cache manifest files in local appdata.</remarks>
public class CachingManifestService : IManifestService
{
    private static readonly Dictionary<PS2Environment, string> ManifestUrls = new()
    {
        { PS2Environment.PS2, "http://manifest.patch.daybreakgames.com/patch/sha/manifest/planetside2/planetside2-livecommon/livenext/planetside2-livecommon.sha.soe.txt" },
        { PS2Environment.PTS, "http://manifest.patch.daybreakgames.com/patch/sha/manifest/planetside2/planetside2-testcommon/livenext/planetside2-testcommon.sha.soe.txt" }
    };

    /// <summary>
    /// Gets the length of time for which a digest should be cached for.
    /// </summary>
    protected static readonly TimeSpan DigestCacheTime = TimeSpan.FromMinutes(10);

    private readonly ILogger<CachingManifestService> _logger;
    private readonly Mandible.Abstractions.Manifest.IManifestService _manifestService;
    private readonly IMemoryCache _memoryCache;

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
    /// <param name="manifestService">The underlying manifest service to use.</param>
    /// <param name="memoryCache">The memory cache to use.</param>
    /// <param name="commonOptions">The configured common options.</param>
    /// <param name="fileSystem">The file system implementation to use.</param>
    public CachingManifestService
    (
        ILogger<CachingManifestService> logger,
        Mandible.Abstractions.Manifest.IManifestService manifestService,
        IMemoryCache memoryCache,
        IOptions<CommonOptions> commonOptions,
        IFileSystem fileSystem
    )
    {
        _logger = logger;
        _manifestService = manifestService;
        _memoryCache = memoryCache;

        FileSystem = fileSystem;
        CacheDirectory = FileSystem.Path.Combine(commonOptions.Value.AppDataDirectory, "ManifestCache");
    }

    /// <inheritdoc />
    public virtual async Task<ManifestFileWrapper> GetFileAsync
    (
        string fileName,
        PS2Environment ps2Environment,
        CancellationToken ct = default
    )
    {
        bool hasCachedDigest = _memoryCache.TryGetValue
        (
            GetDigestCacheKey(ps2Environment),
            out Digest? digest
        );

        if (digest is null || !hasCachedDigest)
            digest = await CacheDigestAsync(ps2Environment, ct).ConfigureAwait(false);

        ManifestFile? file = null;
        foreach (Folder folder in digest.Folders)
        {
            file = FindManifestFile(fileName, folder);
            if (file is not null)
                break;
        }

        return file is null
            ? throw new KeyNotFoundException($"Failed to find the file {fileName}")
            : new ManifestFileWrapper(file, digest);
    }

    /// <inheritdoc />
    public virtual async Task<Stream> GetFileDataAsync
    (
        ManifestFileWrapper file,
        PS2Environment ps2Environment,
        CancellationToken ct = default
    )
    {
        string cacheDirectory = Path.Combine(CacheDirectory, ps2Environment.ToString());
        if (!FileSystem.Directory.Exists(cacheDirectory))
            FileSystem.Directory.CreateDirectory(cacheDirectory);

        string filePath = FileSystem.Path.Combine(cacheDirectory, file.File.Name);
        IFileInfo fileInfo = FileSystem.FileInfo.New(filePath);

        if (fileInfo.Exists && fileInfo.LastWriteTimeUtc >= file.File.Timestamp)
        {
            _logger.LogDebug
            (
                "[{Env}] Manifest file retrieved from cache. Last write time: {Lwt}. Manifest timestamp: {Ts}",
                ps2Environment,
                fileInfo.LastWriteTimeUtc,
                file.File.Timestamp
            );
            return FileSystem.File.OpenRead(filePath);
        }

        Stream dataStream = await _manifestService.GetFileDataAsync(file.Owner, file.File, ct).ConfigureAwait(false);
        await using Stream fs = FileSystem.File.OpenWrite(filePath);

        await dataStream.CopyToAsync(fs, ct).ConfigureAwait(false);
        dataStream.Seek(0, SeekOrigin.Begin);

        return dataStream;
    }

    /// <summary>
    /// Caches the latest digest.
    /// </summary>
    /// <param name="environment">The environment to cache the digest of.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The retrieved digest.</returns>
    protected async Task<Digest> CacheDigestAsync(PS2Environment environment, CancellationToken ct)
    {
        Digest digest = await _manifestService.GetDigestAsync(ManifestUrls[environment], ct).ConfigureAwait(false);
        _memoryCache.Set(GetDigestCacheKey(environment), digest, DigestCacheTime);
        return digest;
    }

    /// <summary>
    /// Recursively attempts to find a manifest file within a folder.
    /// </summary>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="searchRoot">The folder begin searching from.</param>
    /// <returns>The file, or <c>null</c> if it does not exist within the <paramref name="searchRoot"/>.</returns>
    protected static ManifestFile? FindManifestFile(string fileName, Folder searchRoot)
    {
        foreach (ManifestFile file in searchRoot.Files)
        {
            if (file.Name == fileName)
                return file;
        }

        foreach (Folder child in searchRoot.Children)
        {
            ManifestFile? childFile = FindManifestFile(fileName, child);
            if (childFile is not null)
                return childFile;
        }

        return null;
    }

    /// <summary>
    /// Gets the key used to store digest objects in the memory cache.
    /// </summary>
    /// <param name="environment">The environment that the digest was retrieved from.</param>
    /// <returns>The cache key.</returns>
    protected static object GetDigestCacheKey(PS2Environment environment)
        => (typeof(Digest), environment);
}
