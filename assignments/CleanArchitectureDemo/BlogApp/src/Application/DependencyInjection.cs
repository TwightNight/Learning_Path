using System.Reflection;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddFastEndpoints();
        var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");
        builder.Services
            .AddAuthenticationJwtBearer(s => s.SigningKey = jwtSigningKey)
            .AddAuthorization();

    }
}
