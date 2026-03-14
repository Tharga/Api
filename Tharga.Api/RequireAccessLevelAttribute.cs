namespace Tharga.Api;

/// <summary>
/// Declares the minimum access level required to call this method.
/// Used with <see cref="AccessLevelProxy{T}"/> for automatic enforcement.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RequireAccessLevelAttribute : Attribute
{
    public AccessLevel MinimumLevel { get; }

    public RequireAccessLevelAttribute(AccessLevel minimumLevel)
    {
        MinimumLevel = minimumLevel;
    }
}
