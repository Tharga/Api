using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Tharga.MongoDB;
using Tharga.MongoDB.Disk;

namespace Tharga.Api;

internal class ApiKeyRepositoryCollection : DiskRepositoryCollectionBase<ApiKeyEntity>, IApiKeyRepositoryCollection
{
    public ApiKeyRepositoryCollection(IMongoDbServiceFactory mongoDbServiceFactory, ILogger<ApiKeyRepositoryCollection> logger)
        : base(mongoDbServiceFactory, logger)
    {
    }

    public override string CollectionName => "ApiKey";

    public override IEnumerable<CreateIndexModel<ApiKeyEntity>> Indices =>
    [
        new(Builders<ApiKeyEntity>.IndexKeys.Ascending(x => x.Key), new CreateIndexOptions { Unique = true, Name = nameof(ApiKeyEntity.Key) }),
        new(Builders<ApiKeyEntity>.IndexKeys.Ascending(x => x.ApiKeyHash), new CreateIndexOptions { Unique = true, Name = nameof(ApiKeyEntity.ApiKeyHash) })
    ];
}
