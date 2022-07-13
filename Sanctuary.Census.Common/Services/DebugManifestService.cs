using Sanctuary.Census.Common.Objects;
using System.IO;
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
    /// <param name="httpClient">The HTTP client to use.</param>
    /// <param name="appDataDirectory">The path to the app data directory to use.</param>
    public DebugManifestService(HttpClient httpClient, string appDataDirectory)
        : base(httpClient, appDataDirectory)
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
