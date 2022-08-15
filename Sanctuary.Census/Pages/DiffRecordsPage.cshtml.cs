using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sanctuary.Census.Pages;

/// <summary>
/// The page responsible for showing diff records.
/// </summary>
public class DiffRecordsPage : PageModel
{
    private readonly IMongoContext _mongoContext;

    /// <summary>
    /// Gets the environment to pull diffs from.
    /// </summary>
    public PS2Environment Environment { get; private set; }

    /// <summary>
    /// Gets the list of diff records to display.
    /// </summary>
    public IReadOnlyList<DiffRecord>? DiffRecords { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiffRecordsPage"/> class.
    /// </summary>
    /// <param name="mongoContext">The Mongo DB context.</param>
    public DiffRecordsPage(IMongoContext mongoContext)
    {
        _mongoContext = mongoContext;
    }

    /// <summary>
    /// Called when a GET request is made to this page.
    /// </summary>
    public async Task OnGetAsync(string environment)
    {
        Environment = ParseEnvironment(environment);

        IMongoCollection<DiffRecord> diffRecords = _mongoContext.GetCollection<DiffRecord>(Environment);
        DiffRecords = await diffRecords.Find(new BsonDocument())
            .Sort(Builders<DiffRecord>.Sort.Descending(x => x.GeneratedAt))
            .Limit(100)
            .ToListAsync();
    }

    private static PS2Environment ParseEnvironment(string environment)
        => environment.ToLower() switch {
            "pts" => PS2Environment.PTS,
            _ => PS2Environment.PS2
        };
}
