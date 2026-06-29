namespace AfterQuake.Web.Models;

public class ProfileViewModel
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? DocumentId { get; set; }
    public string? EmergencyContact { get; set; }
    public string PreferredLanguage { get; set; } = "es";
    public bool TwoFactorEnabled { get; set; }
    public bool PasswordExpired { get; set; }
}
