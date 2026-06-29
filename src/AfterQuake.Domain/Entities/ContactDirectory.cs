using AfterQuake.Domain.Common;

namespace AfterQuake.Domain.Entities;

public class ContactDirectory : BaseAuditableEntity
{
    public string? OrganizationName { get; set; }
    public string? OrganizationType { get; set; }
    public string? ContactPerson { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AlternativePhone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ZoneCode { get; set; }
    public string? OperatingHours { get; set; }
    public bool IsAvailable24Hours { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Services { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsEmergencyNumber { get; set; }

    public ApplicationUser? UpdatedBy { get; set; }
}
