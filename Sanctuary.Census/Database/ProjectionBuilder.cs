using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;

namespace Sanctuary.Census.Database;

/// <summary>
/// Builds a MongoDB aggregate projection operator from a query show/hide string.
/// </summary>
public class ProjectionBuilder
{
    private readonly List<string> _show;
    private readonly List<string> _hide;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectionBuilder"/> class.
    /// </summary>
    public ProjectionBuilder()
    {
        _show = new List<string>();
        _hide = new List<string>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectionBuilder"/> class.
    /// </summary>
    /// <param name="show">The fields to include.</param>
    /// <param name="hide">The fields to exclude.</param>
    public ProjectionBuilder(IEnumerable<string>? show, IEnumerable<string>? hide)
        : this()
    {
        if (show is not null)
            _show.AddRange(show);
        if (hide is not null)
            _hide.AddRange(hide);
    }

    /// <summary>
    /// Builds the projection definition, automatically ignoring any <c>_id</c> fields.
    /// </summary>
    /// <returns>The built projection definition.</returns>
    public ProjectionDefinition<BsonDocument> Build()
    {
        ProjectionDefinition<BsonDocument>? projection = Builders<BsonDocument>.Projection
            .Exclude("_id");

        foreach (string value in _show)
            projection = projection.Include(value);

        foreach (string value in _hide)
            projection = projection.Exclude(value);

        return projection;
    }

    /// <summary>
    /// Includes a field in the projection.
    /// </summary>
    /// <param name="fieldToInclude">The name of the field to include.</param>
    public void Include(string fieldToInclude)
        => _show.Add(fieldToInclude);

    /// <summary>
    /// Excludes a field from the projection.
    /// </summary>
    /// <param name="fieldToExclude">The name of the field to exclude.</param>
    public void Exclude(string fieldToExclude)
        => _hide.Add(fieldToExclude);
}
