namespace Tharga.Api.Audit;

/// <summary>
/// Interface for audit logging. Implementations handle storage (MongoDB, ILogger, etc.).
/// </summary>
public interface IAuditLogger
{
    void Log(AuditEntry entry);
    Task<IReadOnlyList<AuditEntry>> QueryAsync(AuditQuery query);
}

/// <summary>
/// Query parameters for retrieving audit entries.
/// </summary>
public record AuditQuery
{
    public string TeamKey { get; init; }
    public string CallerIdentity { get; init; }
    public AuditCallerType? CallerType { get; init; }
    public string Feature { get; init; }
    public string Action { get; init; }
    public AuditCallerSource? CallerSource { get; init; }
    public bool? Success { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; } = 100;
}
