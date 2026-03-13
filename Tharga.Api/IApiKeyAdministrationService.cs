namespace Tharga.Api;

/// <summary>
/// Service for managing and validating API keys.
/// </summary>
public interface IApiKeyAdministrationService
{
    /// <summary>Looks up an API key by its raw value. Returns <c>null</c> if no match is found.</summary>
    Task<IApiKey> GetByApiKeyAsync(string apiKey);

    /// <summary>Returns all API keys for the specified team, creating default keys if fewer than two exist.</summary>
    IAsyncEnumerable<IApiKey> GetKeysAsync(string teamKey);

    /// <summary>Generates a new API key value for an existing key entry.</summary>
    Task RefreshKeyAsync(string teamKey, string key);

    /// <summary>Locks an API key so it can no longer be used for authentication.</summary>
    Task LockKeyAsync(string key);
}
