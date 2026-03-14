using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Tharga.Api.Tests;

public interface IScopedTestService
{
    [RequireScope("doc:read")]
    string ReadMethod();

    [RequireScope("doc:download")]
    string DownloadMethod();

    [RequireScope("doc:delete")]
    string DeleteMethod();

    string UnprotectedMethod();
}

public class ScopeProxyTests
{
    private static IHttpContextAccessor CreateAccessor(string teamKey, params string[] scopes)
    {
        var claims = new List<Claim>();
        if (teamKey != null) claims.Add(new Claim(TeamClaimTypes.TeamKey, teamKey));
        foreach (var scope in scopes)
            claims.Add(new Claim(TeamClaimTypes.Scope, scope));

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);
        return accessor;
    }

    private static IScopedTestService CreateProxy(string teamKey, params string[] scopes)
    {
        var target = Substitute.For<IScopedTestService>();
        target.ReadMethod().Returns("read-ok");
        target.DownloadMethod().Returns("download-ok");
        target.DeleteMethod().Returns("delete-ok");
        target.UnprotectedMethod().Returns("unprotected-ok");

        var accessor = CreateAccessor(teamKey, scopes);
        return ScopeProxy<IScopedTestService>.Create(target, accessor);
    }

    [Fact]
    public void With_Required_Scope_Succeeds()
    {
        var proxy = CreateProxy("team-1", "doc:read");
        Assert.Equal("read-ok", proxy.ReadMethod());
    }

    [Fact]
    public void With_Multiple_Scopes_Succeeds()
    {
        var proxy = CreateProxy("team-1", "doc:read", "doc:download", "doc:delete");
        Assert.Equal("read-ok", proxy.ReadMethod());
        Assert.Equal("download-ok", proxy.DownloadMethod());
        Assert.Equal("delete-ok", proxy.DeleteMethod());
    }

    [Fact]
    public void Without_Required_Scope_Throws()
    {
        var proxy = CreateProxy("team-1", "doc:read");
        Assert.Throws<UnauthorizedAccessException>(() => proxy.DownloadMethod());
    }

    [Fact]
    public void Without_Any_Scopes_Throws()
    {
        var proxy = CreateProxy("team-1");
        Assert.Throws<UnauthorizedAccessException>(() => proxy.ReadMethod());
    }

    [Fact]
    public void Without_TeamKey_Throws()
    {
        var proxy = CreateProxy(null, "doc:read");
        Assert.Throws<UnauthorizedAccessException>(() => proxy.ReadMethod());
    }

    [Fact]
    public void Missing_Attribute_Throws_InvalidOperation()
    {
        var proxy = CreateProxy("team-1", "doc:read");
        Assert.Throws<InvalidOperationException>(() => proxy.UnprotectedMethod());
    }
}
