using AfterQuake.Domain.Common;
using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Domain.Entities;

public class HelpOffer : BaseAuditableEntity
{
    public HelpOfferType OfferType { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; } = 1;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public string? ZoneCode { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime OfferedAt { get; set; } = DateTime.UtcNow;

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
