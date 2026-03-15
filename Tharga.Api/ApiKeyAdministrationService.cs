using MongoDB.Bson;
using Microsoft.Extensions.Options;
using Tharga.Toolkit;
using Tharga.Toolkit.Password;

namespace Tharga.Api;

/// <summary>
/// Default implementation of <see cref="IApiKeyAdministrationService"/> using MongoDB storage.
/// </summary>
public class ApiKeyAdministrationService : IApiKeyAdministrationService
{
    private readonly IApiKeyRepository _repository;
    private readonly IApiKeyService _apiKeyService;
    private readonly ApiKeyOptions _options;

    /// <summary>
    /// Creates a new instance using the specified repository and key hashing service.
    /// </summary>
    public ApiKeyAdministrationService(IApiKeyRepository repository, IApiKeyService apiKeyService, IOptions<ApiKeyOptions> options = null)
    {
        _repository = repository;
        _apiKeyService = apiKeyService;
        _options = options?.Value ?? new ApiKeyOptions();
    }

    /// <inheritdoc />
    public async Task<IApiKey> GetByApiKeyAsync(string apiKey)
    {
        var items = await _repository.GetAsync().ToArrayAsync();
        var item = items.SingleOrDefault(x => _apiKeyService.Verify(apiKey, x.ApiKeyHash));

        if (item?.ExpiryDate != null && item.ExpiryDate < DateTime.UtcNow)
            return null;

        return item;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IApiKey> GetKeysAsync(string teamKey)
    {
        var count = 0;
        await foreach (var item in _repository.GetAsync())
        {
            count++;
            yield return item;
        }

        if (_options.AdvancedMode) yield break;

        for (var i = count; i < _options.AutoKeyCount; i++)
        {
            var name = IntegerExtensions.GetNameForNumber(i + 1);
            var expiryDate = GetDefaultExpiryDate();
            var entity = BuildKey(teamKey, name, [], AccessLevel.User, null, expiryDate);
            var created = await _repository.AddAsync(entity);

            if (_options.AutoLockKeys)
            {
                await _repository.LockKeyAsync(created.Key);
                created = created with { ApiKey = null };
            }

            yield return created;
        }
    }

    /// <inheritdoc />
    public async Task<IApiKey> CreateKeyAsync(string teamKey, string name, AccessLevel accessLevel, string[] roles = null, DateTime? expiryDate = null)
    {
        expiryDate ??= GetDefaultExpiryDate();

        if (_options.MaxExpiryDays.HasValue && expiryDate.HasValue)
        {
            var maxDate = DateTime.UtcNow.AddDays(_options.MaxExpiryDays.Value);
            if (expiryDate > maxDate)
                throw new InvalidOperationException($"Expiry date cannot exceed {_options.MaxExpiryDays} days from now.");
        }

        var entity = BuildKey(teamKey, name, new Dictionary<string, string>(), accessLevel, roles, expiryDate);
        var created = await _repository.AddAsync(entity);

        if (_options.AutoLockKeys)
            await _repository.LockKeyAsync(created.Key);

        return created;
    }

    /// <inheritdoc />
    public async Task<IApiKey> RefreshKeyAsync(string teamKey, string key)
    {
        var item = await _repository.GetAsync(key);
        var refreshed = BuildKey(teamKey, item.Name, item.Tags, item.AccessLevel ?? AccessLevel.Administrator, item.Roles, item.ExpiryDate);
        await _repository.UpdateAsync(key, refreshed);

        if (_options.AutoLockKeys)
            await _repository.LockKeyAsync(key);

        return refreshed;
    }

    /// <inheritdoc />
    public Task LockKeyAsync(string key)
    {
        return _repository.LockKeyAsync(key);
    }

    /// <inheritdoc />
    public Task DeleteKeyAsync(string key)
    {
        return _repository.DeleteAsync(key);
    }

    private DateTime? GetDefaultExpiryDate()
    {
        return _options.MaxExpiryDays.HasValue
            ? DateTime.UtcNow.AddDays(_options.MaxExpiryDays.Value)
            : null;
    }

    private ApiKeyEntity BuildKey(string teamKey, string name, Dictionary<string, string> tags, AccessLevel accessLevel, string[] roles, DateTime? expiryDate)
    {
        var apiKey = _apiKeyService.BuildApiKey(teamKey, () => StringExtension.GetRandomString(24, 32));
        var encryptedApiKey = _apiKeyService.Encrypt(apiKey);
        return new ApiKeyEntity
        {
            Id = ObjectId.GenerateNewId(),
            Key = Guid.NewGuid().ToString(),
            Name = name,
            ApiKey = apiKey,
            TeamKey = teamKey,
            Tags = tags,
            ApiKeyHash = encryptedApiKey,
            AccessLevel = accessLevel,
            Roles = roles,
            ExpiryDate = expiryDate,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
