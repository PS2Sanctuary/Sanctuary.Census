using System;

namespace Sanctuary.Census.Common.Attributes;

/// <summary>
/// Indicates that the decorated class is a queryable collection.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CollectionAttribute : Attribute
{
    /// <summary>
    /// If <c>true</c>, this collection shouldn't be considered
    /// as a top-level entity, but should still be processed
    /// as queryable.
    /// </summary>
    public bool IsNestedType { get; set; }
}
