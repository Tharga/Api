# Tharga Api
[![NuGet](https://img.shields.io/nuget/v/Tharga.Api)](https://www.nuget.org/packages/Tharga.Api)
![Nuget](https://img.shields.io/nuget/dt/Tharga.Api)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![GitHub repo Issues](https://img.shields.io/github/issues/Tharga/Api?style=flat&logo=github&logoColor=red&label=Issues)](https://github.com/Tharga/Api/issues?q=is%3Aopen)

Reusable API-key authentication handler, controller registration, and OpenAPI/Swagger setup for ASP.NET Core projects. Targets .NET 9.0 and .NET 10.0.

## Features

- **API key authentication** – `AuthenticationHandler` that reads the `X-API-KEY` header, validates it against a MongoDB-backed store, and populates claims (`TeamKey`, `Name`).
- **Controller registration** – `AddThargaControllers` registers MVC controllers, OpenAPI document generation (with API key security scheme), Swagger UI, and endpoints API explorer in a single call.
- **API key management** – `IApiKeyAdministrationService` provides key lookup, listing, refresh, and lock operations. A default MongoDB-backed implementation is included.
- **Pluggable storage** – Implement `IApiKeyAdministrationService` to use your own data store, or use the built-in `ApiKeyAdministrationService` with Tharga.MongoDB.

## Getting started

### 1. Install the package

```
dotnet add package Tharga.Api
```

### 2. Register controllers and authentication

```csharp
// Program.cs
builder.Services.AddThargaControllers(o =>
{
    o.SwaggerTitle = "My API v1";
});

builder.Services.AddAuthentication()
    .AddThargaApiKeyAuthentication();

// If using the built-in MongoDB key store:
builder.Services.AddThargaApiKeys();
```

### 3. Map controllers and Swagger

```csharp
var app = builder.Build();
app.UseThargaControllers();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
```

### 4. Protect endpoints

```csharp
[Authorize(Policy = ApiKeyConstants.PolicyName)]
[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var teamKey = User.FindFirst(ApiKeyConstants.TeamKeyClaim)?.Value;
        return Ok(new { teamKey });
    }
}
```

### 5. Custom key service (optional)

To use your own storage backend, implement `IApiKeyAdministrationService` and register it:

```csharp
builder.Services.AddAuthentication()
    .AddThargaApiKeyAuthentication<MyCustomKeyService>();
```

## Public API

| Type | Description |
|------|-------------|
| `IApiKey` | Interface representing an API key with metadata (Key, Name, ApiKey, TeamKey, Tags). |
| `IApiKeyAdministrationService` | Service interface for key lookup, listing, refresh, and lock. |
| `ApiKeyConstants` | Well-known constants: `HeaderName`, `SchemeName`, `PolicyName`, `TeamKeyClaim`. |
| `ApiKeyRegistration` | Extension methods: `AddThargaApiKeyAuthentication`, `AddThargaApiKeyAuthentication<T>`. |
| `ControllersRegistration` | Extension methods: `AddThargaControllers`, `AddThargaApiKeys`, `UseThargaControllers`. |
| `ThargaControllerOptions` | Options for Swagger title and route prefix. |
| `ApiKeyEntity` | Default MongoDB entity implementing `IApiKey`. |
| `IApiKeyRepository` | Repository interface for API key persistence. |

## Dependencies

- [Tharga.MongoDB](https://www.nuget.org/packages/Tharga.MongoDB) – MongoDB repository infrastructure and auto-registration.
- [Tharga.Toolkit](https://www.nuget.org/packages/Tharga.Toolkit) – Shared utilities including API key hashing.
- [Swashbuckle.AspNetCore](https://www.nuget.org/packages/Swashbuckle.AspNetCore) – Swagger UI generation.
