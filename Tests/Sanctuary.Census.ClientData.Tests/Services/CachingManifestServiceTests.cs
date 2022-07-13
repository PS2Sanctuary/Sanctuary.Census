using Moq;
using Moq.Protected;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Services;
using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sanctuary.Census.ClientData.Tests.Services;

public class CachingManifestServiceTests
{
    [Fact]
    public async Task TestGetFileDataAsync()
    {
        ManifestFile file = new("manifest.xml", 2, 1, 0, DateTimeOffset.Now, "abcdefgh", PS2Environment.Live);
        MockFileSystem fileSystem = new();
        CachingManifestService ms = GetManifestService(out Mock<HttpMessageHandler> handler, fileSystem);

        Assert.False(fileSystem.Directory.Exists(ms.CacheDirectory));
        await ms.GetFileDataAsync(file, CancellationToken.None);

        // Assert that the file is retrieved from the manifest source
        // and cached
        handler.Protected()
            .Verify
            (
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        Assert.True(fileSystem.Directory.Exists(ms.CacheDirectory));
        Assert.True(fileSystem.File.Exists(fileSystem.Path.Combine(ms.CacheDirectory, file.Environment.ToString(), "manifest.xml")));

        // Assert that the file is not retrieved from the manifest source again
        ms = GetManifestService(out handler, fileSystem);
        await ms.GetFileDataAsync(file, CancellationToken.None);
        handler.Protected()
            .Verify
            (
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    private static CachingManifestService GetManifestService(out Mock<HttpMessageHandler> handlerMock, IFileSystem fileSystem)
    {
        FileStream manifestFS = new("Data\\ManifestService\\manifest.xml", FileMode.Open, FileAccess.Read, FileShare.Read);

        handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>
            (
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync
            (
                new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StreamContent(manifestFS)
                }
            )
            .Verifiable();

        return new CachingManifestService
        (
            new HttpClient(handlerMock.Object),
            "AppData",
            fileSystem
        );
    }
}
