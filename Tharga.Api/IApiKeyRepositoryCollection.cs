using Tharga.MongoDB;

namespace Tharga.Api;

/// <summary>
/// Collection interface for disk-backed API key storage. Auto-registered by Tharga.MongoDB.
/// </summary>
public interface IApiKeyRepositoryCollection : IDiskRepositoryCollection<ApiKeyEntity>;
