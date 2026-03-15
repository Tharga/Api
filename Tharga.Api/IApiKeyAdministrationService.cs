namespace Tharga.Api;

/// <summary>
/// Service for managing and validating API keys.
/// </summary>
public interface IApiKeyAdministrationService
{
    /// <summary>Looks up an API key by its raw value. Returns <c>null</c> if no match is found.</summary>
    Task<IApiKey> GetByApiKeyAsync(string apiKey);

    /// <summary>Returns all API keys for the specified team, creating default keys if fewer than AutoKeyCount exist.</summary>
    IAsyncEnumerable<IApiKey> GetKeysAsync(string teamKey);

    /// <summary>Creates a new API key with the specified settings (advanced mode).</summary>
    Task<IApiKey> CreateKeyAsync(string teamKey, string name, AccessLevel accessLevel, string[] roles = null, DateTime? expiryDate = null);

    /// <summary>Generates a new API key value for an existing key entry. Returns the entity with the raw key visible once.</summary>
    Task<IApiKey> RefreshKeyAsync(string teamKey, string key);

    /// <summary>Locks an API key so it can no longer be used for authentication. Verifies team ownership.</summary>
    Task LockKeyAsync(string teamKey, string key);

    /// <summary>Deletes an API key. Verifies team ownership.</summary>
    Task DeleteKeyAsync(string teamKey, string key);
}
