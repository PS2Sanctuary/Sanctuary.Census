using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.Objects;
using Sanctuary.Census.Common.Abstractions;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.ClientData.DataContributors;

/// <summary>
/// <inheritdoc />
/// This abstract contributor is designed to source data from a packed datasheet.
/// </summary>
/// <typeparam name="TContributeFrom">The model of the source datasheet.</typeparam>
public abstract class BaseDataContributor<TContributeFrom> : IDataContributor
{
    private readonly IDatasheetLoaderService _datasheetLoader;

    private bool _isCached;

    /// <summary>
    /// The datasheet file from which this contributor should source data.
    /// </summary>
    protected PackedFileInfo SourceDatasheet { get; }

    /// <summary>
    /// The environment that this contributor is sourcing data from.
    /// </summary>
    protected PS2Environment Environment { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDataContributor{TContributeFrom}"/>
    /// </summary>
    /// <param name="datasheetLoader">The datasheet loader service.</param>
    /// <param name="sourceDatasheet">The datasheet file from which this contributor should source data.</param>
    /// <param name="environment">The environment to source data from.</param>
    protected BaseDataContributor
    (
        IDatasheetLoaderService datasheetLoader,
        PackedFileInfo sourceDatasheet,
        EnvironmentContextProvider environment
    )
    {
        _datasheetLoader = datasheetLoader;
        SourceDatasheet = sourceDatasheet;
        Environment = environment.Environment;
    }

    /// <inheritdoc />
    public abstract ValueTask<ContributionResult<TContributeTo>> ContributeAsync<TContributeTo>
    (
        TContributeTo item,
        CancellationToken ct = default
    ) where TContributeTo : class;

    /// <inheritdoc />
    public abstract bool CanContributeTo<TContributeTo>();

    /// <inheritdoc />
    public abstract ValueTask<IReadOnlyList<uint>> GetContributableIDsAsync<TContributeTo>(CancellationToken ct = default);

    /// <summary>
    /// Caches all records from the datasheet.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    protected async ValueTask CacheRecordsAsync(CancellationToken ct)
    {
        if (_isCached)
            return;

        IEnumerable<TContributeFrom> records = await _datasheetLoader.LoadDatasheetDataAsync<TContributeFrom>
        (
            SourceDatasheet,
            Environment,
            ct
        ).ConfigureAwait(false);

        StoreRecords(records);
        _isCached = true;
    }

    /// <summary>
    /// Stores data source records.
    /// </summary>
    /// <param name="records">The records to store.</param>
    protected abstract void StoreRecords(IEnumerable<TContributeFrom> records);

    /// <summary>
    /// Creates an exception that should be thrown if this contributor
    /// is asked to contribute to a type that it does not support.
    /// </summary>
    /// <returns></returns>
    protected InvalidOperationException GetTypeNotSupportedException<TContributeTo>()
        => new($"{GetType().Name} cannot contribute to the type {typeof(TContributeTo).Name}");
}
