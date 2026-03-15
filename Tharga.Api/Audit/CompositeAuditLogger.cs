using Microsoft.Extensions.Options;

namespace Tharga.Api.Audit;

/// <summary>
/// Dispatches audit entries to configured loggers based on AuditOptions filters.
/// </summary>
public class CompositeAuditLogger : IAuditLogger
{
    private readonly IAuditLogger[] _loggers;
    private readonly AuditOptions _options;
    private readonly IAuditLogger _queryLogger;

    public CompositeAuditLogger(IEnumerable<IAuditLogger> loggers, IOptions<AuditOptions> options)
    {
        _loggers = loggers.Where(l => l != this).ToArray();
        _options = options.Value;
        _queryLogger = _loggers.FirstOrDefault(l => l is MongoDbAuditLogger);
    }

    public void Log(AuditEntry entry)
    {
        if (!ShouldLog(entry)) return;

        foreach (var logger in _loggers)
        {
            logger.Log(entry);
        }
    }

    public Task<AuditQueryResult> QueryAsync(AuditQuery query)
    {
        return _queryLogger?.QueryAsync(query) ?? Task.FromResult(new AuditQueryResult());
    }

    private bool ShouldLog(AuditEntry entry)
    {
        // Check caller filter
        var callerFlag = entry.CallerSource switch
        {
            AuditCallerSource.Api => AuditCallerFilter.Api,
            AuditCallerSource.Web => AuditCallerFilter.Web,
            _ => AuditCallerFilter.Api | AuditCallerFilter.Web
        };
        if ((_options.CallerFilter & callerFlag) == 0) return false;

        // Check event filter
        var eventFlag = entry.EventType switch
        {
            AuditEventType.ServiceCall => AuditEventFilter.ServiceCalls,
            AuditEventType.AuthSuccess or AuditEventType.AuthFailure => AuditEventFilter.AuthEvents,
            AuditEventType.ScopeDenial => AuditEventFilter.Denials,
            AuditEventType.DataChange => AuditEventFilter.DataChanges,
            AuditEventType.RateLimit => AuditEventFilter.RateLimits,
            _ => AuditEventFilter.None
        };
        if ((_options.EventFilter & eventFlag) == 0) return false;

        // Check excluded actions
        if (entry.Action != null && _options.ExcludedActions.Contains(entry.Action, StringComparer.OrdinalIgnoreCase))
            return false;

        return true;
    }
}
