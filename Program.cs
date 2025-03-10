using DotNetEnv;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.SignalR;
using MoodTracker_back.Infrastructure.Data.Postgres.Config;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MoodTracker_back;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Infrastructure.Logging;
using MoodTracker_back.Infrastructure.Middlewares;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Infrastructure.Adapters.Notifications;
using MoodTracker_back.Infrastructure.Adapters.Redis;
using MoodTracker_back.Infrastructure.Adapters.Smtp;
using StackExchange.Redis;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? 
                       builder.Configuration.GetConnectionString("DefaultConnection");

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING"))
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging());

LoggingConfiguration.ConfigureLogging(builder);
builder.Services.AddSingleton(new EmailSettings() 
{
    SmtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "",
    SmtpPort = Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587",
    UserEmail = Environment.GetEnvironmentVariable("SMTP_USER") ?? "",
    Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? "",
    SenderName = Environment.GetEnvironmentVariable("SMTP_SENDER_NAME") ?? "MoodTracker"
});

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(
        new ConfigurationOptions{
            EndPoints= {{
                Environment.GetEnvironmentVariable("REDIS_ENDPOINT"), 
                int.Parse(Environment.GetEnvironmentVariable("REDIS_PORT"))
            }},
            User=Environment.GetEnvironmentVariable("REDIS_USER"),
            Password=Environment.GetEnvironmentVariable("REDIS_PASSWORD")
        }
    )
);

builder.Services.AddSingleton<NotificationHub>();
builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddSingleton<IHostedService, CheckInactivityAppService>();

builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MoodTracker API", Version = "v1" });
    c.AddSecurityDefinition("JWT", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Insira o token JWT Bearer {seu token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "JWT"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

builder.Services.AddCustomCors();
builder.Services.AddCustomServices();
builder.Services.AddCustomRateLimiter();
builder.Services.AddCustomJwtSecurity(builder.Configuration);

builder.Services.AddOptions<EmailSettings>()
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSignalR();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
    // options.ListenAnyIP(5001, listenOptions =>
    //     {
    //         listenOptions.UseHttps();
    //     });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var signalR = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();
        var redis = scope.ServiceProvider.GetRequiredService<IRedisService>();
        
        if (dbContext.Database.GetPendingMigrations().Any())
        {
            logger.LogInformation("Applying pending migrations...");
            dbContext.Database.Migrate();
            logger.LogInformation("Migrations applied successfully");
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSecurityHeaders(app.Environment);
app.UseMiddleware<AuthMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MoodTracker API v1.0"));
app.MapHub<NotificationHub>("/notificationHub");
// app.UseHttpsRedirection();
app.MapHealthChecks("/health");

// app.Map("/api", api =>
// {
//     api.Use(async (context, next) =>
//     {
//         if (context.Request.Method == "OPTIONS")
//         {
//             context.Response.StatusCode = 200;
//             await context.Response.CompleteAsync();
//             return;
//         }
//         await next();
//     });
// });

app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();



app.MapControllers();
app.Run();

public partial class Program { }