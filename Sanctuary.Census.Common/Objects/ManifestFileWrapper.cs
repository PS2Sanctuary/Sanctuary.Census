using Mandible.Manifest;

namespace Sanctuary.Census.Common.Objects;

/// <summary>
/// Contains information about a manifest file.
/// </summary>
/// <param name="File">The manifest file.</param>
/// <param name="Owner">The digest that the file was retrieved from.</param>
public record ManifestFileWrapper
(
    ManifestFile File,
    Digest Owner
);
