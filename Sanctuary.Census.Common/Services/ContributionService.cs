using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sanctuary.Census.Common.Abstractions;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Common.Services;

/// <inheritdoc />
public class ContributionService : IContributionService
{
    private readonly ILogger<ContributionService> _logger;
    private readonly IReadOnlyList<IDataContributor> _contributors;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContributionService"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="services">The service provider.</param>
    /// <param name="contributorRepo">The contributor repository.</param>
    public ContributionService
    (
        ILogger<ContributionService> logger,
        IServiceProvider services,
        IDataContributorTypeRepository contributorRepo
    )
    {
        _logger = logger;
        _contributors = contributorRepo.GetContributors()
            .Select(t => (IDataContributor)services.GetRequiredService(t))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> BuildThroughContributions<T>
    (
        Func<uint, T> typeCreationFactory,
        CancellationToken ct = default
    ) where T : class
    {
        List<IDataContributor> validContributors = new();
        HashSet<uint> idsToRetrieve = new();

        foreach (IDataContributor contributor in _contributors)
        {
            if (!contributor.CanContributeTo<T>())
                continue;

            try
            {
                IReadOnlyList<uint> ids = await contributor.GetContributableIDsAsync<T>(ct).ConfigureAwait(false);
                foreach (uint id in ids)
                    idsToRetrieve.Add(id);
            }
            catch (Exception ex)
            {
                _logger.LogError
                (
                    ex,
                    "Failed to get contributable IDs. The {ContributorName} contributor will not be used",
                    contributor.GetType().Name
                );
                continue;
            }

            validContributors.Add(contributor);
        }

        List<T> builtTypes = new();
        foreach (uint id in idsToRetrieve)
        {
            T toBuild = typeCreationFactory(id);
            bool typeCompleted = true;

            try
            {
                foreach (IDataContributor contributor in validContributors)
                {
                    ContributionResult<T> result = await contributor.ContributeAsync(toBuild, ct).ConfigureAwait(false);
                    toBuild = result.Item;

                    if (!result.WasContributedTo)
                    {
                        typeCompleted = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError
                (
                    ex,
                    "A contributor failed to contribute ID {ID} to the {TypeName} collection",
                    id,
                    typeof(T).Name
                );
            }

            if (typeCompleted)
                builtTypes.Add(toBuild);
        }

        return builtTypes;
    }
}
