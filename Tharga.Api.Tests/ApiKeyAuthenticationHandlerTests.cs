using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tharga.Api.Tests;

public class ApiKeyAuthenticationHandlerTests
{
    private readonly IApiKeyAdministrationService _apiKeyService = Substitute.For<IApiKeyAdministrationService>();

    private async Task<ApiKeyAuthenticationHandler> CreateHandler(HttpContext httpContext)
    {
        var options = new AuthenticationSchemeOptions();
        var optionsMonitor = Substitute.For<IOptionsMonitor<AuthenticationSchemeOptions>>();
        optionsMonitor.Get(ApiKeyConstants.SchemeName).Returns(options);

        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());

        var handler = new ApiKeyAuthenticationHandler(
            optionsMonitor,
            loggerFactory,
            UrlEncoder.Default,
            _apiKeyService);

        var scheme = new AuthenticationScheme(ApiKeyConstants.SchemeName, "API Key", typeof(ApiKeyAuthenticationHandler));
        await handler.InitializeAsync(scheme, httpContext);

        return handler;
    }

    private static HttpContext CreateHttpContext(string apiKeyHeaderValue = null)
    {
        var context = new DefaultHttpContext();
        if (apiKeyHeaderValue != null)
        {
            context.Request.Headers[ApiKeyConstants.HeaderName] = apiKeyHeaderValue;
        }
        return context;
    }

    [Fact]
    public async Task Without_Header_Returns_NoResult()
    {
        var context = CreateHttpContext();
        var handler = await CreateHandler(context);

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.True(result.None);
    }

    [Fact]
    public async Task With_Empty_Header_Returns_NoResult()
    {
        var context = CreateHttpContext("");
        var handler = await CreateHandler(context);

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.True(result.None);
    }

    [Fact]
    public async Task With_Whitespace_Header_Returns_NoResult()
    {
        var context = CreateHttpContext("   ");
        var handler = await CreateHandler(context);

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.True(result.None);
    }

    [Fact]
    public async Task With_Invalid_ApiKey_Returns_Fail()
    {
        _apiKeyService.GetByApiKeyAsync("invalid-key").Returns(Task.FromResult<IApiKey>(null));

        var context = CreateHttpContext("invalid-key");
        var handler = await CreateHandler(context);

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.False(result.None);
        Assert.Contains("Invalid", result.Failure.Message);
    }

    [Fact]
    public async Task With_Valid_ApiKey_Returns_Success_With_Claims()
    {
        var apiKey = Substitute.For<IApiKey>();
        apiKey.TeamKey.Returns("team-123");
        apiKey.Name.Returns("Test Key");
        _apiKeyService.GetByApiKeyAsync("valid-key").Returns(Task.FromResult(apiKey));

        var context = CreateHttpContext("valid-key");
        var handler = await CreateHandler(context);

        var result = await handler.AuthenticateAsync();

        Assert.True(result.Succeeded);
        var teamKeyClaim = result.Principal.FindFirst(ApiKeyConstants.TeamKeyClaim);
        Assert.NotNull(teamKeyClaim);
        Assert.Equal("team-123", teamKeyClaim.Value);

        var nameClaim = result.Principal.FindFirst(ClaimTypes.Name);
        Assert.NotNull(nameClaim);
        Assert.Equal("Test Key", nameClaim.Value);
    }

    [Fact]
    public async Task With_Valid_ApiKey_And_Null_Name_Uses_TeamKey_As_Name()
    {
        var apiKey = Substitute.For<IApiKey>();
        apiKey.TeamKey.Returns("team-456");
        apiKey.Name.Returns((string)null);
        _apiKeyService.GetByApiKeyAsync("valid-key").Returns(Task.FromResult(apiKey));

        var context = CreateHttpContext("valid-key");
        var handler = await CreateHandler(context);

        var result = await handler.AuthenticateAsync();

        Assert.True(result.Succeeded);
        var nameClaim = result.Principal.FindFirst(ClaimTypes.Name);
        Assert.NotNull(nameClaim);
        Assert.Equal("team-456", nameClaim.Value);
    }
}
