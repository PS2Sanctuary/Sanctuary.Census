using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.Objects;
using Sanctuary.Census.Common.Abstractions;
using Sanctuary.Census.Common.Objects;
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
public abstract class BaseDataContributor<TContributeFrom> : IDataContributor<TContributeFrom>
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
    /// A dictionary of methods that will retrieve the ID value for which the given
    /// key type expects data to be contributed.
    /// </summary>
    protected Dictionary<Type, Func<TContributeFrom, uint>> TypeIDBindings { get; }

    /// <summary>
    /// The type-indexed data store. Call <see cref="CacheRecordsAsync"/>
    /// to ensure that this store is populated.
    /// </summary>
    protected Dictionary<Type, Dictionary<uint, TContributeFrom>> IndexedStore { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDataContributor{TContributeFrom}"/>
    /// </summary>
    /// <param name="datasheetLoader">The datasheet loader service.</param>
    /// <param name="sourceDatasheet">The datasheet file from which this contributor should source data.</param>
    /// <param name="environment">The environment to source data from.</param>
    /// <param name="typeIDBindings">
    /// A dictionary of methods that will retrieve the ID value for which the given
    /// key type expects data to be contributed.
    /// </param>
    protected BaseDataContributor
    (
        IDatasheetLoaderService datasheetLoader,
        PackedFileInfo sourceDatasheet,
        PS2Environment environment,
        Dictionary<Type, Func<TContributeFrom, uint>> typeIDBindings
    )
    {
        _datasheetLoader = datasheetLoader;
        SourceDatasheet = sourceDatasheet;
        Environment = environment;
        TypeIDBindings = typeIDBindings;

        IndexedStore = new Dictionary<Type, Dictionary<uint, TContributeFrom>>();
        foreach (Type element in TypeIDBindings.Keys)
            IndexedStore.Add(element, new Dictionary<uint, TContributeFrom>());
    }

    /// <inheritdoc />
    public abstract ValueTask<ContributionResult<TContributeTo>> ContributeAsync<TContributeTo>(TContributeTo item, CancellationToken ct = default)
        where TContributeTo : class;

    /// <inheritdoc />
    public bool CanContributeTo<TContributeTo>()
        => TypeIDBindings.ContainsKey(typeof(TContributeTo));

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<uint>> GetContributableIDsAsync<TContributeTo>(CancellationToken ct = default)
    {
        if (!CanContributeTo<TContributeTo>())
            throw GetTypeNotSupportedException<TContributeTo>();

        await CacheRecordsAsync(ct).ConfigureAwait(false);
        return IndexedStore[typeof(TContributeTo)].Keys.ToList();
    }

    // /// <inheritdoc />
    // public async ValueTask<TContributeFrom> GetDataAsync(uint id, CancellationToken ct = default)
    // {
    //     await CacheRecordsAsync(ct).ConfigureAwait(false);
    //     return IndexedStore[typeof(TContributeFrom)][id];
    // }

    /// <summary>
    /// Caches all records from the datasheet. Call this to ensure
    /// that <see cref="IndexedStore"/> is populated.
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
    /// Stores the given records in the <see cref="IndexedStore"/>.
    /// </summary>
    /// <param name="records">The records to store.</param>
    protected virtual void StoreRecords(IEnumerable<TContributeFrom> records)
    {
        foreach (TContributeFrom record in records)
        {
            foreach (KeyValuePair<Type, Func<TContributeFrom, uint>> typeIDBinding in TypeIDBindings)
            {
                uint id = typeIDBinding.Value(record);
                IndexedStore[typeIDBinding.Key][id] = record;
            }
        }
    }

    /// <summary>
    /// Creates an exception that should be thrown if this contributor
    /// is asked to contribute to a type that it does not support.
    /// </summary>
    /// <returns></returns>
    protected InvalidOperationException GetTypeNotSupportedException<TContributeTo>()
        => new($"{GetType().Name} cannot contribute to the type {typeof(TContributeTo).Name}");
}
