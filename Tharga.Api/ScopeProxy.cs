using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Tharga.Api.Audit;

namespace Tharga.Api;

/// <summary>
/// DispatchProxy that intercepts service method calls and enforces
/// <see cref="RequireScopeAttribute"/> by checking scope claims on HttpContext.User.
/// Methods without the attribute throw InvalidOperationException (fail-closed).
/// Also verifies that a TeamKey claim is present.
/// Logs audit entries when IAuditLogger is available.
/// </summary>
public class ScopeProxy<T> : DispatchProxy where T : class
{
    private T _target;
    private IHttpContextAccessor _httpContextAccessor;
    private IAuditLogger _auditLogger;

    public static T Create(T target, IHttpContextAccessor httpContextAccessor, IAuditLogger auditLogger = null)
    {
        var proxy = Create<T, ScopeProxy<T>>() as ScopeProxy<T>;
        proxy._target = target;
        proxy._httpContextAccessor = httpContextAccessor;
        proxy._auditLogger = auditLogger;
        return proxy as T;
    }

    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        var attribute = GetAttribute(targetMethod);
        if (attribute == null)
            throw new InvalidOperationException(
                $"Method '{typeof(T).Name}.{targetMethod.Name}' is missing the [RequireScope] attribute. " +
                $"All methods on services registered with AddScopedWithScopes must declare their required scope.");

        var (feature, action) = AuditEntry.ParseScope(attribute.Scope);
        var sw = Stopwatch.StartNew();

        try
        {
            CheckScope(attribute.Scope);

            var result = targetMethod.Invoke(_target, args);
            sw.Stop();

            LogAudit(attribute.Scope, feature, action, targetMethod.Name, sw.ElapsedMilliseconds, true, AuditScopeResult.Allowed);

            return result;
        }
        catch (UnauthorizedAccessException ex) when (ex.Message.Contains("Missing required scope"))
        {
            sw.Stop();
            LogAudit(attribute.Scope, feature, action, targetMethod.Name, sw.ElapsedMilliseconds, false, AuditScopeResult.Denied, ex.Message);
            throw;
        }
        catch (TargetInvocationException tie)
        {
            sw.Stop();
            LogAudit(attribute.Scope, feature, action, targetMethod.Name, sw.ElapsedMilliseconds, false, AuditScopeResult.Allowed, tie.InnerException?.Message);
            throw;
        }
        catch (Exception ex)
        {
            sw.Stop();
            LogAudit(attribute.Scope, feature, action, targetMethod.Name, sw.ElapsedMilliseconds, false, AuditScopeResult.Allowed, ex.Message);
            throw;
        }
    }

    private void LogAudit(string scope, string feature, string action, string methodName, long durationMs, bool success, AuditScopeResult scopeResult, string errorMessage = null)
    {
        if (_auditLogger == null) return;

        var user = _httpContextAccessor.HttpContext?.User;
        var identity = user?.Identity;

        var callerSource = identity?.AuthenticationType switch
        {
            ApiKeyConstants.SchemeName => AuditCallerSource.Api,
            "Cookies" or "AuthenticationTypes.Federation" => AuditCallerSource.Web,
            _ => AuditCallerSource.Unknown
        };

        var callerType = callerSource == AuditCallerSource.Api
            ? AuditCallerType.ApiKey
            : AuditCallerType.User;

        var entry = new AuditEntry
        {
            Timestamp = DateTime.UtcNow,
            EventType = scopeResult == AuditScopeResult.Denied ? AuditEventType.ScopeDenial : AuditEventType.ServiceCall,
            Feature = feature,
            Action = action,
            MethodName = methodName,
            DurationMs = durationMs,
            Success = success,
            ErrorMessage = errorMessage,
            CallerType = callerType,
            CorrelationId = Guid.TryParse(_httpContextAccessor.HttpContext?.TraceIdentifier, out var traceId) ? traceId : Guid.NewGuid(),
            CallerIdentity = user?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                ?? user?.FindFirst("preferred_username")?.Value
                ?? user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? user?.FindFirst("name")?.Value,
            TeamKey = user?.FindFirst(TeamClaimTypes.TeamKey)?.Value,
            AccessLevel = user?.FindFirst(TeamClaimTypes.AccessLevel)?.Value,
            CallerSource = callerSource,
            ScopeChecked = scope,
            ScopeResult = scopeResult,
        };

        _auditLogger.Log(entry);
    }

    private RequireScopeAttribute GetAttribute(MethodInfo methodInfo)
    {
        var interfaceMethod = typeof(T).GetMethod(
            methodInfo.Name,
            methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());
        return interfaceMethod?.GetCustomAttribute<RequireScopeAttribute>()
               ?? methodInfo.GetCustomAttribute<RequireScopeAttribute>();
    }

    private void CheckScope(string requiredScope)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        var teamKey = user?.FindFirst(TeamClaimTypes.TeamKey)?.Value;
        if (string.IsNullOrEmpty(teamKey))
            throw new UnauthorizedAccessException("No team selected.");

        var hasScope = user?.Claims
            .Where(c => c.Type == TeamClaimTypes.Scope)
            .Any(c => c.Value == requiredScope) ?? false;

        if (!hasScope)
            throw new UnauthorizedAccessException(
                $"Missing required scope '{requiredScope}'.");
    }
}
