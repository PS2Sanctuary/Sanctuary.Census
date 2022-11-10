using Microsoft.Extensions.Options;
using Sanctuary.Census.Common.Objects;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Net.Http;
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
    /// <summary>
    /// Initializes a new instance of the <see cref="DebugManifestService"/> class.
    /// </summary>
    /// <param name="clientFactory">The HTTP client to use.</param>
    /// <param name="commonOptions">The configured common options.</param>
    /// <param name="fileSystem">The filesystem to use.</param>
    public DebugManifestService
    (
        IHttpClientFactory clientFactory,
        IOptions<CommonOptions> commonOptions,
        IFileSystem fileSystem
    ) : base(clientFactory, commonOptions, fileSystem)
    {
    }

    /// <inheritdoc />
    public override async Task<ManifestFile> GetFileAsync
    (
        string fileName,
        PS2Environment ps2Environment,
        CancellationToken ct = default
    )
    {
        string filePath = Path.Combine(CacheDirectory, ps2Environment.ToString(), fileName);
        FileInfo info = new(filePath);

        if (!info.Exists)
            return await base.GetFileAsync(fileName, ps2Environment, ct);

        if (info.LastWriteTime.AddHours(24) < DateTime.Now)
        {
            info.Delete();
            return await base.GetFileAsync(fileName, ps2Environment, ct);;
        }

        return new ManifestFile
        (
            fileName,
            (int)info.Length,
            (int)info.Length,
            0,
            info.LastWriteTimeUtc,
            string.Empty,
            ps2Environment
        );
    }
}
