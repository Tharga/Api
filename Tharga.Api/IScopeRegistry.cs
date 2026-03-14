namespace Tharga.Api;

/// <summary>
/// Registry of all known scopes. Used at runtime to resolve effective scopes for an access level.
/// </summary>
public interface IScopeRegistry
{
    IReadOnlyList<ScopeDefinition> All { get; }
    IReadOnlyList<string> GetScopesForAccessLevel(AccessLevel accessLevel);
}
