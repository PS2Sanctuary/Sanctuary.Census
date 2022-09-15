namespace Sanctuary.Census.Builder.Abstractions.Services;

/// <summary>
/// Contains helpers for retrieving image set data for use in
/// non-image collections.
/// </summary>
public interface IImageSetHelperService
{
    /// <summary>
    /// Attempts to find the default image of an image set.
    /// </summary>
    /// <param name="imageSetID">The ID of the image set.</param>
    /// <param name="imageID">The ID of the set's default image.</param>
    /// <returns><c>True</c> if a default image set was found, otherwise <c>false</c>.</returns>
    bool TryGetDefaultImage(uint imageSetID, out uint imageID);

    /// <summary>
    /// Gets a relative path to an image.
    /// </summary>
    /// <param name="imageID">The ID of the image.</param>
    /// <returns>The constructed path.</returns>
    string GetRelativeImagePath(uint imageID);
}
