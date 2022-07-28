using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.Models;
using System;
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
    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Diff Viewer";

        IMongoCollection<DiffRecord> diffRecords = _mongoContext.GetCollection<DiffRecord>();
        DiffRecords = await diffRecords.Find(new BsonDocument())
            .Sort(Builders<DiffRecord>.Sort.Descending(x => x.GeneratedAt))
            .Limit(100)
            .ToListAsync();

        Console.WriteLine("test");
    }
}
