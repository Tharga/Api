namespace Tharga.Api;

/// <summary>
/// Stores scope definitions and resolves effective scopes for a given access level.
/// Owner and Administrator get all registered scopes.
/// User gets scopes registered at User or Viewer level.
/// Viewer gets only scopes registered at Viewer level.
/// </summary>
public class ScopeRegistry : IScopeRegistry
{
    private readonly List<ScopeDefinition> _scopes = new();

    public IReadOnlyList<ScopeDefinition> All => _scopes;

    public void Register(string scopeName, AccessLevel defaultMinimumLevel)
    {
        if (_scopes.Any(s => s.Name == scopeName))
            throw new InvalidOperationException($"Scope '{scopeName}' is already registered.");

        _scopes.Add(new ScopeDefinition(scopeName, defaultMinimumLevel));
    }

    public IReadOnlyList<string> GetScopesForAccessLevel(AccessLevel accessLevel)
    {
        if (accessLevel <= AccessLevel.Administrator)
            return _scopes.Select(s => s.Name).ToList();

        return _scopes
            .Where(s => s.DefaultMinimumLevel >= accessLevel)
            .Select(s => s.Name)
            .ToList();
    }
}
