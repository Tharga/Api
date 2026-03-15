namespace Tharga.Api.Audit;

public enum AuditEventType
{
    ServiceCall,
    AuthSuccess,
    AuthFailure,
    ScopeDenial,
    DataChange,
    RateLimit
}
