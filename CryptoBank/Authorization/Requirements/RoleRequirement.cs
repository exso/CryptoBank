using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CryptoBank.Authorization.Requirements;

public class RoleRequirement : IAuthorizationRequirement
{
    public RoleRequirement(string requiredRole)
    {
        RequiredRole = requiredRole;
    }

    public string RequiredRole { get; }
}

public class RoleRequirementHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        if (context.User.HasClaim(ClaimTypes.Role, requirement.RequiredRole))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
