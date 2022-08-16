using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Sanctuary.Census.Common;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.DiffModels;
using Sanctuary.Census.Json;
using Sanctuary.Census.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sanctuary.Census.Pages;

/// <summary>
/// The page responsible for showing a diff set.
/// </summary>
public class DiffViewerPage : PageModel
{
    private static readonly JsonSerializerOptions EntryDisplayJsonOptions;

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
            DiffEntryBuckets.TryAdd(diff.CollectionName, new List<DiffObjectsDisplay>());

            string? oldObject = null;
            if (diff.OldObject is not null)
                oldObject = JsonSerializer.Serialize(diff.OldObject, EntryDisplayJsonOptions);

            string? newObject = null;
            if (diff.NewObject is not null)
                newObject = JsonSerializer.Serialize(diff.NewObject, EntryDisplayJsonOptions);

            DiffObjectsDisplay objectsDisplay = new(oldObject, newObject);
            DiffEntryBuckets[diff.CollectionName].Add(objectsDisplay);
        }
    }

    /// <summary>
    /// Represents stringified diff entry objects.
    /// </summary>
    /// <param name="Old">The old object.</param>
    /// <param name="New">The new object.</param>
    public record DiffObjectsDisplay(string? Old, string? New);
}
