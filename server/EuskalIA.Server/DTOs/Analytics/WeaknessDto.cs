namespace EuskalIA.Server.DTOs.Analytics
{
    /// <summary>
    /// Represents a user's weakness in a particular topic.
    /// </summary>
    public class WeaknessDto
    {
        public string Topic { get; set; } = string.Empty;
        public int FailureCount { get; set; }
        public int TotalAttempts { get; set; }
        public double FailureRate => TotalAttempts > 0 ? (double)FailureCount / TotalAttempts : 0;
    }
}
