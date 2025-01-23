using DotNetEnv;
using System.Text;

using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Domain.Interfaces;
using Infrastructure.Middlewares;
using Infrastructure.Data.Config;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Infrastructure.Logging;
using MoodTracker_back.Infrastructure.Adapters;
using MoodTracker_back.Infrastructure.Middlewares;
using MoodTracker_back.Infrastructure.Data.Repositories;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? 
                       builder.Configuration.GetConnectionString("DefaultConnection");

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.Configure<EmailSettings>(options =>
{
    options.SmtpServer = Environment.GetEnvironmentVariable("SMTP_HOST") ?? string.Empty;
    options.SmtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "25");
    options.SmtpUsername = Environment.GetEnvironmentVariable("USER") ?? string.Empty;
    options.SmtpPassword = Environment.GetEnvironmentVariable("PASSWORD") ?? string.Empty;
    options.FromEmail = Environment.GetEnvironmentVariable("FROM_ADDERS") ?? string.Empty;
    options.FromName = "Your App Name";
    options.UseSSL = false;
});

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddTransient<IEmailService, Smtp>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IMoodService, MoodService>();
builder.Services.AddScoped<IUserService, UserService>(); 
builder.Services.AddScoped<IHabitService, HabitService>();
builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IMoodRepository, MoodRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>(); 
builder.Services.AddScoped<ITokenService, JwtTokenGenerator>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IHabitRepository, HabitRepository>();
builder.Services.AddScoped<IQuickNotesService, QuickNotesService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IQuickNoteRepository, QuickNotesRepository>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IHabitCompletionRepository, HabitCompletionCompletionRepository>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
    // options.ListenAnyIP(5001, listenOptions =>
    //     {
    //         listenOptions.UseHttps();
    //     });
    
});

LoggingConfiguration.ConfigureLogging(builder);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    if (dbContext.Database.GetPendingMigrations().Any())
    {
        dbContext.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCors();
}

app.UseSwagger();
app.UseMiddleware<AuthMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MoodTracker API v1.0"));

app.MapHealthChecks("/health");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }