namespace EuskalIA.Server.Services.Notifications
{
    public interface INotificationService
    {
        Task SendPushNotificationAsync(string pushToken, string title, string body, object? data = null);
        Task SendPushNotificationsAsync(List<string> pushTokens, string title, string body, object? data = null);
    }

    public class ExpoPushMessage
    {
        public string? To { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public object? Data { get; set; }
        public string Sound { get; set; } = "default";
        public string Priority { get; set; } = "high";
    }
}
