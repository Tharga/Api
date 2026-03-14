using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Tharga.Api;

/// <summary>
/// DispatchProxy that intercepts service method calls and enforces
/// <see cref="RequireScopeAttribute"/> by checking scope claims on HttpContext.User.
/// Methods without the attribute throw InvalidOperationException (fail-closed).
/// Also verifies that a TeamKey claim is present.
/// </summary>
public class ScopeProxy<T> : DispatchProxy where T : class
{
    private T _target;
    private IHttpContextAccessor _httpContextAccessor;

    public static T Create(T target, IHttpContextAccessor httpContextAccessor)
    {
        var proxy = Create<T, ScopeProxy<T>>() as ScopeProxy<T>;
        proxy._target = target;
        proxy._httpContextAccessor = httpContextAccessor;
        return proxy as T;
    }

    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        var attribute = GetAttribute(targetMethod);
        if (attribute == null)
            throw new InvalidOperationException(
                $"Method '{typeof(T).Name}.{targetMethod.Name}' is missing the [RequireScope] attribute. " +
                $"All methods on services registered with AddScopedWithScopes must declare their required scope.");

        CheckScope(attribute.Scope);

        return targetMethod.Invoke(_target, args);
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
