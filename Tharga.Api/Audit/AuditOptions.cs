namespace Tharga.Api.Audit;

/// <summary>
/// Configuration for audit logging.
/// </summary>
public class AuditOptions
{
    /// <summary>Where to store audit entries. Default: Logger only.</summary>
    public AuditStorageMode StorageMode { get; set; } = AuditStorageMode.Logger;

    /// <summary>Which caller sources to log. Default: Api and Web.</summary>
    public AuditCallerFilter CallerFilter { get; set; } = AuditCallerFilter.Api | AuditCallerFilter.Web;

    /// <summary>Which event types to log. Default: All.</summary>
    public AuditEventFilter EventFilter { get; set; } = AuditEventFilter.All;

    /// <summary>Actions to exclude from logging (e.g. "read", "list", "get"). Default: empty.</summary>
    public string[] ExcludedActions { get; set; } = Array.Empty<string>();

    /// <summary>Endpoints to exclude from logging (e.g. "/health"). Default: empty.</summary>
    public string[] ExcludedEndpoints { get; set; } = Array.Empty<string>();

    /// <summary>Days to retain audit entries in MongoDB. Default: 90.</summary>
    public int RetentionDays { get; set; } = 90;

    /// <summary>Batch size for background MongoDB writer. Default: 100.</summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>Flush interval for background MongoDB writer in seconds. Default: 5.</summary>
    public int FlushIntervalSeconds { get; set; } = 5;
}
