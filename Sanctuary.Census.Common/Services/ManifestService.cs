using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using SevenZip.Compression.LZMA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Sanctuary.Census.Common.Services;

/// <inheritdoc />
public class ManifestService : IManifestService
{
    private const string ROOT_DOWNLOAD_URL = "http://pls.patch.daybreakgames.com/patch/sha/planetside2/planetside2.sha.zs/";

    private static readonly Dictionary<PS2Environment, string> ManifestUrls = new()
    {
        { PS2Environment.Live, "http://manifest.patch.daybreakgames.com/patch/sha/manifest/planetside2/planetside2-livecommon/livenext/planetside2-livecommon.sha.soe.txt" },
        { PS2Environment.PTS, "http://manifest.patch.daybreakgames.com/patch/sha/manifest/planetside2/planetside2-testcommon/livenext/planetside2-testcommon.sha.soe.txt" }
    };

    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManifestService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use.</param>
    public ManifestService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public virtual async Task<ManifestFile> GetFileAsync(string fileName, PS2Environment ps2Environment, CancellationToken ct = default)
    {
        await using Stream manifestData = await _httpClient
            .GetStreamAsync(ManifestUrls[ps2Environment], ct)
            .ConfigureAwait(false);

        using XmlReader reader = XmlReader.Create
        (
            manifestData,
            new XmlReaderSettings { Async = true }
        );

        while (await reader.ReadAsync())
        {
            if (reader.NodeType is not XmlNodeType.Element)
                continue;

            if (reader.Name != "file")
                continue;

            string? nameAttribute = reader.GetAttribute("name");
            if (nameAttribute != fileName)
                continue;

            string? compressedSizeAttribute = reader.GetAttribute("compressedSize");
            if (!int.TryParse(compressedSizeAttribute, out int compressedSize))
                throw new FormatException("compressedSize attribute missing or invalid: " + compressedSizeAttribute);

            string? uncompressedSizeAttribute = reader.GetAttribute("uncompressedSize");
            if (!int.TryParse(uncompressedSizeAttribute, out int uncompressedSize))
                throw new FormatException("uncompressedSize attribute missing or invalid: " + uncompressedSizeAttribute);

            string? crcAttribute = reader.GetAttribute("crc");
            if (!uint.TryParse(crcAttribute, out uint crc))
                throw new FormatException("crc attribute missing or invalid: " + crc);

            string? timestampAttribute = reader.GetAttribute("timestamp");
            if (!long.TryParse(timestampAttribute, out long timestamp))
                throw new FormatException("Timestamp attribute contained an invalid value: " + timestampAttribute);

            string? sha = reader.GetAttribute("sha");
            if (sha is null)
                throw new KeyNotFoundException("sha");

            return new ManifestFile
            (
                nameAttribute,
                compressedSize,
                uncompressedSize,
                crc,
                DateTimeOffset.FromUnixTimeSeconds(timestamp),
                sha,
                ps2Environment
            );
        }

        throw new KeyNotFoundException($"Failed to find the file {fileName}");
    }

    /// <inheritdoc />
    public virtual async Task<Stream> GetFileDataAsync(ManifestFile file, CancellationToken ct = default)
    {
        string downloadPath = $"{ROOT_DOWNLOAD_URL}{file.SHA[..2]}/{file.SHA[2..5]}/{file.SHA[5..]}";
        await using Stream manifestData = await _httpClient.GetStreamAsync(downloadPath, ct).ConfigureAwait(false);

        // We must copy to a MemoryStream so that the data is seekable
        MemoryStream ms = new();

        if (file.CompressedSize < file.UncompressedSize)
           LMZADecompress(file.CompressedSize, manifestData, ms);
        else
            await manifestData.CopyToAsync(ms, ct).ConfigureAwait(false);

        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    private static void LMZADecompress(long inputStreamLength, Stream inputStream, Stream outputStream)
    {
        Decoder decoder = new();

        byte[] properties = new byte[5];
        int amountRead = inputStream.Read(properties, 0, 5);
        if (amountRead != 5)
            throw new Exception("Input is too short");

        long outSize = 0;
        for (int i = 0; i < 8; i++)
        {
            int v = inputStream.ReadByte();
            if (v < 0)
                throw new Exception("Input stream is too short");

            outSize |= (long)(byte)v << (8 * i);
            amountRead++;
        }

        decoder.SetDecoderProperties(properties);
        long compressedSize = inputStreamLength - amountRead;
        decoder.Code(inputStream, outputStream, compressedSize, outSize, null!);
    }
}
