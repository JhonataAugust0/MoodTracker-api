using System.Text;
using System.Threading.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Domain.Interfaces;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Infrastructure.Logging;
using MoodTracker_back.Infrastructure.Adapters;
using MoodTracker_back.Infrastructure.Data.Repositories;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddTransient<IEmailService, Smtp>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IMoodService, MoodService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IHabitService, HabitService>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ILoggingService, LoggingService>();
        services.AddScoped<IMoodRepository, MoodRepository>();
        services.AddScoped<IUserRepository, UserRepository>(); 
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IHabitRepository, HabitRepository>();
        services.AddScoped<IQuickNotesService, QuickNotesService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IQuickNoteRepository, QuickNotesRepository>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IHabitCompletionRepository, HabitCompletionCompletionRepository>();
        return services;
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        var csp = "default-src 'self'; " +
                  "script-src 'self'; " +
                  "style-src 'self'; " +
                  "img-src 'self' data:; " +
                  "connect-src 'self'; " +
                  "frame-src 'none'; " +
                  "base-uri 'self'; " +
                  "form-action 'self';";

        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Content-Security-Policy", csp);
            // context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            await next();
        });

        return app;
    }

    public static IServiceCollection AddCustomJwtSecurity(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
                    ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")!))
                };
            });
        return services;
    }

    public static IServiceCollection AddCustomRateLimiter(this IServiceCollection services)
    {
    services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var clientId = context.Request.Headers["X-Client-Id"].ToString();
                return RateLimitPartition.GetFixedWindowLimiter(clientId ?? "anonymous",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 300,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });
    
            options.OnRejected = async (context, _) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken: _);
            };
        });

        return services;
    }

    public static IServiceCollection AddCustomCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
        return services;
    }
}