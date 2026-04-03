namespace EuskalIA.Server.DTOs.Users
{
    /// <summary>
    /// Data Transfer Object for updating a user's interface language preference.
    /// </summary>
    public class UpdateLanguageDto
    {
        public string Language { get; set; } = string.Empty;
    }
}
