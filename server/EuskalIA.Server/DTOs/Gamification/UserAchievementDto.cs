namespace EuskalIA.Server.DTOs.Gamification
{
    /// <summary>
    /// Data Transfer Object representing an achievement earned by a user.
    /// </summary>
    public class UserAchievementDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int TargetValue { get; set; }
        public bool IsEarned { get; set; }
        public DateTime? EarnedAt { get; set; }
    }
}
