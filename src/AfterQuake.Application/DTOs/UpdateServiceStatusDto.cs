using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Application.DTOs;

public class UpdateServiceStatusDto
{
    public ServiceType ServiceType { get; set; }
    public ServiceStatusType StatusType { get; set; }
    public string? ZoneCode { get; set; }
    public string? Description { get; set; }
}
