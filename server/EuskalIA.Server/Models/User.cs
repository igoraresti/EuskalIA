using System;

namespace EuskalIA.Server.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public string? DeletionCode { get; set; }
        public DateTime? CodeExpiration { get; set; }
        
        // Email verification fields
        public bool IsVerified { get; set; } = false;
        public string? VerificationToken { get; set; }
        public DateTime? TokenExpiration { get; set; }
        
        // Language preference
        public string Language { get; set; } = "es"; // Default: Spanish
        
        // Deactivation fields
        public bool IsActive { get; set; } = true;
        public string? DeactivationToken { get; set; }
        public DateTime? DeactivationTokenExpiration { get; set; }
        
        // Role-based access
        public string Role { get; set; } = "User";
        
        // Push notification token for Expo
        public string? ExpoPushToken { get; set; }
        
        // Navigation property
        public Progress? Progress { get; set; }
        
        // Navigation property for SRS nodes
        public ICollection<UserSrsNode> SrsNodes { get; set; } = new List<UserSrsNode>();
        
        // Navigation property for exercise attempts
        public ICollection<UserExerciseAttempt> ExerciseAttempts { get; set; } = new List<UserExerciseAttempt>();
        
        // Navigation property for achievements
        public ICollection<UserAchievement> Achievements { get; set; } = new List<UserAchievement>();
    }
}
