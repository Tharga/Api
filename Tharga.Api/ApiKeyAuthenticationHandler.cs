using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tharga.Api;

/// <summary>
/// Authentication handler that validates API keys from the X-API-KEY header.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IApiKeyAdministrationService _apiKeyAdministrationService;
    private readonly IScopeRegistry _scopeRegistry;

    /// <summary>
    /// Creates a new instance of the API key authentication handler.
    /// </summary>
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyAdministrationService apiKeyAdministrationService,
        IScopeRegistry scopeRegistry = null)
        : base(options, logger, encoder)
    {
        _apiKeyAdministrationService = apiKeyAdministrationService;
        _scopeRegistry = scopeRegistry;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyConstants.HeaderName, out var apiKeyHeader))
            return AuthenticateResult.NoResult();

        var apiKey = apiKeyHeader.ToString();
        if (string.IsNullOrWhiteSpace(apiKey))
            return AuthenticateResult.NoResult();

        var key = await _apiKeyAdministrationService.GetByApiKeyAsync(apiKey);
        if (key == null)
            return AuthenticateResult.Fail("Invalid API key.");

        var accessLevelStr = key.Tags.TryGetValue(TeamClaimTypes.AccessLevel, out var level)
            ? level
            : AccessLevel.Administrator.ToString();

        var claims = new List<Claim>
        {
            new(TeamClaimTypes.TeamKey, key.TeamKey),
            new(ClaimTypes.Name, key.Name ?? key.TeamKey),
            new(TeamClaimTypes.AccessLevel, accessLevelStr),
        };

        if (_scopeRegistry != null && Enum.TryParse<AccessLevel>(accessLevelStr, out var accessLevel))
        {
            var roleNames = key.Tags.TryGetValue("TenantRoles", out var roles)
                ? roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                : Array.Empty<string>();

            foreach (var scope in _scopeRegistry.GetEffectiveScopes(accessLevel, roleNames))
            {
                claims.Add(new Claim(TeamClaimTypes.Scope, scope));
            }
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
