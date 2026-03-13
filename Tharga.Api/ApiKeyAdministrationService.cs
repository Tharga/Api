using MongoDB.Bson;
using Tharga.Blazor.Framework;
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

    public ApiKeyAdministrationService(IApiKeyRepository repository, IApiKeyService apiKeyService)
    {
        _repository = repository;
        _apiKeyService = apiKeyService;
    }

    public async Task<IApiKey> GetByApiKeyAsync(string apiKey)
    {
        var items = await _repository.GetAsync().ToArrayAsync();
        var item = items.SingleOrDefault(x => _apiKeyService.Verify(apiKey, x.ApiKeyHash));
        return item!;
    }

    public async IAsyncEnumerable<IApiKey> GetKeysAsync(string teamKey)
    {
        var count = 0;
        await foreach (var item in _repository.GetAsync())
        {
            count++;
            yield return item;
        }

        for (var i = count; i < 2; i++)
        {
            var name = IntegerExtensions.GetNameForNumber(i + 1);
            yield return await _repository.AddAsync(BuildKey(teamKey, name, []));
        }
    }

    public async Task RefreshKeyAsync(string teamKey, string key)
    {
        var item = await _repository.GetAsync(key);
        await _repository.UpdateAsync(key, BuildKey(teamKey, item.Name, item.Tags));
    }

    public Task LockKeyAsync(string key)
    {
        return _repository.LockKeyAsync(key);
    }

    private ApiKeyEntity BuildKey(string teamKey, string name, Dictionary<string, string> tags)
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
        };
    }
}
