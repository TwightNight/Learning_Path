using System.Security.Claims;
using BlogApp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Web.Middleware;

public sealed class TokenVersionMiddleware
{
    private readonly RequestDelegate _next;

    public TokenVersionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // IApplicationDbContext là Scoped -> phải inject qua InvokeAsync,
    // KHÔNG inject qua constructor (middleware chỉ khởi tạo 1 lần).
    public async Task InvokeAsync(HttpContext context, IApplicationDbContext dbContext)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var tokenVersionClaim = user.FindFirstValue("tv");

            if (!int.TryParse(userIdClaim, out var userId) ||
                !int.TryParse(tokenVersionClaim, out var tokenVersionFromToken))
            {
                await RejectAsync(context, "Token is invalid.");
                return;
            }

            var currentVersion = await dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => (int?)u.TokenVersioning)
                .FirstOrDefaultAsync(context.RequestAborted);

            if (currentVersion is null || currentVersion.Value != tokenVersionFromToken)
            {
                await RejectAsync(context, "Your token has been revoked, please log in again.");
                return;
            }
        }

        await _next(context);
    }

    private static async Task RejectAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = message,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}