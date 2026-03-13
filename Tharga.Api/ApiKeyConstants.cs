namespace Tharga.Api;

/// <summary>
/// Well-known constants for API key authentication.
/// </summary>
public static class ApiKeyConstants
{
    /// <summary>HTTP header name for the API key.</summary>
    public const string HeaderName = "X-API-KEY";

    /// <summary>Authentication scheme name.</summary>
    public const string SchemeName = "ApiKeyScheme";

    /// <summary>Authorization policy name. Use with [Authorize(Policy = ApiKeyConstants.PolicyName)].</summary>
    public const string PolicyName = "ApiKeyPolicy";

    /// <summary>Claim type for the team key.</summary>
    public const string TeamKeyClaim = "TeamKey";

    /// <summary>OpenAPI security scheme identifier.</summary>
    public const string OpenApiSchemeId = "ApiKey";
}
