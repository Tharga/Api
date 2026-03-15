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

        var (accessLevel, roleNames, scopeOverrides) = ResolveKeyDetails(key);

        var claims = new List<Claim>
        {
            new(TeamClaimTypes.TeamKey, key.TeamKey),
            new(ClaimTypes.Name, key.Name ?? key.TeamKey),
            new(TeamClaimTypes.AccessLevel, accessLevel.ToString()),
        };

        if (_scopeRegistry != null)
        {
            foreach (var scope in _scopeRegistry.GetEffectiveScopes(accessLevel, roleNames, scopeOverrides))
            {
                claims.Add(new Claim(TeamClaimTypes.Scope, scope));
            }
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    private static (AccessLevel accessLevel, string[] roleNames, string[] scopeOverrides) ResolveKeyDetails(IApiKey key)
    {
        if (key is ApiKeyEntity entity)
        {
            var al = entity.AccessLevel ?? AccessLevel.Administrator;
            var roles = entity.Roles ?? Array.Empty<string>();
            var overrides = entity.ScopeOverrides ?? Array.Empty<string>();
            return (al, roles, overrides);
        }

        var accessLevelStr = key.Tags.TryGetValue(TeamClaimTypes.AccessLevel, out var level)
            ? level
            : AccessLevel.Viewer.ToString();

        var accessLevel = Enum.TryParse<AccessLevel>(accessLevelStr, out var parsed)
            ? parsed
            : AccessLevel.Viewer;

        var roleStr = key.Tags.TryGetValue("TenantRoles", out var r)
            ? r.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : Array.Empty<string>();

        return (accessLevel, roleStr, Array.Empty<string>());
    }
}
