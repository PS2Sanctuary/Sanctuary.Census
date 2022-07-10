using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.Objects;
using Sanctuary.Census.ClientData.Objects.ClientDataModels;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.DtoModels;
using Sanctuary.Census.Common.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.ClientData.DataContributors;

/// <summary>
/// Contributes data from the client <see cref="ImageSetMapping"/> data source.
/// </summary>
public class ImageSetMappingDataContributor : BaseDataContributor<ImageSetMapping>
{
    private readonly Dictionary<uint, uint> _imageSetToPrimaryImageMap;

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
        environment
    )
    {
        _imageSetToPrimaryImageMap = new Dictionary<uint, uint>();
    }

    /// <inheritdoc />
    public override bool CanContributeTo<TContributeTo>()
        => typeof(TContributeTo) == typeof(Item);

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
        if (item is not Item i)
            throw GetTypeNotSupportedException<TContributeTo>();

        if (i.ImageSetID is null)
            return new ContributionResult<TContributeTo>(true, item);

        await CacheRecordsAsync(ct).ConfigureAwait(false);

        // If we don't have a requisite mapping it's not a huge deal for this data
        // Return true so we don't botch the build
        if(!_imageSetToPrimaryImageMap.TryGetValue(i.ImageSetID.Value, out uint imageID))
            return new ContributionResult<TContributeTo>(true, item);

        Item contributed = i with
        {
            ImageID = imageID,
            ImagePath = $"/files/ps2/images/static/{imageID}.png"
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
        foreach (ImageSetMapping mapping in records)
        {
            if (mapping.ImageType is not ImageSetType.Large)
                continue;

            _imageSetToPrimaryImageMap[mapping.ImageSetID] = mapping.ImageID;
        }
    }
}
