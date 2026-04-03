namespace EuskalIA.Server.DTOs.Notifications
{
    /// <summary>
    /// Data Transfer Object for sending push notifications via Expo.
    /// </summary>
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
