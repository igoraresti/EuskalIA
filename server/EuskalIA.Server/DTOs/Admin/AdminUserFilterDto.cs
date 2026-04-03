namespace EuskalIA.Server.DTOs.Admin
{
    /// <summary>
    /// Filtering and pagination parameters for the administrative user list.
    /// </summary>
    public class AdminUserFilterDto
    {
        /// <summary>Gets or sets the current page number (1-indexed).</summary>
        public int Page { get; set; } = 1;
        /// <summary>Gets or sets the number of items per page.</summary>
        public int PageSize { get; set; } = 10;
        /// <summary>Gets or sets an optional search string (username).</summary>
        public string? Search { get; set; }
        /// <summary>Gets or sets an optional status filter.</summary>
        public bool? IsActive { get; set; }
        /// <summary>Gets or sets an optional filter for registration start date.</summary>
        public DateTime? JoinedFrom { get; set; }
        /// <summary>Gets or sets an optional filter for registration end date.</summary>
        public DateTime? JoinedTo { get; set; }
    }
}
