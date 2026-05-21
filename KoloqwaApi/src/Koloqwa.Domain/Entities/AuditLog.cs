using Koloqwa.Domain.Common;

namespace Koloqwa.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? ActorId { get; set; }
    public string Action { get; set; } = string.Empty;       // e.g. "Approved", "Rejected", "Edited"
    public string EntityType { get; set; } = string.Empty;   // e.g. "WordEntry"
    public Guid EntityId { get; set; }
    public string? DiffJson { get; set; }                    // JSON snapshot of before/after
    public string? IpAddress { get; set; }

    // Navigation
    public User? Actor { get; set; }
}
