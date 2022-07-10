using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.Objects;
using Sanctuary.Census.ClientData.Objects.ClientDataModels;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Common.Objects.DtoModels;
using Sanctuary.Census.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.ClientData.DataContributors;

/// <summary>
/// Contributes data from the client <see cref="ItemProfile"/> data source.
/// </summary>
public class ItemProfileDataContributor : BaseDataContributor<ItemProfile>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ItemProfileDataContributor"/> class.
    /// </summary>
    /// <param name="datasheetLoader">The datasheet loader service.</param>
    /// <param name="environment">The environment to source data from.</param>
    public ItemProfileDataContributor
    (
        IDatasheetLoaderService datasheetLoader,
        EnvironmentContextProvider environment
    )
    : base
    (
        datasheetLoader,
        new PackedFileInfo("ItemProfiles.txt", "data_x64_0.pack2"),
        environment.Environment,
        new Dictionary<Type, Func<ItemProfile, uint>> {
            { typeof(Item), ip => ip.ItemID }
        }
    )
    {
    }

    /// <inheritdoc />
    public override async ValueTask<ContributionResult<TContributeTo>> ContributeAsync<TContributeTo>
    (
        TContributeTo item,
        CancellationToken ct = default
    ) where TContributeTo : class
    {
        await CacheRecordsAsync(ct).ConfigureAwait(false);

        if (item is not Item i)
            throw GetTypeNotSupportedException<TContributeTo>();

        FactionDefinition faction = FactionDefinition.All;
        if (IndexedStore[typeof(TContributeTo)].TryGetValue(i.ItemID, out ItemProfile? profile))
            faction = profile.FactionID;

        return new ContributionResult<TContributeTo>
        (
            true,
            (i with { FactionID = faction } as TContributeTo)!
        );
    }

    /// <inheritdoc />
    protected override void StoreRecords(IEnumerable<ItemProfile> records)
    {
        List<ItemProfile> enumerated = records.ToList();
        base.StoreRecords(enumerated);

        // Update the Item-indexed store to ensure that items with
        // cross-faction profiles are appropriately stored.
        Dictionary<uint, ItemProfile> store = IndexedStore[typeof(Item)];
        foreach (ItemProfile p in enumerated)
        {
            ItemProfile storedProfile = store[p.ItemID];

            if (storedProfile.FactionID != p.FactionID)
                store[p.ItemID] = storedProfile with { FactionID = FactionDefinition.All };
        }
    }
}
