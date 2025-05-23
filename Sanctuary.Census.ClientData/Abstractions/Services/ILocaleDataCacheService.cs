﻿using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.Diagnostics.CodeAnalysis;

namespace Sanctuary.Census.ClientData.Abstractions.Services;

/// <summary>
/// Represents a service for retrieving locale strings.
/// </summary>
public interface ILocaleDataCacheService : IDataCacheService
{
    /// <summary>
    /// Attempts to retrieve a locale string.
    /// </summary>
    /// <param name="stringID">The code ID of the string.</param>
    /// <param name="localeString">The locale string, or <c>null</c> if the <paramref name="stringID"/> is invalid.</param>
    /// <returns><c>True</c> if the given <paramref name="stringID"/> pointed to a valid locale string.</returns>
    bool TryGetLocaleString(long stringID, [NotNullWhen(true)] out LocaleString? localeString);

    /// <summary>
    /// Attempts to retrieve a locale string.
    /// </summary>
    /// <param name="stringID">The code ID of the string.</param>
    /// <returns>
    /// A valid locale string, or <c>null</c> if the given <paramref name="stringID"/> could not be resolved.
    /// </returns>
    LocaleString? GetLocaleStringOrNull(long stringID);
}
