using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Common.Objects.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="Oops"/> collection.
/// </summary>
public sealed class OopsCollectionBuilder : ICollectionBuilder
{
    /// <inheritdoc />
    public async Task BuildAsync(ICollectionsContext dbContext, CancellationToken ct = default)
    {
        List<Oops> builtOopsies = new()
        {
            new Oops
            (
                8000572,
                4900,
                592400255,
                2,
                "SanityPoints",
                "Colossal",
                5428703501287322737,
                "Wrote a Census supplementation"
            ),
            new Oops
            (
                9030608,
                2057,
                316,
                3,
                "SleepHours",
                "Moderate",
                5429104310777173297,
                "Tested Sanctuary stuff"
            )
        };

        await dbContext.UpsertCollectionAsync(builtOopsies, ct);
    }
}
