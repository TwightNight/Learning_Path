using BlogApp.Application.Common.Interfaces;
using BlogApp.Infrastructure.Auth;
using BlogApp.Infrastructure.Data;
using BlogApp.Infrastructure.Data.Interceptors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(Services.Database);
        Guard.Against.Null(connectionString, message: $"Connection string '{Services.Database}' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlite(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });
        Console.WriteLine(connectionString);
        Console.WriteLine(Directory.GetCurrentDirectory());

        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();
        builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // builder.Services.AddAuthentication()
        //     .AddBearerToken(IdentityConstants.BearerScheme);

        // builder.Services.AddAuthorizationBuilder();


        builder.Services.AddSingleton(TimeProvider.System);
    }
}
