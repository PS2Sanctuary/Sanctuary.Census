using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.Objects;
using Sanctuary.Census.ClientData.Objects.ClientDataModels;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.DtoModels;
using Sanctuary.Census.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.ClientData.DataContributors;

/// <summary>
/// Contributes data from the client <see cref="ImageSetMapping"/> data source.
/// </summary>
public class ImageSetMappingDataContributor : BaseDataContributor<ImageSetMapping>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSetMappingDataContributor"/> class.
    /// </summary>
    /// <param name="datasheetLoader">The datasheet loader service.</param>
    /// <param name="environment">The environment to source data from.</param>
    public ImageSetMappingDataContributor
    (
        IDatasheetLoaderService datasheetLoader,
        EnvironmentContextProvider environment
    )
    : base
    (
        datasheetLoader,
        new PackedFileInfo("ImageSetMappings.txt", "data_x64_0.pack2"),
        environment.Environment,
        new Dictionary<Type, Func<ImageSetMapping, uint>> {
            { typeof(Item), ism => ism.ImageSetID }
        }
    )
    {
    }

    /// <inheritdoc />
    public override ValueTask<IReadOnlyList<uint>> GetContributableIDsAsync<TContributeTo>(CancellationToken ct = default)
        => new(Array.Empty<uint>());

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

        if (i.ImageSetID is null)
            return new ContributionResult<TContributeTo>(true, item);

        // If we don't have a requisite mapping it's not a huge deal for this data
        // Return true so we don't botch the build
        if(!IndexedStore[typeof(Item)].TryGetValue(i.ImageSetID.Value, out ImageSetMapping? mapping))
            return new ContributionResult<TContributeTo>(true, item);

        Item contributed = i with
        {
            ImageID = mapping.ImageID,
            ImagePath = $"/files/ps2/images/static/{mapping.ImageID}.png"
        };

        return new ContributionResult<TContributeTo>
        (
            true,
            (contributed as TContributeTo)!
        );
    }

    /// <inheritdoc />
    protected override void StoreRecords(IEnumerable<ImageSetMapping> records)
    {
        List<ImageSetMapping> enumerated = records.ToList();
        base.StoreRecords(enumerated);

        // Update the Item-indexed store to ensure that only
        // the large-size image mappings are stored
        Dictionary<uint, ImageSetMapping> store = IndexedStore[typeof(Item)];
        foreach (ImageSetMapping p in enumerated)
        {
            if (p.ImageType is not ImageSetType.Large)
                continue;

            ImageSetMapping storedProfile = store[p.ImageSetID];
            if (storedProfile.ImageType != p.ImageType)
                store[p.ImageSetID] = p;
        }
    }
}
