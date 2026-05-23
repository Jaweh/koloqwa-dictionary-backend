using Koloqwa.Domain.Common;

namespace Koloqwa.Domain.Entities;

public class WordReport : AuditableEntity
{
    public Guid EntryId { get; set; }
    public string EntryType { get; set; } = string.Empty;
    public Guid ReportedById { get; set; }
    public string Reason { get; set; } = string.Empty; // "Offensive", "IncorrectMeaning", "Spam", "Other"
    public string? Notes { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Reviewed, Dismissed
    public Guid? ReviewedById { get; set; }
    public DateTime? ReviewedAt { get; set; }

    // Navigation
    public User ReportedBy { get; set; } = null!;
    public User? ReviewedBy { get; set; }
}
