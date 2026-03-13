using Tharga.MongoDB;

namespace Tharga.Api;

/// <summary>
/// Repository interface for API key persistence. Auto-registered by Tharga.MongoDB.
/// </summary>
public interface IApiKeyRepository : IRepository
{
    IAsyncEnumerable<ApiKeyEntity> GetAsync();
    Task<ApiKeyEntity> GetAsync(string key);
    Task<ApiKeyEntity> AddAsync(ApiKeyEntity apiKeyEntity);
    Task UpdateAsync(string key, ApiKeyEntity apiKeyEntity);
    Task LockKeyAsync(string key);
}
