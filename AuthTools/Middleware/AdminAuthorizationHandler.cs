using AuthTools.Constants;
using Microsoft.AspNetCore.Authorization;

namespace AuthTools.Middleware;

public class AdminAuthorizationHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.User.IsInRole(RoleNames.Admin) == false)
        {
            return Task.CompletedTask;
        }

        // Automatically succeed all requirements for admin users
        foreach (var requirement in context.PendingRequirements.ToList())
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}