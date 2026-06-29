using AfterQuake.Domain.Common;

namespace AfterQuake.Domain.Entities;

public class GuideContent : BaseAuditableEntity
{
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public string? IconClass { get; set; }
    public bool IsPdfAvailable { get; set; }
    public string? PdfUrl { get; set; }
    public string? VideoUrl { get; set; }
    public bool RequiresAuth { get; set; }
    public bool IsPublished { get; set; } = true;
    public int DisplayOrder { get; set; }
    public string? Language { get; set; } = "es";

    public string? PublishedById { get; set; }
    public ApplicationUser? PublishedBy { get; set; }
}
