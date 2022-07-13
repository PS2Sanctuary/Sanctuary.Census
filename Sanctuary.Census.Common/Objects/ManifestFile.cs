using System;

namespace Sanctuary.Census.Common.Objects;

/// <summary>
/// Represents a file object in a PlanetSide patch manifest.
/// </summary>
/// <param name="FileName">The name of the file.</param>
/// <param name="CompressedSize">The compressed size of the file in bytes.</param>
/// <param name="UncompressedSize">The uncompressed size of the file in bytes.</param>
/// <param name="Crc32">A CRC-32 hash of the file contents.</param>
/// <param name="Timestamp">The time at which the file was last updated.</param>
/// <param name="SHA">A SHA hash that can be used to identify and download the file entry.</param>
public record ManifestFile
(
    string FileName,
    int CompressedSize,
    int UncompressedSize,
    uint Crc32,
    DateTimeOffset Timestamp,
    string SHA
);
