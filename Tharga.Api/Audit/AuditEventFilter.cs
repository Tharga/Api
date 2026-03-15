namespace Tharga.Api.Audit;

[Flags]
public enum AuditEventFilter
{
    None = 0,
    ServiceCalls = 1,
    AuthEvents = 2,
    Denials = 4,
    DataChanges = 8,
    RateLimits = 16,
    All = ServiceCalls | AuthEvents | Denials | DataChanges | RateLimits
}
