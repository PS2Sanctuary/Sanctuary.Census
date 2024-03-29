﻿namespace Sanctuary.Census.Common.Objects.CommonModels;

/// <summary>
/// Enumerates character genders.
/// </summary>
public enum GenderDefinition : byte
{
    /// <summary>
    /// The given entity can be used on all genders.
    /// </summary>
    All = 0,

    /// <summary>
    /// The given entity can be used on, or represents, male-gendered characters.
    /// </summary>
    Male = 1,

    /// <summary>
    /// The given entity can be used on, or represents, female-gendered characters.
    /// </summary>
    Female = 2
}
