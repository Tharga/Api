using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace Tharga.Api.Audit;

/// <summary>
/// Audit logger that writes to MongoDB via a background channel for zero-latency impact.
/// Resolves IAuditRepositoryCollection lazily to avoid DI issues in test environments.
/// </summary>
public class MongoDbAuditLogger : BackgroundService, IAuditLogger
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MongoDbAuditLogger> _logger;
    private readonly AuditOptions _options;
    private readonly Channel<AuditEntry> _channel;

    public MongoDbAuditLogger(
        IServiceProvider serviceProvider,
        ILogger<MongoDbAuditLogger> logger,
        IOptions<AuditOptions> options = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options?.Value ?? new AuditOptions();
        _channel = Channel.CreateBounded<AuditEntry>(new BoundedChannelOptions(10_000)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
    }

    public void Log(AuditEntry entry)
    {
        _channel.Writer.TryWrite(entry);
    }

    public async Task<IReadOnlyList<AuditEntry>> QueryAsync(AuditQuery query)
    {
        var collection = _serviceProvider.GetService<IAuditRepositoryCollection>();
        if (collection == null) return Array.Empty<AuditEntry>();

        var entities = new List<AuditEntryEntity>();
        await foreach (var entity in collection.GetAsync(BuildFilter(query)))
        {
            entities.Add(entity);
        }

        return entities
            .OrderByDescending(e => e.Timestamp)
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(ToAuditEntry)
            .ToList();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var collection = _serviceProvider.GetService<IAuditRepositoryCollection>();
        if (collection == null)
        {
            _logger.LogWarning("IAuditRepositoryCollection not registered, MongoDB audit logging disabled");
            return;
        }

        var batch = new List<AuditEntryEntity>();
        var flushInterval = TimeSpan.FromSeconds(_options.FlushIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                cts.CancelAfter(flushInterval);

                while (batch.Count < _options.BatchSize)
                {
                    try
                    {
                        var entry = await _channel.Reader.ReadAsync(cts.Token);
                        batch.Add(ToEntity(entry));
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }

                if (batch.Count > 0)
                {
                    foreach (var entity in batch)
                    {
                        await collection.AddAsync(entity);
                    }
                    batch.Clear();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing audit batch to MongoDB");
                batch.Clear();
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private static System.Linq.Expressions.Expression<Func<AuditEntryEntity, bool>> BuildFilter(AuditQuery query)
    {
        return e =>
            (query.TeamKey == null || e.TeamKey == query.TeamKey) &&
            (query.CallerIdentity == null || e.CallerIdentity == query.CallerIdentity) &&
            (query.CallerType == null || e.CallerType == query.CallerType) &&
            (query.Feature == null || e.Feature == query.Feature) &&
            (query.Action == null || e.Action == query.Action) &&
            (query.CallerSource == null || e.CallerSource == query.CallerSource) &&
            (query.Success == null || e.Success == query.Success) &&
            (query.From == null || e.Timestamp >= query.From) &&
            (query.To == null || e.Timestamp <= query.To);
    }

    private static AuditEntryEntity ToEntity(AuditEntry entry)
    {
        return new AuditEntryEntity
        {
            Id = ObjectId.GenerateNewId(),
            Timestamp = entry.Timestamp,
            CorrelationId = entry.CorrelationId,
            EventType = entry.EventType,
            Feature = entry.Feature,
            Action = entry.Action,
            MethodName = entry.MethodName,
            DurationMs = entry.DurationMs,
            Success = entry.Success,
            ErrorMessage = entry.ErrorMessage,
            CallerType = entry.CallerType,
            CallerIdentity = entry.CallerIdentity,
            TeamKey = entry.TeamKey,
            AccessLevel = entry.AccessLevel,
            CallerSource = entry.CallerSource,
            ScopeChecked = entry.ScopeChecked,
            ScopeResult = entry.ScopeResult,
            Metadata = entry.Metadata,
        };
    }

    private static AuditEntry ToAuditEntry(AuditEntryEntity entity)
    {
        return new AuditEntry
        {
            Timestamp = entity.Timestamp,
            CorrelationId = entity.CorrelationId,
            EventType = entity.EventType,
            Feature = entity.Feature,
            Action = entity.Action,
            MethodName = entity.MethodName,
            DurationMs = entity.DurationMs,
            Success = entity.Success,
            ErrorMessage = entity.ErrorMessage,
            CallerType = entity.CallerType,
            CallerIdentity = entity.CallerIdentity,
            TeamKey = entity.TeamKey,
            AccessLevel = entity.AccessLevel,
            CallerSource = entity.CallerSource,
            ScopeChecked = entity.ScopeChecked,
            ScopeResult = entity.ScopeResult,
            Metadata = entity.Metadata,
        };
    }
}
