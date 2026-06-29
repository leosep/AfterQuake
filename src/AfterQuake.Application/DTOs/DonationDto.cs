using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Application.DTOs;

public class DonationDto
{
    public Guid Id { get; set; }
    public string? DonationTypeName { get; set; }
    public DonationType DonationType { get; set; }
    public string? StatusName { get; set; }
    public DonationStatus Status { get; set; }
    public decimal? MonetaryAmount { get; set; }
    public string? ItemName { get; set; }
    public int? ItemQuantity { get; set; }
    public string? DonorName { get; set; }
    public bool IsAnonymous { get; set; }
    public DateTime DonatedAt { get; set; }
    public string? DonationPointName { get; set; }
}

public class CreateDonationDto
{
    public DonationType DonationType { get; set; }
    public decimal? MonetaryAmount { get; set; }
    public string? ItemName { get; set; }
    public int? ItemQuantity { get; set; }
    public string? ItemUnit { get; set; }
    public string? Description { get; set; }
    public string? DonorName { get; set; }
    public bool IsAnonymous { get; set; }
    public Guid? DonationPointId { get; set; }
}
