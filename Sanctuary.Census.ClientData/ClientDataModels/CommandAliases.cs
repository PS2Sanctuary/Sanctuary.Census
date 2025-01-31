using System.Collections.Generic;
using System.Xml.Serialization;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents a list of command aliases.
/// </summary>
[XmlRoot("CommandAliases")]
public sealed class CommandAliases
{
    /// <summary>
    /// The list of aliases.
    /// </summary>
    [XmlElement("Alias")]
    public required List<Alias> Aliases { get; set; }
}

/// <summary>
/// Represents an individual command alias.
/// </summary>
[XmlType("Alias")]
public sealed class Alias
{
    /// <summary>
    /// The command string of the alias.
    /// </summary>
    [XmlAttribute("name")]
    public required string Name { get; set; }

    /// <summary>
    /// The command(s) to be performed by this alias.
    /// </summary>
    [XmlAttribute("commandstring")]
    public required string CommandString { get; set; }
}
