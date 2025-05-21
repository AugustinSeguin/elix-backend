using System.Security.Claims;
using ElixBackend.Business.IService;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ElixAPI.Middlewares;

public class JwtJtiValidationMiddleware
{
    private readonly RequestDelegate _next;

    public JwtJtiValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IServiceScopeFactory scopeFactory)
    {
        if (context.User.Identity?.IsAuthenticated ?? false)
        {
            var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (jti == null || userId == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token invalid.");
                return;
            }

            // Créer un scope pour les services scoped
            using var scope = scopeFactory.CreateScope();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

            var tokenExists = await tokenService.TokenExistsAsync(jti, int.Parse(userId), DateTime.UtcNow);

            if (!tokenExists)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token invalidated or expired.");
                return;
            }
        }

        await _next(context);
    }
}