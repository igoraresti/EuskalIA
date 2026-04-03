namespace EuskalIA.Server.DTOs.Users
{
    /// <summary>
    /// Data Transfer Object for updating a user's profile details.
    /// All fields are optional.
    /// </summary>
    public class ProfileUpdateDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Nickname { get; set; }
        public string? Password { get; set; }
    }
}
