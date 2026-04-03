namespace EuskalIA.Server.DTOs.Srs
{
    /// <summary>
    /// Data Transfer Object for signaling lesson completion.
    /// </summary>
    public class LessonCompletionDto
    {
        /// <summary>Gets or sets the user ID.</summary>
        public int UserId { get; set; }
        /// <summary>Gets or sets the identifier of the completed lesson.</summary>
        public int LessonId { get; set; }
    }
}
