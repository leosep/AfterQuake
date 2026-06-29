using AfterQuake.Domain.Common;
using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Domain.Entities;

public class Donation : BaseAuditableEntity
{
    public DonationType DonationType { get; set; }
    public DonationStatus Status { get; set; } = DonationStatus.Pending;
    public decimal? MonetaryAmount { get; set; }
    public string? Currency { get; set; } = "MXN";
    public string? ItemName { get; set; }
    public int? ItemQuantity { get; set; }
    public string? ItemUnit { get; set; }
    public string? Description { get; set; }
    public string? DonorName { get; set; }
    public bool IsAnonymous { get; set; }
    public string? PaymentReference { get; set; }
    public DateTime DonatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DistributedAt { get; set; }
    public string? DistributionNotes { get; set; }

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public Guid? DonationPointId { get; set; }
    public DonationPoint? DonationPoint { get; set; }
}
