using EuskalIA.Server.DTOs.Notifications;

namespace EuskalIA.Server.Services.Notifications
{
    public interface INotificationService
    {
        Task SendPushNotificationAsync(string pushToken, string title, string body, object? data = null);
        Task SendPushNotificationsAsync(List<string> pushTokens, string title, string body, object? data = null);
    }
}
