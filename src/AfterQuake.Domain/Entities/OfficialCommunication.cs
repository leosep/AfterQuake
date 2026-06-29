using AfterQuake.Domain.Common;

namespace AfterQuake.Domain.Entities;

public class OfficialCommunication : BaseAuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public CommunicationSeverity Severity { get; set; } = CommunicationSeverity.Info;
    public CommunicationStatus Status { get; set; } = CommunicationStatus.Draft;
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? SignedBy { get; set; }
    public string? DigitalSignature { get; set; }
    public string? PdfUrl { get; set; }

    public string? PublishedById { get; set; }
    public ApplicationUser? PublishedBy { get; set; }
}

public enum CommunicationSeverity { Info, Important, Urgent, Critical }
public enum CommunicationStatus { Draft, Published, Archived }
