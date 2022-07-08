using Mandible.Util;

namespace Sanctuary.Census.ClientData.Objects;

/// <summary>
/// Contains information about a packet file.
/// </summary>
/// <param name="FileName">The name of the packed file.</param>
/// <param name="PackName">The name of the pack in which the file can be found.</param>
public record PackedFileInfo
(
    string FileName,
    string PackName
)
{
    /// <summary>
    /// The hash of the <see cref="FileName"/>.
    /// </summary>
    public ulong NameHash { get; } = PackCrc64.Calculate(FileName);
}
