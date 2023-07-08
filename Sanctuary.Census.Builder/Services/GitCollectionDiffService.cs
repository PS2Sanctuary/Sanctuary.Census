using CliWrap;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Objects;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Json;
using Sanctuary.Census.Common.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.Services;

/// <inheritdoc />
public class GitCollectionDiffService : ICollectionDiffService
{
    private static readonly JsonSerializerOptions JSON_OPTIONS;
    private static readonly IReadOnlyList<Type> _knownCollectionTypes;

    private readonly string _gitDiffRepoPath;
    private readonly bool _pushOnCommit;
    private readonly IMongoContext _mongoContext;
    private readonly EnvironmentContextProvider _environmentContextProvider;
    private readonly HashSet<Type> _modifiedCollections;

    static GitCollectionDiffService()
    {
        JSON_OPTIONS = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            NumberHandling = JsonNumberHandling.WriteAsString,
            PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy(),
            WriteIndented = true
        };

        JSON_OPTIONS.Converters.Insert(0, new BooleanJsonConverter(true));
        JSON_OPTIONS.Converters.Add(new BsonDecimal128JsonConverter());
        JSON_OPTIONS.Converters.Add(new BsonDocumentJsonConverter(true));

        _knownCollectionTypes = typeof(CollectionAttribute).Assembly
            .GetTypes()
            .Where(t => t.IsDefined(typeof(CollectionAttribute)))
            .ToList();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GitCollectionDiffService"/> class.
    /// </summary>
    /// <param name="options">The builder options.</param>
    /// <param name="mongoContext">The Mongo DB context.</param>
    /// <param name="environmentContextProvider">The environment context provider.</param>
    public GitCollectionDiffService
    (
        IOptions<BuildOptions> options,
        IMongoContext mongoContext,
        EnvironmentContextProvider environmentContextProvider
    )
    {
        _mongoContext = mongoContext;
        _environmentContextProvider = environmentContextProvider;
        _modifiedCollections = new HashSet<Type>();

        _pushOnCommit = options.Value.PushGitDiffToRemote;
        _gitDiffRepoPath = options.Value.GitDiffRepoPath
            ?? throw new InvalidOperationException("A path to the git diff repo must be provided");
    }

    /// <inheritdoc />
    public void SetAdded<T>(T document)
        => _modifiedCollections.Add(typeof(T));

    /// <inheritdoc />
    public void SetDeleted<T>(T document)
        => _modifiedCollections.Add(typeof(T));

    /// <inheritdoc />
    public void SetUpdated<T>(T oldDocument, T newDocument)
        => _modifiedCollections.Add(typeof(T));

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (!Directory.Exists(_gitDiffRepoPath))
            throw new DirectoryNotFoundException($"The git diff repo path was not found: {_gitDiffRepoPath}");

        string envPath = Path.Combine(_gitDiffRepoPath, _environmentContextProvider.Environment.ToString());
        Directory.CreateDirectory(envPath);

        // Add collections that haven't been changed, but were never added to the diff
        foreach (Type collType in _knownCollectionTypes)
        {
            string collPath = Path.Combine
            (
                envPath,
                SnakeCaseJsonNamingPolicy.Default.ConvertName(collType.Name) + ".json"
            );
            if (!File.Exists(collPath))
                _modifiedCollections.Add(collType);
        }

        if (_modifiedCollections.Count == 0)
            return;

        foreach (Type collType in _modifiedCollections)
        {
            IMongoCollection<BsonDocument> collection = _mongoContext
                .GetCollection(collType, _environmentContextProvider.Environment);

            List<BsonDocument> items = await collection.Find(new BsonDocument())
                .Project
                (
                    Builders<BsonDocument>.Projection.Exclude("_id")
                )
                .ToListAsync(ct);

            string collName = SnakeCaseJsonNamingPolicy.Default.ConvertName(collType.Name);
            string collPath = Path.Combine(envPath, collName + ".json");

            await using FileStream fs = new(collPath, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(fs, items, JSON_OPTIONS, ct).ConfigureAwait(false);
        }

        CommandResult addResult = await Cli.Wrap("git")
            .WithArguments("add -A")
            .WithWorkingDirectory(_gitDiffRepoPath)
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync(ct)
            .ConfigureAwait(false);
        if (addResult.ExitCode is not (0 or 141)) // 141 thrown because STDOUT closed too fast
            throw new Exception($"Failed to stage diff files. Exit code {addResult.ExitCode}");

        CommandResult commitResult = await Cli.Wrap("git")
            .WithArguments($"commit -m \"[{_environmentContextProvider.Environment}] {DateTimeOffset.UtcNow.ToString(CultureInfo.InvariantCulture)}\"")
            .WithWorkingDirectory(_gitDiffRepoPath)
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync(ct)
            .ConfigureAwait(false);
        if (commitResult.ExitCode is not (0 or 141)) // 141 thrown because STDOUT closed too fast
            throw new Exception($"Failed to commit diff files. Exit code {commitResult.ExitCode}");

        if (_pushOnCommit)
        {
            CommandResult pushResult = await Cli.Wrap("git")
                .WithArguments("push")
                .WithWorkingDirectory(_gitDiffRepoPath)
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync(ct)
                .ConfigureAwait(false);
            if (pushResult.ExitCode is not (0 or 141)) // 141 thrown because STDOUT closed too fast
                throw new Exception($"Failed to push diff files. Exit code {pushResult.ExitCode}");
        }
    }
}
