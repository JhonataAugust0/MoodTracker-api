using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Infrastructure.Adapters;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class InactivityCheckService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<InactivityCheckService> _logger;

    public InactivityCheckService(IServiceScopeFactory serviceScopeFactory, ILogger<InactivityCheckService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Inactivity Check Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                var redisService = scope.ServiceProvider.GetRequiredService<IRedisService>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var notificationService = scope.ServiceProvider.GetRequiredService<NotificationHub>();

                var inactiveUsers = await userService.GetInactiveUsers(stoppingToken);

                foreach (var user in inactiveUsers)
                {
                    var connectionIds = await redisService.GetConnectionIds(user.Id);
                    if (connectionIds.Any())
                    {
                        _logger.LogInformation($"User {user.Id} online, sending SignalR notification");
                        await notificationService.Clients.Clients(connectionIds)
                            .SendAsync("ReceiveNotification",
                                "Você está há 3 dias sem registrar seu humor!", cancellationToken: stoppingToken);
                    }
                    else
                    {
                        _logger.LogInformation($"User {user.Id} offline, sending email");
                        await emailService.SendNotificationEmail(user.Email);
                        await redisService.StoreNotification(user.Id, "Você está há 3 dias sem registrar seu humor!");
                    }

                    await userService.UpdateUserLastNotifiedAsync(user.Id, DateTime.UtcNow, stoppingToken);
                }
            }

            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
}
