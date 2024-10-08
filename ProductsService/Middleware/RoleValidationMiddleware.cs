using Microsoft.AspNetCore.Authorization;
using ProductsService.Constants;
using ProductsService.Services;

namespace ProductsService.Middleware;

public class RoleValidationMiddleware : IMiddleware
{
    private readonly IAuthService _authService;

    public RoleValidationMiddleware(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var ct = context.RequestAborted;

        if (context.Items.TryGetValue(MiddlewareConsts.IsTokenValidContextItemName, out var isTokenValidItem) == false)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return;
        }

        if (isTokenValidItem is false)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Provided token is invalid.", ct);
            return;
        }

        var endpoint = context.GetEndpoint();
        var authorizeAttribute = endpoint?.Metadata.GetMetadata<AuthorizeAttribute>();

        if (authorizeAttribute != null && !string.IsNullOrWhiteSpace(authorizeAttribute.Roles))
        {
            var requiredRoles = authorizeAttribute.Roles.Split(',').Select(r => r.Trim()).ToArray();
            var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("No token provided", ct);
                return;
            }

            var hasRequiredRole = await _authService.ValidateRolesAsync(token, requiredRoles, ct);

            if (hasRequiredRole == false)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync(
                    $"Access denied. Required role(s): {string.Join(", ", requiredRoles)}",
                    ct);
                return;
            }
        }

        await next(context);
    }
}