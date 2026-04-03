namespace EuskalIA.Server.DTOs.Admin
{
    /// <summary>
    /// Request model for bulk updating the status of multiple exercises.
    /// </summary>
    /// <param name="Ids">List of unique exercise identifiers.</param>
    /// <param name="Status">The new moderation status to apply.</param>
    public record BulkStatusRequest(List<Guid> Ids, string Status);
}
