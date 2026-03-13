using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Tharga.Api;

/// <summary>
/// Extension methods for registering API key authentication.
/// </summary>
public static class ApiKeyRegistration
{
    /// <summary>
    /// Adds API key authentication with the default <see cref="ApiKeyAdministrationService"/>.
    /// </summary>
    public static AuthenticationBuilder AddThargaApiKeyAuthentication(this AuthenticationBuilder builder)
    {
        return builder.AddThargaApiKeyAuthentication<ApiKeyAdministrationService>();
    }

    /// <summary>
    /// Adds API key authentication with a custom <see cref="IApiKeyAdministrationService"/> implementation.
    /// </summary>
    public static AuthenticationBuilder AddThargaApiKeyAuthentication<TService>(this AuthenticationBuilder builder)
        where TService : class, IApiKeyAdministrationService
    {
        builder.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
            ApiKeyConstants.SchemeName, "API Key", null);

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(ApiKeyConstants.PolicyName, policy =>
            {
                policy.AddAuthenticationSchemes(ApiKeyConstants.SchemeName);
                policy.RequireAuthenticatedUser();
            });
        });

        builder.Services.AddScoped<IApiKeyAdministrationService, TService>();

        return builder;
    }
}
