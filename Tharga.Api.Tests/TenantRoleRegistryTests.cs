namespace Tharga.Api.Tests;

public class TenantRoleRegistryTests
{
    [Fact]
    public void Register_And_Get_Role()
    {
        var registry = new TenantRoleRegistry();
        registry.Register("TeamDeveloper", "apikey:manage", "config:read");

        var role = registry.Get("TeamDeveloper");

        Assert.Equal("TeamDeveloper", role.Name);
        Assert.Equal(2, role.Scopes.Count);
        Assert.Contains("apikey:manage", role.Scopes);
        Assert.Contains("config:read", role.Scopes);
    }

    [Fact]
    public void Duplicate_Registration_Throws()
    {
        var registry = new TenantRoleRegistry();
        registry.Register("TeamDeveloper", "apikey:manage");

        Assert.Throws<InvalidOperationException>(() => registry.Register("TeamDeveloper", "other:scope"));
    }

    [Fact]
    public void Get_Unknown_Role_Throws()
    {
        var registry = new TenantRoleRegistry();

        Assert.Throws<KeyNotFoundException>(() => registry.Get("Unknown"));
    }

    [Fact]
    public void GetScopesForRoles_Returns_Union()
    {
        var registry = new TenantRoleRegistry();
        registry.Register("RoleA", "scope:a", "scope:shared");
        registry.Register("RoleB", "scope:b", "scope:shared");

        var scopes = registry.GetScopesForRoles(new[] { "RoleA", "RoleB" });

        Assert.Equal(3, scopes.Count);
        Assert.Contains("scope:a", scopes);
        Assert.Contains("scope:b", scopes);
        Assert.Contains("scope:shared", scopes);
    }

    [Fact]
    public void GetScopesForRoles_Ignores_Unknown_Roles()
    {
        var registry = new TenantRoleRegistry();
        registry.Register("RoleA", "scope:a");

        var scopes = registry.GetScopesForRoles(new[] { "RoleA", "Unknown" });

        Assert.Single(scopes);
        Assert.Contains("scope:a", scopes);
    }

    [Fact]
    public void GetScopesForRoles_With_Null_Returns_Empty()
    {
        var registry = new TenantRoleRegistry();

        var scopes = registry.GetScopesForRoles(null);

        Assert.Empty(scopes);
    }
}

public class ScopeRegistryWithRolesTests
{
    [Fact]
    public void GetEffectiveScopes_Combines_AccessLevel_And_Roles()
    {
        var roleRegistry = new TenantRoleRegistry();
        roleRegistry.Register("TeamDeveloper", "apikey:manage");

        var scopeRegistry = new ScopeRegistry();
        scopeRegistry.Register("doc:read", AccessLevel.Viewer);
        scopeRegistry.Register("doc:download", AccessLevel.User);
        scopeRegistry.SetRoleRegistry(roleRegistry);

        var scopes = scopeRegistry.GetEffectiveScopes(AccessLevel.User, new[] { "TeamDeveloper" });

        Assert.Equal(3, scopes.Count);
        Assert.Contains("doc:read", scopes);
        Assert.Contains("doc:download", scopes);
        Assert.Contains("apikey:manage", scopes);
    }

    [Fact]
    public void GetEffectiveScopes_Viewer_With_Role_Gets_Role_Scopes()
    {
        var roleRegistry = new TenantRoleRegistry();
        roleRegistry.Register("TeamDeveloper", "apikey:manage");

        var scopeRegistry = new ScopeRegistry();
        scopeRegistry.Register("doc:read", AccessLevel.Viewer);
        scopeRegistry.Register("doc:download", AccessLevel.User);
        scopeRegistry.SetRoleRegistry(roleRegistry);

        var scopes = scopeRegistry.GetEffectiveScopes(AccessLevel.Viewer, new[] { "TeamDeveloper" });

        Assert.Equal(2, scopes.Count);
        Assert.Contains("doc:read", scopes);
        Assert.Contains("apikey:manage", scopes);
        Assert.DoesNotContain("doc:download", scopes);
    }

    [Fact]
    public void GetEffectiveScopes_Without_Roles_Returns_AccessLevel_Only()
    {
        var scopeRegistry = new ScopeRegistry();
        scopeRegistry.Register("doc:read", AccessLevel.Viewer);

        var scopes = scopeRegistry.GetEffectiveScopes(AccessLevel.Viewer, null);

        Assert.Single(scopes);
        Assert.Contains("doc:read", scopes);
    }

    [Fact]
    public void GetEffectiveScopes_Admin_Gets_All_Plus_Roles()
    {
        var roleRegistry = new TenantRoleRegistry();
        roleRegistry.Register("TeamDeveloper", "apikey:manage");

        var scopeRegistry = new ScopeRegistry();
        scopeRegistry.Register("doc:read", AccessLevel.Viewer);
        scopeRegistry.SetRoleRegistry(roleRegistry);

        var scopes = scopeRegistry.GetEffectiveScopes(AccessLevel.Administrator, new[] { "TeamDeveloper" });

        Assert.Equal(2, scopes.Count);
        Assert.Contains("doc:read", scopes);
        Assert.Contains("apikey:manage", scopes);
    }
}
