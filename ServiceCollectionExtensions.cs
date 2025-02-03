using System.Text;
using System.Threading.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Infrastructure.Logging;
using MoodTracker_back.Infrastructure.Adapters;
using MoodTracker_back.Infrastructure.Adapters.Security;
using MoodTracker_back.Infrastructure.Adapters.Smtp;
using MoodTracker_back.Infrastructure.Data.Postgres.Repositories;

namespace MoodTracker_back;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddTransient<IEmailService, SmtpService>();
        services.AddScoped<ITagService, TagAppService>();
        services.AddScoped<IMoodService, MoodAppService>();
        services.AddScoped<IUserService, UserAppService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IHabitService, HabitAppService>();
        services.AddScoped<ILoggingService, LoggingService>();
        services.AddScoped<IMoodRepository, MoodRepository>();
        services.AddScoped<IUserRepository, UserRepository>(); 
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IHabitRepository, HabitRepository>();
        services.AddScoped<IQuickNotesService, QuickNotesAppService>();
        services.AddScoped<ICurrentUserService, CurrentUserAppService>();
        services.AddScoped<IQuickNoteRepository, QuickNotesRepository>();
        services.AddScoped<IAuthenticationService, AuthenticationAppService>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IHabitCompletionRepository, HabitCompletionCompletionRepository>();

        return services;
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        var csp = env.IsDevelopment()
          ? "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data:; " +
            "connect-src 'self'; " +
            "font-src 'self'; " +
            "frame-src 'self';"
          : "default-src 'self'; " + 
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self'; " +
            "img-src 'self' data:; " +
            "connect-src 'self'; " +
            "frame-src 'none';";

        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Frame-Options", env.IsDevelopment() ? "SAMEORIGIN" : "DENY");
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
                policy.WithOrigins("https://mood-tracker-front-mpwb-b1cdno52q-jhonataaugust0s-projects.vercel.app")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
        return services;
    }
}
