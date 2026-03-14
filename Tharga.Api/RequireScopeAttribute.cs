namespace Tharga.Api;

/// <summary>
/// Declares the scope required to call this method.
/// Used with <see cref="ScopeProxy{T}"/> for automatic enforcement.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RequireScopeAttribute : Attribute
{
    public string Scope { get; }

    public RequireScopeAttribute(string scope)
    {
        Scope = scope;
    }
}
