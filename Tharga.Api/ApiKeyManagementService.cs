namespace Tharga.Api;

/// <summary>
/// Delegates to <see cref="IApiKeyAdministrationService"/> for all operations.
/// Scope enforcement is handled by <see cref="ScopeProxy{T}"/>.
/// </summary>
public class ApiKeyManagementService : IApiKeyManagementService
{
    private readonly IApiKeyAdministrationService _inner;

    public ApiKeyManagementService(IApiKeyAdministrationService inner)
    {
        _inner = inner;
    }

    public IAsyncEnumerable<IApiKey> GetKeysAsync(string teamKey) => _inner.GetKeysAsync(teamKey);
    public Task<IApiKey> CreateKeyAsync(string teamKey, string name, AccessLevel accessLevel, string[] roles = null, DateTime? expiryDate = null) => _inner.CreateKeyAsync(teamKey, name, accessLevel, roles, expiryDate);
    public Task<IApiKey> RefreshKeyAsync(string teamKey, string key) => _inner.RefreshKeyAsync(teamKey, key);
    public Task LockKeyAsync(string key) => _inner.LockKeyAsync(key);
    public Task DeleteKeyAsync(string key) => _inner.DeleteKeyAsync(key);
}
