using MongoDB.Bson.Serialization.Attributes;
using Tharga.MongoDB;

namespace Tharga.Api;

/// <summary>
/// Default MongoDB entity for API keys.
/// </summary>
public record ApiKeyEntity : EntityBase, IApiKey
{
    public required string Key { get; init; }
    public required string Name { get; init; }

    [BsonIgnoreIfDefault]
    public string ApiKey { get; init; }

    [BsonIgnoreIfDefault]
    public string TeamKey { get; init; }

    [BsonIgnoreIfDefault]
    public Dictionary<string, string> Tags { get; init; } = new();

    public required string ApiKeyHash { get; init; }
}
