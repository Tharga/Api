using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Tharga.Api;

/// <summary>
/// Extension methods for registering services with automatic access level enforcement.
/// </summary>
public static class AccessLevelServiceCollectionExtensions
{
    /// <summary>
    /// Registers a scoped service wrapped in an <see cref="AccessLevelProxy{T}"/>
    /// that enforces <see cref="RequireAccessLevelAttribute"/> on every method call.
    /// </summary>
    public static IServiceCollection AddScopedWithAccessLevel<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddScoped<TImplementation>();
        services.AddScoped<TService>(sp =>
        {
            var target = sp.GetRequiredService<TImplementation>();
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            return AccessLevelProxy<TService>.Create(target, httpContextAccessor);
        });
        return services;
    }
}
