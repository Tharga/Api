using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Tharga.Api.Audit;

namespace Tharga.Api;

/// <summary>
/// Extension methods for registering scopes and scope-protected services.
/// </summary>
public static class ScopeServiceCollectionExtensions
{
    /// <summary>
    /// Registers the scope registry and configures scopes.
    /// </summary>
    public static IServiceCollection AddThargaScopes(this IServiceCollection services, Action<ScopeRegistry> configure)
    {
        var registry = services.BuildServiceProvider().GetService<IScopeRegistry>() as ScopeRegistry;
        if (registry == null)
        {
            registry = new ScopeRegistry();
            services.AddSingleton<IScopeRegistry>(registry);
        }

        configure(registry);
        return services;
    }

    /// <summary>
    /// Registers a scoped service wrapped in a <see cref="ScopeProxy{T}"/>
    /// that enforces <see cref="RequireScopeAttribute"/> on every method call.
    /// </summary>
    public static IServiceCollection AddScopedWithScopes<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddScoped<TImplementation>();
        services.AddScoped<TService>(sp =>
        {
            var target = sp.GetRequiredService<TImplementation>();
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            var auditLogger = sp.GetService<CompositeAuditLogger>();
            return ScopeProxy<TService>.Create(target, httpContextAccessor, auditLogger);
        });
        return services;
    }
}
