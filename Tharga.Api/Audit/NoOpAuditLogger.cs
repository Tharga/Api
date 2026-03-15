namespace Tharga.Api.Audit;

/// <summary>
/// No-op audit logger for testing or when audit logging is disabled.
/// </summary>
public class NoOpAuditLogger : IAuditLogger
{
    public void Log(AuditEntry entry) { }
    public Task<IReadOnlyList<AuditEntry>> QueryAsync(AuditQuery query)
        => Task.FromResult<IReadOnlyList<AuditEntry>>(Array.Empty<AuditEntry>());
}
