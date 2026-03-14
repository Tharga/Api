namespace Tharga.Api;

/// <summary>
/// Defines a tenant role with its associated scopes.
/// </summary>
public record TenantRoleDefinition(string Name, IReadOnlyList<string> Scopes);
