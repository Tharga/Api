using Tharga.Api.Audit;

namespace Tharga.Api.Tests.Audit;

public class AuditEntryTests
{
    [Theory]
    [InlineData("source:read", "source", "read")]
    [InlineData("document:delete", "document", "delete")]
    [InlineData("apikey:manage", "apikey", "manage")]
    public void ParseScope_Splits_Feature_And_Action(string scope, string expectedFeature, string expectedAction)
    {
        var (feature, action) = AuditEntry.ParseScope(scope);

        Assert.Equal(expectedFeature, feature);
        Assert.Equal(expectedAction, action);
    }

    [Fact]
    public void ParseScope_Single_Part_Returns_Feature_Only()
    {
        var (feature, action) = AuditEntry.ParseScope("auth");

        Assert.Equal("auth", feature);
        Assert.Null(action);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ParseScope_Null_Or_Empty_Returns_Nulls(string scope)
    {
        var (feature, action) = AuditEntry.ParseScope(scope);

        Assert.Null(feature);
        Assert.Null(action);
    }

    [Fact]
    public void ParseScope_With_Multiple_Colons_Keeps_Rest_In_Action()
    {
        var (feature, action) = AuditEntry.ParseScope("team:role:assign");

        Assert.Equal("team", feature);
        Assert.Equal("role:assign", action);
    }
}
