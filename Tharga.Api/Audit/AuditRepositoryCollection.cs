using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Tharga.MongoDB;
using Tharga.MongoDB.Disk;

namespace Tharga.Api.Audit;

internal class AuditRepositoryCollection : DiskRepositoryCollectionBase<AuditEntryEntity>, IAuditRepositoryCollection
{
    private readonly int _retentionDays;

    public AuditRepositoryCollection(
        IMongoDbServiceFactory mongoDbServiceFactory,
        ILogger<AuditRepositoryCollection> logger,
        Microsoft.Extensions.Options.IOptions<AuditOptions> options = null)
        : base(mongoDbServiceFactory, logger)
    {
        _retentionDays = options?.Value?.RetentionDays ?? 90;
    }

    public override string CollectionName => "AuditLog";

    public override IEnumerable<CreateIndexModel<AuditEntryEntity>> Indices =>
    [
        // TTL index for automatic retention cleanup
        new(Builders<AuditEntryEntity>.IndexKeys.Ascending(x => x.Timestamp),
            new CreateIndexOptions { Name = "Timestamp_TTL", ExpireAfter = TimeSpan.FromDays(_retentionDays) }),

        // Compound index for default sort (descending time) with team filter
        new(Builders<AuditEntryEntity>.IndexKeys
                .Descending(x => x.Timestamp)
                .Ascending(x => x.TeamKey),
            new CreateIndexOptions { Name = "Timestamp_desc_TeamKey" }),

        // Individual indices for $in queries from multi-select filters
        new(Builders<AuditEntryEntity>.IndexKeys.Ascending(x => x.Feature),
            new CreateIndexOptions { Name = "Feature" }),
        new(Builders<AuditEntryEntity>.IndexKeys.Ascending(x => x.Action),
            new CreateIndexOptions { Name = "Action" }),
        new(Builders<AuditEntryEntity>.IndexKeys.Ascending(x => x.CallerIdentity),
            new CreateIndexOptions { Name = "CallerIdentity" }),
        new(Builders<AuditEntryEntity>.IndexKeys.Ascending(x => x.CallerSource),
            new CreateIndexOptions { Name = "CallerSource" }),
        new(Builders<AuditEntryEntity>.IndexKeys.Ascending(x => x.EventType),
            new CreateIndexOptions { Name = "EventType" }),
        new(Builders<AuditEntryEntity>.IndexKeys.Ascending(x => x.ScopeChecked),
            new CreateIndexOptions { Name = "ScopeChecked" }),
    ];
}
