namespace Tharga.Api;

/// <summary>
/// Defines a scope with its default minimum access level.
/// </summary>
public record ScopeDefinition(string Name, AccessLevel DefaultMinimumLevel);
