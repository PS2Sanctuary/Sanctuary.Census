using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;

namespace Sanctuary.Census.Database;

/// <summary>
/// Builds a MongoDB aggregate projection operator from a query show/hide string.
/// </summary>
public class ProjectionBuilder
{
    private readonly IEnumerable<string>? _show;
    private readonly IEnumerable<string>? _hide;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectionBuilder"/> class.
    /// </summary>
    /// <param name="show">The fields to include.</param>
    /// <param name="hide">The fields to exclude.</param>
    public ProjectionBuilder(IEnumerable<string>? show, IEnumerable<string>? hide)
    {
        _show = show;
        _hide = hide;
    }

    /// <summary>
    /// Builds the projection definition, automatically ignoring any <c>_id</c> fields.
    /// </summary>
    /// <returns>The built projection definition.</returns>
    public ProjectionDefinition<BsonDocument> Build()
    {
        ProjectionDefinition<BsonDocument>? projection = Builders<BsonDocument>.Projection
            .Exclude("_id");

        if (_show is not null)
        {
            foreach (string value in _show)
                projection = projection.Include(value);
        }

        if (_hide is not null)
        {
            foreach (string value in _hide)
                projection = projection.Exclude(value);
        }

        return projection;
    }
}
