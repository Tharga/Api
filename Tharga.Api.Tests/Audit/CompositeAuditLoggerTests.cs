using Microsoft.Extensions.Options;
using Tharga.Api.Audit;

namespace Tharga.Api.Tests.Audit;

public class CompositeAuditLoggerTests
{
    private static CompositeAuditLogger CreateLogger(AuditOptions options, params IAuditLogger[] loggers)
    {
        return new CompositeAuditLogger(loggers, Options.Create(options));
    }

    [Fact]
    public void Log_Dispatches_To_All_Loggers()
    {
        var spy1 = new SpyAuditLogger();
        var spy2 = new SpyAuditLogger();
        var logger = CreateLogger(new AuditOptions(), spy1, spy2);

        logger.Log(new AuditEntry
        {
            Timestamp = DateTime.UtcNow,
            EventType = AuditEventType.ServiceCall,
            CallerSource = AuditCallerSource.Api
        });

        Assert.Single(spy1.Entries);
        Assert.Single(spy2.Entries);
    }

    [Fact]
    public void Log_Filters_By_CallerSource()
    {
        var spy = new SpyAuditLogger();
        var options = new AuditOptions { CallerFilter = AuditCallerFilter.Api };
        var logger = CreateLogger(options, spy);

        logger.Log(new AuditEntry
        {
            Timestamp = DateTime.UtcNow,
            EventType = AuditEventType.ServiceCall,
            CallerSource = AuditCallerSource.Web
        });

        Assert.Empty(spy.Entries);
    }

    [Fact]
    public void Log_Filters_By_EventType()
    {
        var spy = new SpyAuditLogger();
        var options = new AuditOptions { EventFilter = AuditEventFilter.Denials };
        var logger = CreateLogger(options, spy);

        logger.Log(new AuditEntry
        {
            Timestamp = DateTime.UtcNow,
            EventType = AuditEventType.ServiceCall,
            CallerSource = AuditCallerSource.Api
        });

        Assert.Empty(spy.Entries);
    }

    [Fact]
    public void Log_Allows_Matching_EventType()
    {
        var spy = new SpyAuditLogger();
        var options = new AuditOptions { EventFilter = AuditEventFilter.Denials };
        var logger = CreateLogger(options, spy);

        logger.Log(new AuditEntry
        {
            Timestamp = DateTime.UtcNow,
            EventType = AuditEventType.ScopeDenial,
            CallerSource = AuditCallerSource.Api
        });

        Assert.Single(spy.Entries);
    }

    [Fact]
    public void Log_Filters_By_ExcludedActions()
    {
        var spy = new SpyAuditLogger();
        var options = new AuditOptions { ExcludedActions = ["read", "list"] };
        var logger = CreateLogger(options, spy);

        logger.Log(new AuditEntry
        {
            Timestamp = DateTime.UtcNow,
            EventType = AuditEventType.ServiceCall,
            CallerSource = AuditCallerSource.Api,
            Action = "read"
        });

        Assert.Empty(spy.Entries);
    }

    [Fact]
    public void Log_ExcludedActions_Is_Case_Insensitive()
    {
        var spy = new SpyAuditLogger();
        var options = new AuditOptions { ExcludedActions = ["Read"] };
        var logger = CreateLogger(options, spy);

        logger.Log(new AuditEntry
        {
            Timestamp = DateTime.UtcNow,
            EventType = AuditEventType.ServiceCall,
            CallerSource = AuditCallerSource.Api,
            Action = "read"
        });

        Assert.Empty(spy.Entries);
    }

    [Fact]
    public void Log_Allows_Non_Excluded_Action()
    {
        var spy = new SpyAuditLogger();
        var options = new AuditOptions { ExcludedActions = ["read"] };
        var logger = CreateLogger(options, spy);

        logger.Log(new AuditEntry
        {
            Timestamp = DateTime.UtcNow,
            EventType = AuditEventType.ServiceCall,
            CallerSource = AuditCallerSource.Api,
            Action = "write"
        });

        Assert.Single(spy.Entries);
    }

    [Fact]
    public async Task QueryAsync_Returns_Empty_When_No_Queryable_Logger()
    {
        var spy = new SpyAuditLogger();
        var logger = CreateLogger(new AuditOptions(), spy);

        var result = await logger.QueryAsync(new AuditQuery());

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    private class SpyAuditLogger : IAuditLogger
    {
        public List<AuditEntry> Entries { get; } = new();

        public void Log(AuditEntry entry) => Entries.Add(entry);

        public Task<AuditQueryResult> QueryAsync(AuditQuery query)
            => Task.FromResult(new AuditQueryResult());
    }
}
