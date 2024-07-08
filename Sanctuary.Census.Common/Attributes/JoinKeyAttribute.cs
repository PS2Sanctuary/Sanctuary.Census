using System;

namespace Sanctuary.Census.Common.Attributes;

/// <summary>
/// Indicates that the decorated property can be used as an implicit key in joins.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class JoinKeyAttribute : Attribute
{
}
