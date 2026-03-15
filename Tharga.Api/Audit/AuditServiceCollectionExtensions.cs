using Microsoft.Extensions.DependencyInjection;

namespace Tharga.Api.Audit;

/// <summary>
/// Extension methods for registering audit logging.
/// </summary>
public static class AuditServiceCollectionExtensions
{
    /// <summary>
    /// Registers audit logging with the specified options.
    /// </summary>
    public static IServiceCollection AddThargaAuditLogging(this IServiceCollection services, Action<AuditOptions> configure = null)
    {
        var options = new AuditOptions();
        configure?.Invoke(options);
        services.Configure<AuditOptions>(o =>
        {
            o.StorageMode = options.StorageMode;
            o.CallerFilter = options.CallerFilter;
            o.EventFilter = options.EventFilter;
            o.ExcludedActions = options.ExcludedActions;
            o.ExcludedEndpoints = options.ExcludedEndpoints;
            o.RetentionDays = options.RetentionDays;
            o.BatchSize = options.BatchSize;
            o.FlushIntervalSeconds = options.FlushIntervalSeconds;
        });

        if (options.StorageMode.HasFlag(AuditStorageMode.Logger))
        {
            services.AddSingleton<LoggerAuditLogger>();
            services.AddSingleton<IAuditLogger>(sp => sp.GetRequiredService<LoggerAuditLogger>());
        }

        if (options.StorageMode.HasFlag(AuditStorageMode.MongoDB))
        {
            services.AddTransient<IAuditRepositoryCollection, AuditRepositoryCollection>();
            services.AddSingleton<MongoDbAuditLogger>();
            services.AddSingleton<IAuditLogger>(sp => sp.GetRequiredService<MongoDbAuditLogger>());
            services.AddHostedService(sp => sp.GetRequiredService<MongoDbAuditLogger>());
        }

        // Register as concrete type only — NOT as IAuditLogger to avoid circular dependency.
        // CompositeAuditLogger takes IEnumerable<IAuditLogger> in its constructor,
        // so registering it as IAuditLogger would create a circular resolution deadlock.
        services.AddSingleton<CompositeAuditLogger>();

        return services;
    }
}
