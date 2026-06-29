using AfterQuake.Domain.Common;
using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Domain.Entities;

public class ServiceStatus : BaseAuditableEntity
{
    public ServiceType ServiceType { get; set; }
    public ServiceStatusType StatusType { get; set; } = ServiceStatusType.Operational;
    public string? ZoneCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Description { get; set; }
    public string? EstimatedRestorationTime { get; set; }
    public string? AffectedAreas { get; set; }
    public bool IsEmergencyService { get; set; }

    public ApplicationUser? UpdatedBy { get; set; }
}
