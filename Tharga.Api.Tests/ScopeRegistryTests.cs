namespace Tharga.Api.Tests;

public class ScopeRegistryTests
{
    [Fact]
    public void Owner_Gets_All_Scopes()
    {
        var registry = new ScopeRegistry();
        registry.Register("doc:read", AccessLevel.Viewer);
        registry.Register("doc:delete", AccessLevel.Administrator);

        var scopes = registry.GetScopesForAccessLevel(AccessLevel.Owner);

        Assert.Equal(2, scopes.Count);
        Assert.Contains("doc:read", scopes);
        Assert.Contains("doc:delete", scopes);
    }

    [Fact]
    public void Administrator_Gets_All_Scopes()
    {
        var registry = new ScopeRegistry();
        registry.Register("doc:read", AccessLevel.Viewer);
        registry.Register("doc:delete", AccessLevel.Administrator);

        var scopes = registry.GetScopesForAccessLevel(AccessLevel.Administrator);

        Assert.Equal(2, scopes.Count);
    }

    [Fact]
    public void User_Gets_User_And_Viewer_Scopes()
    {
        var registry = new ScopeRegistry();
        registry.Register("doc:read", AccessLevel.Viewer);
        registry.Register("doc:download", AccessLevel.User);
        registry.Register("doc:delete", AccessLevel.Administrator);

        var scopes = registry.GetScopesForAccessLevel(AccessLevel.User);

        Assert.Equal(2, scopes.Count);
        Assert.Contains("doc:read", scopes);
        Assert.Contains("doc:download", scopes);
        Assert.DoesNotContain("doc:delete", scopes);
    }

    [Fact]
    public void Viewer_Gets_Only_Viewer_Scopes()
    {
        var registry = new ScopeRegistry();
        registry.Register("doc:read", AccessLevel.Viewer);
        registry.Register("doc:download", AccessLevel.User);
        registry.Register("doc:delete", AccessLevel.Administrator);

        var scopes = registry.GetScopesForAccessLevel(AccessLevel.Viewer);

        Assert.Single(scopes);
        Assert.Contains("doc:read", scopes);
    }

    [Fact]
    public void Duplicate_Registration_Throws()
    {
        var registry = new ScopeRegistry();
        registry.Register("doc:read", AccessLevel.Viewer);

        Assert.Throws<InvalidOperationException>(() => registry.Register("doc:read", AccessLevel.User));
    }
}
