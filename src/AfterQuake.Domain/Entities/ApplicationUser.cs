using Microsoft.AspNetCore.Identity;
using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? DocumentId { get; set; }
    public string? EmergencyContact { get; set; }
    public double? LastLatitude { get; set; }
    public double? LastLongitude { get; set; }
    public DateTime? LastLocationUpdate { get; set; }
    public bool HasVerifiedEmergency { get; set; }
    public string? PreferredLanguage { get; set; } = "es";
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<EmergencyReport> EmergencyReports { get; set; } = new List<EmergencyReport>();
    public ICollection<PersonReport> PersonReports { get; set; } = new List<PersonReport>();
    public ICollection<HelpRequest> HelpRequests { get; set; } = new List<HelpRequest>();
    public ICollection<HelpOffer> HelpOffers { get; set; } = new List<HelpOffer>();
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public Volunteer? Volunteer { get; set; }
}
