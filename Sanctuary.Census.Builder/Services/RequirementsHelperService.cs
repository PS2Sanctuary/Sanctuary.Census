using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sanctuary.Census.Builder.Services;

/// <inheritdoc />
public class RequirementsHelperService : IRequirementsHelperService
{
    private readonly IClientDataCacheService _clientDataCache;

    private IReadOnlyDictionary<uint, string>? _clientExpressionMappings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSetHelperService"/>.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    public RequirementsHelperService(IClientDataCacheService clientDataCache)
    {
        _clientDataCache = clientDataCache;
    }

    /// <inheritdoc />
    public bool TryGetClientExpression(uint id, [NotNullWhen(true)] out string? expression)
    {
        expression = null;
        if (id == 0)
            return false;

        if (_clientDataCache.ClientRequirementExpressions is null)
            throw new MissingCacheDataException(typeof(ClientRequirementExpression));

        _clientExpressionMappings ??= _clientDataCache.ClientRequirementExpressions
            .ToDictionary(x => x.ID, x => x.Expression);

        return _clientExpressionMappings.TryGetValue(id, out expression);
    }
}
