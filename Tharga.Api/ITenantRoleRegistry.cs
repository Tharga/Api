namespace Tharga.Api;

/// <summary>
/// Registry of tenant role definitions. Used at runtime to resolve scopes for assigned roles.
/// </summary>
public interface ITenantRoleRegistry
{
    IReadOnlyList<TenantRoleDefinition> All { get; }
    TenantRoleDefinition Get(string roleName);
    IReadOnlyList<string> GetScopesForRoles(IEnumerable<string> roleNames);
}
