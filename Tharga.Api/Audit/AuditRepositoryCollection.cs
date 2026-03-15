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
        new(Builders<AuditEntryEntity>.IndexKeys.Ascending(x => x.Timestamp),
            new CreateIndexOptions { Name = "Timestamp", ExpireAfter = TimeSpan.FromDays(_retentionDays) }),
        new(Builders<AuditEntryEntity>.IndexKeys.Ascending(x => x.TeamKey),
            new CreateIndexOptions { Name = "TeamKey" }),
        new(Builders<AuditEntryEntity>.IndexKeys.Ascending(x => x.Feature),
            new CreateIndexOptions { Name = "Feature" }),
        new(Builders<AuditEntryEntity>.IndexKeys.Ascending(x => x.CallerIdentity),
            new CreateIndexOptions { Name = "CallerIdentity" })
    ];
}
