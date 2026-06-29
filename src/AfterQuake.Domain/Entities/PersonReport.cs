using AfterQuake.Domain.Common;
using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Domain.Entities;

public class PersonReport : BaseAuditableEntity
{
    public PersonReportType ReportType { get; set; }
    public PersonReportStatus Status { get; set; } = PersonReportStatus.Active;

    public string? MissingPersonName { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? Description { get; set; }
    public string? PhysicalCharacteristics { get; set; }
    public string? LastKnownClothing { get; set; }
    public string? PhotoUrl { get; set; }
    public double? LastKnownLatitude { get; set; }
    public double? LastKnownLongitude { get; set; }
    public string? LastKnownAddress { get; set; }
    public string? ZoneCode { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }

    public string? FoundByName { get; set; }
    public string? FoundByPhone { get; set; }
    public double? FoundLatitude { get; set; }
    public double? FoundLongitude { get; set; }
    public DateTime? FoundAt { get; set; }
    public string? FoundNotes { get; set; }

    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public Guid? MatchedToReportId { get; set; }
    public PersonReport? MatchedToReport { get; set; }
    public ICollection<PersonReport> PotentialMatches { get; set; } = new List<PersonReport>();
}
