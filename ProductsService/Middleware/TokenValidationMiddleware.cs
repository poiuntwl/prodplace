using ProductsService.Constants;
using ProductsService.Services;

namespace ProductsService.Middleware;

public class TokenValidationMiddleware : IMiddleware
{
    private readonly IAuthService _authService;

    public TokenValidationMiddleware(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var ct = context.RequestAborted;

        var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
        if (string.IsNullOrWhiteSpace(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("No token provided", ct);
            return;
        }

        var isTokenValid = await _authService.ValidateTokenAsync(token, ct);
        if (isTokenValid == false)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync(
                "Access denied. Token is invalid.",
                ct);
            return;
        }

        context.Items[MiddlewareConsts.IsTokenValidContextItemName] = isTokenValid;

        await next(context);
    }
}