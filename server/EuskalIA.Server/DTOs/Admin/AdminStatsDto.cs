namespace EuskalIA.Server.DTOs.Admin
{
    /// <summary>
    /// Global statistics for the administrative dashboard.
    /// </summary>
    public class AdminStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int RegistrationsToday { get; set; }
    }
}
