using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;

namespace Sanctuary.Census.Api.Database;

/// <summary>
/// Builds a MongoDB aggregate projection operator from a query show/hide string.
/// </summary>
public class ProjectionBuilder
{
    private readonly List<string> _projections;

    /// <summary>
    /// Gets a value indicating whether this is an exclusion projection.
    /// </summary>
    public bool IsExclusionProjection { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectionBuilder"/> class.
    /// </summary>
    /// <param name="isExclusionProjection">Indicates whether this is an exclusion projection.</param>
    public ProjectionBuilder(bool isExclusionProjection)
    {
        IsExclusionProjection = isExclusionProjection;
        _projections = new List<string>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectionBuilder"/> class.
    /// </summary>
    /// <param name="isExclusionProjection">Indicates whether this is an exclusion projection.</param>
    /// <param name="projections">The fields to project.</param>
    public ProjectionBuilder(bool isExclusionProjection, IEnumerable<string> projections)
        : this(isExclusionProjection)
    {
        _projections.AddRange(projections);
    }

    /// <summary>
    /// Builds the projection definition, automatically ignoring any <c>_id</c> fields.
    /// </summary>
    /// <returns>The built projection definition.</returns>
    public ProjectionDefinition<BsonDocument> Build()
    {
        ProjectionDefinition<BsonDocument>? projection = Builders<BsonDocument>.Projection
            .Exclude("_id");

        foreach (string value in _projections)
        {
            projection = IsExclusionProjection
                ? projection.Exclude(value)
                : projection.Include(value);
        }

        return projection;
    }

    /// <summary>
    /// Includes a field in the projection.
    /// </summary>
    /// <param name="fieldToInclude">The name of the field to include.</param>
    public void Project(string fieldToInclude)
        => _projections.Add(fieldToInclude);

    /// <summary>
    /// Removes a field from the projection.
    /// </summary>
    /// <param name="fieldToRemove">The name of the field to remove.</param>
    public void RemoveProjection(string fieldToRemove)
        => _projections.Remove(fieldToRemove);

    /// <summary>
    /// Gets a value indicating whether a projection is set for the given field.
    /// </summary>
    /// <param name="field">The name of the field.</param>
    /// <returns><c>True</c> if the field is being projected, otherwise <c>False</c>.</returns>
    public bool ContainsProjection(string field)
        => _projections.Contains(field);
}
