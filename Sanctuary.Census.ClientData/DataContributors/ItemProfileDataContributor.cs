using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.Objects;
using Sanctuary.Census.ClientData.Objects.ClientDataModels;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Common.Objects.DtoModels;
using Sanctuary.Census.Common.Services;
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
    private readonly Dictionary<uint, FactionDefinition> _itemFactionMap;

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
        environment
    )
    {
        _itemFactionMap = new Dictionary<uint, FactionDefinition>();
    }

    /// <inheritdoc />
    public override bool CanContributeTo<TContributeTo>()
        => typeof(TContributeTo) == typeof(Item);

    /// <inheritdoc />
    public override async ValueTask<IReadOnlyList<uint>> GetContributableIDsAsync<TContributeTo>(CancellationToken ct = default)
    {
        if (typeof(TContributeTo) != typeof(Item))
            throw GetTypeNotSupportedException<TContributeTo>();

        await CacheRecordsAsync(ct).ConfigureAwait(false);
        return _itemFactionMap.Keys.ToList();
    }

    /// <inheritdoc />
    public override async ValueTask<ContributionResult<TContributeTo>> ContributeAsync<TContributeTo>
    (
        TContributeTo item,
        CancellationToken ct = default
    ) where TContributeTo : class
    {
        if (item is not Item i)
            throw GetTypeNotSupportedException<TContributeTo>();

        await CacheRecordsAsync(ct).ConfigureAwait(false);

        _itemFactionMap.TryGetValue(i.ItemID, out FactionDefinition faction);
        return new ContributionResult<TContributeTo>
        (
            true,
            (i with { FactionID = faction } as TContributeTo)!
        );
    }

    /// <inheritdoc />
    protected override void StoreRecords(IEnumerable<ItemProfile> records)
    {
        foreach (ItemProfile profile in records)
        {
            _itemFactionMap.TryAdd(profile.ItemID, profile.FactionID);
            if (_itemFactionMap[profile.ItemID] != profile.FactionID)
                _itemFactionMap[profile.ItemID] = FactionDefinition.All;
        }
    }
}
