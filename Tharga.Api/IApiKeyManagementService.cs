namespace Tharga.Api;

/// <summary>
/// User-facing service for API key management. All methods require the apikey:manage scope.
/// </summary>
public interface IApiKeyManagementService
{
    [RequireScope(ApiKeyScopes.Manage)]
    IAsyncEnumerable<IApiKey> GetKeysAsync(string teamKey);

    [RequireScope(ApiKeyScopes.Manage)]
    Task<IApiKey> CreateKeyAsync(string teamKey, string name, AccessLevel accessLevel, string[] roles = null, DateTime? expiryDate = null);

    [RequireScope(ApiKeyScopes.Manage)]
    Task<IApiKey> RefreshKeyAsync(string teamKey, string key);

    [RequireScope(ApiKeyScopes.Manage)]
    Task LockKeyAsync(string key);

    [RequireScope(ApiKeyScopes.Manage)]
    Task DeleteKeyAsync(string key);
}
