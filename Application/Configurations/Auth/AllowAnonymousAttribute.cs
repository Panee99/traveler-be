using Microsoft.AspNetCore.Authorization;

namespace Application.Configurations.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllowAnonymousAttribute : Attribute, IAllowAnonymous
{
}