using AfterQuake.Domain.Common;
using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Domain.Entities;

public class DisasterZone : BaseAuditableEntity
{
    public string? ZoneCode { get; set; }
    public string? Name { get; set; }
    public string? Region { get; set; }
    public AlertLevel CurrentAlertLevel { get; set; } = AlertLevel.Green;
    public double? CenterLatitude { get; set; }
    public double? CenterLongitude { get; set; }
    public double? RadiusKm { get; set; }
    public string? BoundariesGeoJson { get; set; }
    public bool IsActive { get; set; }
    public int EstimatedPopulation { get; set; }
    public int ConfirmedCasualties { get; set; }
    public int EstimatedAffected { get; set; }
    public string? Description { get; set; }
    
}
