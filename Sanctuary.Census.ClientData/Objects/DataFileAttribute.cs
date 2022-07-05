using System;

namespace Sanctuary.Census.ClientData.Objects;

/// <summary>
/// Indicates that any classes decorated with this attribute
/// map directly to a client data file.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class DataFileAttribute : Attribute
{
    /// <summary>
    /// The name of the file that this data type corresponds to.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataFileAttribute"/> class.
    /// </summary>
    /// <param name="fileName">The name of the file that this data type corresponds to.</param>
    public DataFileAttribute(string fileName)
    {
        FileName = fileName;
    }
}
