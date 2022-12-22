using Mandible.Manifest;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sanctuary.Census.Common.Objects;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Common.Services;

/// <inheritdoc />
/// <remarks>
/// This service uses the <see cref="CachingManifestService"/> but returns only local file information,
/// ensuring the the patch manifest servers are only hit when a file is not cached.
/// </remarks>
public class DebugManifestService : CachingManifestService
{
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugManifestService"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="manifestService">The underlying manifest service to use.</param>
    /// <param name="memoryCache">The memory cache to use.</param>
    /// <param name="commonOptions">The configured common options.</param>
    /// <param name="fileSystem">The filesystem to use.</param>
    public DebugManifestService
    (
        ILogger<DebugManifestService> logger,
        Mandible.Abstractions.Manifest.IManifestService manifestService,
        IMemoryCache memoryCache,
        IOptions<CommonOptions> commonOptions,
        IFileSystem fileSystem
    ) : base(logger, manifestService, memoryCache, commonOptions, fileSystem)
    {
        _memoryCache = memoryCache;
    }

    /// <inheritdoc />
    public override async Task<ManifestFileWrapper> GetFileAsync
    (
        string fileName,
        PS2Environment ps2Environment,
        CancellationToken ct = default
    )
    {
        bool hasCachedDigest = _memoryCache.TryGetValue
        (
            GetDigestCacheKey(ps2Environment),
            out _
        );

        if (hasCachedDigest)
            return await base.GetFileAsync(fileName, ps2Environment, ct).ConfigureAwait(false);

        string filePath = Path.Combine(CacheDirectory, ps2Environment.ToString(), "cached.debug.digest");
        FileInfo info = new(filePath);

        if (!info.Exists || info.LastWriteTime.AddHours(24) < DateTime.Now)
        {
            Digest digest = await CacheDigestAsync(ps2Environment, ct).ConfigureAwait(false);
            await using FileStream fs = new(filePath, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(fs, digest, cancellationToken: ct).ConfigureAwait(false);
        }
        else
        {
            await using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
            Digest? digest = await JsonSerializer.DeserializeAsync<Digest>(fs, cancellationToken: ct).ConfigureAwait(false);
            if (digest is not null)
                _memoryCache.Set(GetDigestCacheKey(ps2Environment), digest, DigestCacheTime);
        }

        return await base.GetFileAsync(fileName, ps2Environment, ct).ConfigureAwait(false);
    }
}
