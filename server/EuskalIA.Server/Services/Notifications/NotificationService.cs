using System.Net.Http.Json;
using System.Text.Json;

namespace EuskalIA.Server.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationService> _logger;
        private const string ExpoPushUrl = "https://exp.host/--/api/v2/push/send";

        public NotificationService(HttpClient httpClient, ILogger<NotificationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task SendPushNotificationAsync(string pushToken, string title, string body, object? data = null)
        {
            if (string.IsNullOrEmpty(pushToken))
            {
                _logger.LogWarning("Attempted to send push notification but pushToken is null or empty.");
                return;
            }
            _logger.LogInformation("Sending push notification to token {PushToken}.", pushToken);

            var message = new
            {
                to = pushToken,
                title = title,
                body = body,
                data = data,
                sound = "default",
                priority = "high"
            };

            await SendToExpoAsync(new List<object> { message });
        }

        public async Task SendPushNotificationsAsync(List<string> pushTokens, string title, string body, object? data = null)
        {
            _logger.LogInformation("Sending batch push notifications to {Count} tokens.", pushTokens.Count);
            var messages = pushTokens
                .Where(token => !string.IsNullOrEmpty(token))
                .Select(token => new
                {
                    to = token,
                    title = title,
                    body = body,
                    data = data,
                    sound = "default",
                    priority = "high"
                })
                .ToList<object>();

            if (!messages.Any())
            {
                _logger.LogWarning("No valid push tokens found in batch.");
                return;
            }

            // Expo recommends chunks of 100 messages max
            const int chunkSize = 100;
            for (int i = 0; i < messages.Count; i += chunkSize)
            {
                var chunk = messages.Skip(i).Take(chunkSize).ToList();
                await SendToExpoAsync(chunk);
            }
        }

        private async Task SendToExpoAsync(List<object> messages)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ExpoPushUrl, messages);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully sent {Count} push notifications to Expo.", messages.Count);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send push notifications to Expo. Status: {StatusCode}, Error: {ErrorContent}", response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while sending push notifications to Expo.");
            }
        }
    }
}
