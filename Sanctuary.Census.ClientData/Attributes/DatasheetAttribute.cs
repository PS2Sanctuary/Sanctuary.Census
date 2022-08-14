using System;

namespace Sanctuary.Census.ClientData.Attributes;

/// <summary>
/// Indicates that the decorated class should be extended with
/// source-generated deserialization logic for datasheets.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class DatasheetAttribute : Attribute
{
}
