using AfterQuake.Domain.Common;

namespace AfterQuake.Domain.Entities;

public class DonationPoint : BaseAuditableEntity
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public string? ZoneCode { get; set; }
    public string? ContactPhone { get; set; }
    public string? OperatingHours { get; set; }
    public bool IsActive { get; set; } = true;
    public string? NeededItems { get; set; }
    public string? UrgentlyNeededItems { get; set; }

    public string? ManagedById { get; set; }
    public ApplicationUser? ManagedBy { get; set; }

    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
}
