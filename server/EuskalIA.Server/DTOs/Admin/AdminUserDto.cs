namespace EuskalIA.Server.DTOs.Admin
{
    /// <summary>
    /// User profile data specifically for administrative views.
    /// </summary>
    public class AdminUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public DateTime JoinedAt { get; set; }
        public int XP { get; set; }
    }
}
