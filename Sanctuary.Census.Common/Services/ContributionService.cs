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
    private readonly IReadOnlyList<IDataContributor> _contributors;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContributionService"/> class.
    /// </summary>
    /// <param name="services">The service provider.</param>
    /// <param name="contributorRepo">The contributor repository.</param>
    public ContributionService(IServiceProvider services, IDataContributorTypeRepository contributorRepo)
    {
        _contributors = contributorRepo.GetContributors()
            .Select(t => contributorRepo.BuildContributor(t, services, PS2Environment.Live))
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

            validContributors.Add(contributor);
            IReadOnlyList<uint> ids = await contributor.GetContributableIDsAsync<T>(ct).ConfigureAwait(false);
            foreach (uint id in ids)
                idsToRetrieve.Add(id);
        }

        List<T> builtTypes = new();
        foreach (uint id in idsToRetrieve)
        {
            T toBuild = typeCreationFactory(id);
            bool typeCompleted = true;

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

            if (typeCompleted)
                builtTypes.Add(toBuild);
        }

        return builtTypes;
    }
}
