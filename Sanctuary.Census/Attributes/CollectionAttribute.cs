using System;

namespace Sanctuary.Census.Attributes;

/// <summary>
/// Indicates that the decorated class is a queryable collection.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CollectionAttribute : Attribute
{
}
