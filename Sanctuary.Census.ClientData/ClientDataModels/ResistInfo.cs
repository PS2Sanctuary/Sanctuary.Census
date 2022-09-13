using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client resist info.
/// </summary>
/// <param name="ID">The ID of the resist entry.</param>
/// <param name="ResistType">The type of the resist.</param>
/// <param name="ResistPercent">The percentage resistance applied.</param>
/// <param name="ResistAmount">Unknown.</param>
/// <param name="MultiplierWhenHeadshot">The damage multiplier that a headshot has against this resistance.</param>
/// <param name="ResistIcon">The ID of the resist's image set.</param>
/// <param name="ResistSoundID">The ID of the resist's sound.</param>
[Datasheet]
public partial record ResistInfo
(
    uint ID,
    ushort ResistType,
    int ResistPercent,
    int ResistAmount,
    decimal MultiplierWhenHeadshot,
    uint ResistIcon,
    uint ResistSoundID
);
