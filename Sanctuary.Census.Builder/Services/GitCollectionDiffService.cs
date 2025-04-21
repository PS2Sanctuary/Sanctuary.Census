using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Objects;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Json;
using Sanctuary.Census.Common.Objects;
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

    private readonly ILogger<GitCollectionDiffService> _logger;
    private readonly GitOptions _gitOptions;
    private readonly IMongoContext _mongoContext;
    private readonly EnvironmentContextProvider _environmentContextProvider;

    private readonly HashSet<Type> _modifiedCollections = [];
    private readonly FetchOptions _gitFetchOptions = new();
    private readonly Identity _gitIdentity;

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
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="commonOptions">The common options.</param>
    /// <param name="gitOptions">The Git options.</param>
    /// <param name="mongoContext">The Mongo DB context.</param>
    /// <param name="environmentContextProvider">The environment context provider.</param>
    public GitCollectionDiffService
    (
        ILogger<GitCollectionDiffService> logger,
        IOptions<CommonOptions> commonOptions,
        IOptions<GitOptions> gitOptions,
        IMongoContext mongoContext,
        EnvironmentContextProvider environmentContextProvider
    )
    {
        _logger = logger;
        _gitOptions = gitOptions.Value;
        _gitOptions.LocalRepositoryPath ??= Path.Combine(commonOptions.Value.AppDataDirectory, "git-diffs-repo");
        _mongoContext = mongoContext;
        _environmentContextProvider = environmentContextProvider;

        if (!string.IsNullOrEmpty(_gitOptions.RemoteUsername))
        {
            _gitFetchOptions.CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials
            {
                Username = _gitOptions.RemoteUsername,
                Password = _gitOptions.RemotePassword
            };
        }
        _gitIdentity = new Identity(_gitOptions.CommitUserName, _gitOptions.CommitEmail);
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
        if (!_gitOptions.DoGitDiffing)
            return;

        using Repository repo = SetupRepository();

        string envPath = Path.Combine(repo.Info.WorkingDirectory, _environmentContextProvider.Environment.ToString());
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
            await JsonSerializer.SerializeAsync(fs, items, JSON_OPTIONS, ct);
        }

        // Stage everything
        Commands.Stage(repo, "*");

        if (repo.RetrieveStatus().IsDirty)
        {
            Signature commitSig = new(_gitIdentity, DateTimeOffset.UtcNow);
            Commit commit = repo.Commit
            (
                $"[{_environmentContextProvider.Environment}] {DateTimeOffset.UtcNow.ToString(CultureInfo.InvariantCulture)}",
                commitSig,
                commitSig
            );
            _logger.LogInformation("Created commit {Hash} in git diff repo ({Message})", commit.Sha, commit.MessageShort);
        }

        if (_gitOptions.PushToRemote)
        {
            Branch branchToPush = repo.Branches[_gitOptions.BranchName];
            if (branchToPush.IsTracking)
            {
                PushOptions po = new();
                po.CredentialsProvider = _gitFetchOptions.CredentialsProvider;
                repo.Network.Push(branchToPush, po);
                _logger.LogDebug("Successfully pushed git diff repo to remote");
            }
            else
            {
                throw new InvalidOperationException("Push to remote is enabled, but the local branch is not tracking");
            }
        }
    }

    private Repository SetupRepository()
    {
        if (!Repository.IsValid(_gitOptions.LocalRepositoryPath))
        {
            if (string.IsNullOrEmpty(_gitOptions.RemoteHttpUrl))
                throw new InvalidOperationException("The remote git URL is null, but no repository exists locally");

            _logger.LogInformation
            (
                "Git diff repo does not exist locally, cloning into {RepoPath}",
                _gitOptions.LocalRepositoryPath
            );
            CloneOptions co = new(_gitFetchOptions);
            Repository.Clone(_gitOptions.RemoteHttpUrl, _gitOptions.LocalRepositoryPath, co);
        }

        Repository repo = new(_gitOptions.LocalRepositoryPath);

        Branch branch = repo.Branches[_gitOptions.BranchName];
        if (branch is not null)
        {
            Commands.Checkout(repo, repo.Branches[_gitOptions.BranchName]);

            if (branch.IsTracking)
            {
                PullOptions po = new();
                po.FetchOptions = _gitFetchOptions;
                Commands.Pull(repo, new Signature(_gitIdentity, DateTimeOffset.UtcNow), po);
            }
        }


        _logger.LogDebug("Git diff repo successfully pulled");
        return repo;
    }
}
