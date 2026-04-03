namespace EuskalIA.Server.DTOs.Srs
{
    /// <summary>
    /// Data Transfer Object for recording the result of an SRS review attempt.
    /// </summary>
    public class SrsReviewResultDto
    {
        /// <summary>Gets or sets the user ID.</summary>
        public int UserId { get; set; }
        /// <summary>Gets or sets the reviewed topic.</summary>
        public string Topic { get; set; } = string.Empty;
        /// <summary>Gets or sets whether the attempt was correct.</summary>
        public bool IsCorrect { get; set; }
    }
}
