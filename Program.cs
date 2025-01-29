using DotNetEnv;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;

using Infrastructure.Middlewares;
using Infrastructure.Data.Config;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MoodTracker_back.Infrastructure.Logging;
using MoodTracker_back.Infrastructure.Adapters;
using MoodTracker_back.Infrastructure.Middlewares;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? 
                       builder.Configuration.GetConnectionString("DefaultConnection");

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

LoggingConfiguration.ConfigureLogging(builder);
builder.Services.AddSingleton(new EmailSettings 
{
    SmtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "",
    SmtpPort = Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587",
    UserEmail = Environment.GetEnvironmentVariable("SMTP_USER") ?? "",
    Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? "",
    SenderName = Environment.GetEnvironmentVariable("SMTP_SENDER_NAME") ?? "MoodTracker"
});

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();


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


builder.Services.AddCustomCors();
builder.Services.AddCustomServices();
builder.Services.AddCustomRateLimiter();
builder.Services.AddCustomJwtSecurity(builder.Configuration);

builder.Services.AddOptions<EmailSettings>()
    .ValidateDataAnnotations()
    .ValidateOnStart();

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
    app.UseCors("AllowFrontend");
}

app.UseSwagger();
app.UseSecurityHeaders();
app.UseMiddleware<AuthMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MoodTracker API v1.0"));

// app.UseHttpsRedirection();
app.MapHealthChecks("/health");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }