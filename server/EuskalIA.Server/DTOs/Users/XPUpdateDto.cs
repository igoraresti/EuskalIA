namespace EuskalIA.Server.DTOs.Users
{
    /// <summary>
    /// Data Transfer Object for reporting XP gains after completing a lesson or exercise.
    /// </summary>
    public class XPUpdateDto
    {
        public int XP { get; set; }
        public string? LessonTitle { get; set; }
    }
}
