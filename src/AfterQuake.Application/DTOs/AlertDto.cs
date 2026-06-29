using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Application.DTOs;

public class AlertDto
{
    public Guid Id { get; set; }
    public string? AlertTypeName { get; set; }
    public AlertType AlertType { get; set; }
    public string? SeverityName { get; set; }
    public AlertLevel Severity { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ZoneCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class CreateAlertDto
{
    public AlertType AlertType { get; set; }
    public AlertLevel Severity { get; set; } = AlertLevel.Yellow;
    public string? Title { get; set; }
    public string? Message { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ZoneCode { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool RequiresAcknowledgement { get; set; }
}
