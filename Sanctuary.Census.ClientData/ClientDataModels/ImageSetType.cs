using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client image set types.
/// </summary>
/// <param name="ID">The ID of the image type.</param>
/// <param name="ImageType">Unused.</param>
/// <param name="Description">The description of the image type.</param>
[Datasheet]
public partial record ImageSetType
(
    ImageSetType.Type ID,
    uint ImageType,
    string Description
)
{
    /// <summary>
    /// Enumerates the various image set types.
    /// </summary>
    public enum Type
    {
        /// <summary>
        /// Images with an 8 pixel height.
        /// </summary>
        ExtremelySmall = 3,

        /// <summary>
        /// Images with a 16 pixel height.
        /// </summary>
        VerySmall = 4,

        /// <summary>
        /// Images with a 32 pixel height.
        /// </summary>
        Small = 5,

        /// <summary>
        /// Images with a 64 pixel height.
        /// </summary>
        Medium = 6,

        /// <summary>
        /// Images with a 128 pixel height.
        /// </summary>
        Large = 7,

        /// <summary>
        /// Images with a 256 pixel height.
        /// </summary>
        VeryLarge = 8,

        /// <summary>
        /// Images with no set pixel height.
        /// </summary>
        Massive = 10
    }
}
