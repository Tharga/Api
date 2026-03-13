namespace Tharga.Api;

/// <summary>
/// Represents an API key with associated metadata.
/// </summary>
public interface IApiKey
{
    string Key { get; }
    string Name { get; }
    string ApiKey { get; }
    string TeamKey { get; }
    Dictionary<string, string> Tags { get; }
}
