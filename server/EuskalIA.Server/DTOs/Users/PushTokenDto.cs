namespace EuskalIA.Server.DTOs.Users
{
    /// <summary>
    /// Data Transfer Object for updating a user's push token.
    /// </summary>
    public class PushTokenDto
    {
        public string Token { get; set; } = string.Empty;
    }
}
