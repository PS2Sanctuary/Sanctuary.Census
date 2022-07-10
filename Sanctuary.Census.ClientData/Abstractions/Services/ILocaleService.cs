using Sanctuary.Census.Common.Objects.CommonModels;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.ClientData.Abstractions.Services;

/// <summary>
/// Represents a service for retrieving locale strings.
/// </summary>
public interface ILocaleService
{
    /// <summary>
    /// Gets a locale string.
    /// </summary>
    /// <param name="stringID">The ID of the string.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A locale string, or <c>null</c> if the <paramref name="stringID"/> is invalid.</returns>
    ValueTask<LocaleString?> GetLocaleStringAsync(uint stringID, CancellationToken ct = default);
}
