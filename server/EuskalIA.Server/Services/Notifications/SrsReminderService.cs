using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace EuskalIA.Server.Services.Notifications
{
    public class SrsReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SrsReminderService> _logger;

        public SrsReminderService(IServiceProvider serviceProvider, ILogger<SrsReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SRS Reminder Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing SRS reminders");
                }

                // Run every 12 hours
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }

            _logger.LogInformation("SRS Reminder Background Service stopping.");
        }

        private async Task CheckAndSendRemindersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var now = DateTime.UtcNow;

            // Find users who have at least one node due for review and have a PushToken
            var usersWithDueReviews = await context.Users
                .Where(u => u.IsActive && !string.IsNullOrEmpty(u.ExpoPushToken))
                .Where(u => u.SrsNodes.Any(n => n.NextReviewDate <= now))
                .Select(u => new { u.ExpoPushToken, u.Language, u.Nickname, u.Username })
                .ToListAsync();

            if (!usersWithDueReviews.Any())
            {
                _logger.LogInformation("No pending reviews found for notification.");
                return;
            }

            _logger.LogInformation($"Found {usersWithDueReviews.Count} users with pending reviews.");

            foreach (var user in usersWithDueReviews)
            {
                if (user.ExpoPushToken == null) continue;

                // Simple localization logic
                string title = user.Language switch
                {
                    "eu" => "EuskalLingo: Errepasoak zain!",
                    "en" => "EuskalLingo: Reviews pending!",
                    "fr" => "EuskalLingo: Révisions en attente!",
                    "pl" => "EuskalLingo: Powtórki czekają!",
                    _ => "EuskalLingo: ¡Repasos pendientes!"
                };

                string body = user.Language switch
                {
                    "eu" => "Zure eguneroko errepasoa egiteko unea da. Ez utzi metatu!",
                    "en" => "It's time for your daily review. Don't let them pile up!",
                    "fr" => "C'est l'heure de votre révision quotidienne. Ne les laissez pas s'accumuler!",
                    "pl" => "Czas na codzienną powtórkę. Nie pozwól im się nagromadzić!",
                    _ => "Es hora de tu repaso diario. ¡No dejes que se acumulen!"
                };

                await notificationService.SendPushNotificationAsync(user.ExpoPushToken, title, body, new { type = "REVIEW_REMINDER" });
            }
        }
    }
}
