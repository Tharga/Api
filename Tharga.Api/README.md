# Tharga.Api
Reusable API-key authentication, authorization, controller registration, and OpenAPI/Swagger setup for ASP.NET Core projects. Targets .NET 9.0 and .NET 10.0.

[![GitHub repo](https://img.shields.io/github/repo-size/Tharga/Api?style=flat&logo=github&logoColor=red&label=Repo)](https://github.com/Tharga/Api)

## Features

- **API key authentication** – Reads the `X-API-KEY` header, validates against a store, and populates `TeamKey`, `Name`, and `AccessLevel` claims.
- **Access level authorization** – `[RequireAccessLevel]` attribute with `AccessLevelProxy<T>` for declarative service-level authorization.
- **Controller + Swagger registration** – Single-call setup for MVC controllers, OpenAPI document with API key security scheme, and Swagger UI.
- **Built-in MongoDB storage** – Default `ApiKeyAdministrationService` backed by Tharga.MongoDB with key hashing.
- **Pluggable** – Implement `IApiKeyAdministrationService` to bring your own storage backend.

## Quick start

```csharp
// Program.cs
builder.Services.AddThargaControllers();
builder.Services.AddAuthentication()
    .AddThargaApiKeyAuthentication();
builder.Services.AddThargaApiKeys();

var app = builder.Build();
app.UseThargaControllers();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
```

Protect endpoints with the built-in policy:

```csharp
[Authorize(Policy = ApiKeyConstants.PolicyName)]
[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var teamKey = User.FindFirst(TeamClaimTypes.TeamKey)?.Value;
        return Ok(new { teamKey });
    }
}
```

Enforce access levels on services:

```csharp
public interface IMyService
{
    [RequireAccessLevel(AccessLevel.Viewer)]
    IAsyncEnumerable<Item> GetAsync();

    [RequireAccessLevel(AccessLevel.User)]
    Task<Item> AddAsync(string name);
}

// Program.cs
builder.Services.AddScopedWithAccessLevel<IMyService, MyService>();
```

## Links

- [GitHub repository](https://github.com/Tharga/Api)
- [Report an issue](https://github.com/Tharga/Api/issues)
