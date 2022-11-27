using System;

namespace Sanctuary.Census.Common.Attributes;

/// <summary>
/// Indicates that the decorated property is can automatically
/// be used as a key in joins.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class JoinKeyAttribute : Attribute
{
}
