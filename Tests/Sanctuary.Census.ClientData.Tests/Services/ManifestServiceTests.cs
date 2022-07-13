using Moq;
using Moq.Protected;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sanctuary.Census.ClientData.Tests.Services;

public class ManifestServiceTests
{
    [Fact]
    public async Task TestGetFileAsync()
    {
        ManifestService ms = GetManifestService(out Mock<HttpMessageHandler> handler);
        ManifestFile file = await ms.GetFileAsync("data_x64_0.pack2", PS2Environment.Live, CancellationToken.None);

        Uri expectedUri = new("http://manifest.patch.daybreakgames.com/patch/sha/manifest/planetside2/planetside2-livecommon/livenext/planetside2-livecommon.sha.soe.txt");
        handler.Protected()
            .Verify
            (
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>
                (
                    req => req.Method == HttpMethod.Get
                        && req.RequestUri == expectedUri
                ),
                ItExpr.IsAny<CancellationToken>()
            );

        Assert.Equal(3815748171, file.Crc32);
        Assert.Equal(8679841, file.CompressedSize);
        Assert.Equal(8679712, file.UncompressedSize);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1656703269), file.Timestamp);
        Assert.Equal("data_x64_0.pack2", file.FileName);
        Assert.Equal("ae8f96542558a1911579e9bdcef9d85d90c6477b", file.SHA);
    }

    [Fact]
    public async Task TestGetNonExistentFileAsync()
    {
        ManifestService ms = GetManifestService(out _);

        await Assert.ThrowsAsync<KeyNotFoundException>
        (
            async () => await ms.GetFileAsync("this_file_doesnt_exist", PS2Environment.Live, CancellationToken.None)
        );
    }

    [Fact]
    public async Task TestGetFileDataAsync()
    {
        ManifestFile file = new("manifest.xml", 2, 1, 0, DateTimeOffset.Now, "abcdefgh", PS2Environment.Live);
        ManifestService ms = GetManifestService(out Mock<HttpMessageHandler> handler);
        Stream fileData = await ms.GetFileDataAsync(file, CancellationToken.None);

        Uri expectedUri = new("http://pls.patch.daybreakgames.com/patch/sha/planetside2/planetside2.sha.zs/ab/cde/fgh");
        handler.Protected()
            .Verify
            (
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>
                (
                    req => req.Method == HttpMethod.Get
                           && req.RequestUri == expectedUri
                ),
                ItExpr.IsAny<CancellationToken>()
            );

        Assert.True(fileData.CanSeek);
        Assert.Equal(0, fileData.Position);
    }

    private static ManifestService GetManifestService(out Mock<HttpMessageHandler> handlerMock)
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
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StreamContent(manifestFS)
                }
            )
            .Verifiable();

        return new ManifestService(new HttpClient(handlerMock.Object));
    }
}
