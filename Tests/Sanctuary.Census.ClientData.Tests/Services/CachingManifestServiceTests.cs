using Mandible.Abstractions.Manifest;
using Mandible.Manifest;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sanctuary.Census.ClientData.Tests.Services;

// TODO: Tests are very broken!
public class CachingManifestServiceTests
{
    [Fact]
    public async Task TestGetNonExistentFileAsync()
    {
        CachingManifestService ms = GetManifestService(out _, new MockFileSystem());

        await Assert.ThrowsAsync<KeyNotFoundException>
        (
            async () => await ms.GetFileAsync("this_file_doesnt_exist", PS2Environment.PS2, CancellationToken.None)
        );
    }

    public async Task TestGetFileAsync()
    {
        CachingManifestService ms = GetManifestService(out Mock<IManifestService> msMock, new MockFileSystem());

        await Assert.ThrowsAsync<KeyNotFoundException>
        (
            async () => await ms.GetFileAsync("does_not_exist.txt", PS2Environment.PS2)
        );

        msMock.Verify
        (
            x => x.GetDigestAsync
            (
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once()
        );
    }

    // [Fact]
    // public async Task TestGetFileDataAsync()
    // {
    //     ManifestFile file = new("manifest.xml", 2, 1, 0, DateTimeOffset.Now, "abcdefgh", PS2Environment.PS2);
    //     MockFileSystem fileSystem = new();
    //     CachingManifestService ms = GetManifestService(out Mock<ManifestService> msMock, fileSystem);
    //
    //     Assert.False(fileSystem.Directory.Exists(ms.CacheDirectory));
    //     await ms.GetFileDataAsync(file, CancellationToken.None);
    //
    //     // Assert that the file is retrieved from the manifest source
    //     // and cached
    //     msMock.Protected()
    //         .Verify
    //         (
    //             "SendAsync",
    //             Times.Once(),
    //             ItExpr.IsAny<HttpRequestMessage>(),
    //             ItExpr.IsAny<CancellationToken>()
    //         );
    //     Assert.True(fileSystem.Directory.Exists(ms.CacheDirectory));
    //     Assert.True(fileSystem.File.Exists(fileSystem.Path.Combine(ms.CacheDirectory, file.Environment.ToString(), "manifest.xml")));
    //
    //     // Assert that the file is not retrieved from the manifest source again
    //     ms = GetManifestService(out msMock, fileSystem);
    //     await ms.GetFileDataAsync(file, CancellationToken.None);
    //     msMock.Protected()
    //         .Verify
    //         (
    //             "SendAsync",
    //             Times.Never(),
    //             ItExpr.IsAny<HttpRequestMessage>(),
    //             ItExpr.IsAny<CancellationToken>()
    //         );
    // }

    private static CachingManifestService GetManifestService(out Mock<IManifestService> manifestServiceMock, IFileSystem fileSystem)
    {
        Digest digest = new
        (
            1,
            "test",
            new Uri("https://my.url"),
            string.Empty,
            null,
            12,
            2,
            null,
            "local",
            new Uri("http://pls.patch.daybreakgames.com/patch/sha/planetside2/planetside2.sha.zs"),
            DateTimeOffset.Now,
            "lzma",
            new[] { "pls.patch.daybreakgames.com", "antonica.patch.daybreakgames.com" },
            Array.Empty<Uri>(),
            Array.Empty<Folder>()
        );
        ManifestFile file = new("test.txt", 2, 1, 0, DateTimeOffset.Now, null, "abcdef", null, null, Array.Empty<ManifestFilePatch>());

        manifestServiceMock = new Mock<IManifestService>();

        manifestServiceMock.Setup<Task<Digest>>
            (
                x => x.GetDigestAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(digest)
            .Verifiable();

        manifestServiceMock.Setup<Task<Stream>>
            (
                x => x.GetFileDataAsync
                (
                    It.IsAny<Digest>(),
                    It.IsAny<ManifestFile>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new MemoryStream())
            .Verifiable();

        return new CachingManifestService
        (
            NullLogger<CachingManifestService>.Instance,
            manifestServiceMock.Object,
            Mock.Of<IMemoryCache>(),
            Options.Create(new CommonOptions { AppDataDirectory = "AppData" }),
            fileSystem
        );
    }
}
