namespace Tharga.Api;

/// <summary>
/// Represents an API key with associated metadata.
/// </summary>
public interface IApiKey
{
    /// <summary>Unique identifier for this API key entry.</summary>
    string Key { get; }

    /// <summary>Human-readable name for this API key.</summary>
    string Name { get; }

    /// <summary>The raw API key value (only populated on creation; otherwise empty).</summary>
    string ApiKey { get; }

    /// <summary>Team that owns this API key.</summary>
    string TeamKey { get; }

    /// <summary>Arbitrary key-value metadata associated with this API key.</summary>
    Dictionary<string, string> Tags { get; }
}
