namespace AfterQuake.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public abstract class BaseAuditableEntity : BaseEntity
{
    public string? CreatedById { get; set; }
    public string? UpdatedById { get; set; }
    public string? DeletedById { get; set; }
}
