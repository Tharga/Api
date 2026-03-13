namespace Tharga.Api;

/// <summary>
/// Options for configuring Tharga controller registration.
/// </summary>
public class ThargaControllerOptions
{
    /// <summary>
    /// Title shown in Swagger UI. Defaults to "API v1".
    /// </summary>
    public string SwaggerTitle { get; set; } = "API v1";

    /// <summary>
    /// Swagger UI route prefix. Defaults to "swagger".
    /// </summary>
    public string SwaggerRoutePrefix { get; set; } = "swagger";
}
