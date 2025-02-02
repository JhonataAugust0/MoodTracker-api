using MoodTracker_back.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Infrastructure.Adapters.Notifications;
using Microsoft.Extensions.Logging;

namespace MoodTracker_back.Application.Services;

public class CheckInactivityAppService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRedisService _redisService;
    private readonly ILogger<CheckInactivityAppService> _logger;

    public CheckInactivityAppService(
        IServiceScopeFactory scopeFactory,
        IRedisService redisService,
        ILogger<CheckInactivityAppService> logger)
    {
        _scopeFactory = scopeFactory;
        _redisService = redisService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Inactivity Check Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var notificationService = scope.ServiceProvider.GetRequiredService<NotificationHub>();

                await CheckInactivityAndNotifyUsersAsync(
                    userService, 
                    emailService, 
                    notificationService, 
                    stoppingToken);
            }
            
            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }

    private async Task CheckInactivityAndNotifyUsersAsync(
        IUserService userService,
        IEmailService emailService,
        NotificationHub notificationService,
        CancellationToken stoppingToken)
    {
        var inactiveUsers = await userService.GetInactiveUsers(stoppingToken);

        foreach (var user in inactiveUsers)
        {
            await NotifyUserAsync(user, userService, emailService, notificationService, stoppingToken);
            await userService.UpdateUserLastNotifiedAsync(user.Id, DateTime.UtcNow, stoppingToken);
        }
    }

    private async Task NotifyUserAsync(
        User user, 
        IUserService userService,
        IEmailService emailService,
        NotificationHub notificationService,
        CancellationToken stoppingToken)
    {
        var connectionIds = await _redisService.GetConnectionIds(user.Id);
        if (connectionIds.Any())
        {
            _logger.LogInformation($"User {user.Id} online, sending SignalR notification");
            await SendSignalRNotificationAsync(connectionIds, notificationService, stoppingToken);
        }
        else
        {
            _logger.LogInformation($"User {user.Id} offline, sending email");
            await SendEmailNotificationAsync(user.Email, emailService);
            await _redisService.StoreNotification(user.Id, "Você está há 3 dias sem registrar seu humor!");
        }
    }

    private async Task SendSignalRNotificationAsync(
        IEnumerable<string> connectionIds, 
        NotificationHub notificationService,
        CancellationToken stoppingToken)
    {
        await notificationService.Clients.Clients(connectionIds)
            .SendAsync("ReceiveNotification", "Você está há 3 dias sem registrar seu humor!", cancellationToken: stoppingToken);
    }

    private async Task SendEmailNotificationAsync(string email, IEmailService emailService)
    {
        await emailService.SendNotificationEmail(email);
    }
}