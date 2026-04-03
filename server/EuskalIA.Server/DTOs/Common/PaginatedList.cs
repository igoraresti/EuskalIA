namespace EuskalIA.Server.DTOs.Common
{
    /// <summary>
    /// Generic container for paginated response data.
    /// </summary>
    /// <typeparam name="T">The type of the paginated items.</typeparam>
    public class PaginatedList<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
