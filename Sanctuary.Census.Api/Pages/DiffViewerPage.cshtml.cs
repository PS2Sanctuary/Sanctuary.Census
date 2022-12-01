using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Sanctuary.Census.Common;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Json;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.DiffModels;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sanctuary.Census.Api.Pages;

/// <summary>
/// The page responsible for showing a diff set.
/// </summary>
public class DiffViewerPage : PageModel
{
    private static readonly JsonSerializerOptions EntryDisplayJsonOptions;
    private static readonly Dictionary<Type, PropertyInfo[]> CollectionTypeProperties;

    private readonly IMongoContext _mongoContext;

    /// <summary>
    /// Gets the environment to pull diffs from.
    /// </summary>
    public PS2Environment Environment { get; private set; }

    /// <summary>
    /// Gets the time at which this diff was generated.
    /// </summary>
    public DateTime DiffTime { get; private set; }

    /// <summary>
    /// Gets the diff entries, bucketed by collection name.
    /// </summary>
    public Dictionary<string, List<DiffObjectsDisplay>> DiffEntryBuckets { get; private set; }

    static DiffViewerPage()
    {
        EntryDisplayJsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy(),
            WriteIndented = true
        };
        CollectionTypeProperties = new Dictionary<Type, PropertyInfo[]>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiffViewerPage"/> class.
    /// </summary>
    /// <param name="mongoContext">The Mongo DB context.</param>
    public DiffViewerPage(IMongoContext mongoContext)
    {
        _mongoContext = mongoContext;
        DiffEntryBuckets = new Dictionary<string, List<DiffObjectsDisplay>>();
    }

    /// <summary>
    /// Called when a GET request is made to this page.
    /// </summary>
    public async Task OnGetAsync(PS2Environment environment, long time)
    {
        Environment = environment;
        DiffTime = new DateTime(time);

        IMongoCollection<CollectionDiffEntry> diffEntries = _mongoContext.GetCollection<CollectionDiffEntry>(environment);
        FilterDefinition<CollectionDiffEntry> filter = Builders<CollectionDiffEntry>.Filter.Eq(x => x.DiffTime, DiffTime.ToLocalTime());
        SortDefinition<CollectionDiffEntry> sort = Builders<CollectionDiffEntry>.Sort.Ascending(x => x.CollectionName);

        IReadOnlyList<CollectionDiffEntry> entries = await diffEntries.Find(filter)
            .Sort(sort)
            .ToListAsync();

        foreach (CollectionDiffEntry diff in entries)
        {
            // This should never occur, but worth checking
            if (diff.OldObject is null && diff.NewObject is null)
                continue;

            DiffEntryBuckets.TryAdd(diff.CollectionName, new List<DiffObjectsDisplay>());

            object? oldObject = diff.OldObject;
            object? newObject = diff.NewObject;

            if (oldObject is not null && newObject is not null)
            {
                ExpandoObject tempOld = new();
                ExpandoObject tempNew = new();

                void AddProperty(PropertyInfo prop)
                {
                    string name = SnakeCaseJsonNamingPolicy.Default.ConvertName(prop.Name);
                    tempOld.TryAdd(name, prop.GetValue(diff.OldObject));
                    tempNew.TryAdd(name, prop.GetValue(diff.NewObject));
                }

                foreach (PropertyInfo prop in GetCollectionTypeProperties(diff))
                {
                    if (prop.Name.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                    {
                        AddProperty(prop);
                    }
                    else
                    {
                        object? oldValue = prop.GetValue(oldObject);
                        object? newValue = prop.GetValue(newObject);

                        if (oldValue is null && newValue is not null || oldValue is not null && newValue is null)
                            AddProperty(prop);
                        else if (oldValue is not null && newValue is not null && !oldValue.Equals(newValue))
                            AddProperty(prop);
                    }
                }

                oldObject = tempOld;
                newObject = tempNew;
            }

            string? oldObjectString = null;
            if (oldObject is not null)
                oldObjectString = JsonSerializer.Serialize(oldObject, EntryDisplayJsonOptions);

            string? newObjectString = null;
            if (newObject is not null)
                newObjectString = JsonSerializer.Serialize(newObject, EntryDisplayJsonOptions);

            DiffObjectsDisplay objectsDisplay = new(oldObjectString, newObjectString);
            DiffEntryBuckets[diff.CollectionName].Add(objectsDisplay);
        }
    }

    private static PropertyInfo[] GetCollectionTypeProperties(CollectionDiffEntry diffEntry)
    {
        Type type = diffEntry.OldObject?.GetType() ?? diffEntry.NewObject!.GetType();

        if (CollectionTypeProperties.ContainsKey(type))
            return CollectionTypeProperties[type];

        PropertyInfo[] props = type.GetProperties();
        CollectionTypeProperties[type] = props;

        return props;
    }

    /// <summary>
    /// Represents stringified diff entry objects.
    /// </summary>
    /// <param name="Old">The old object.</param>
    /// <param name="New">The new object.</param>
    public record DiffObjectsDisplay(string? Old, string? New);
}
