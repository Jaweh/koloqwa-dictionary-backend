using Koloqwa.Domain.Common;

namespace Koloqwa.Domain.Entities;

public class WordSuggestion : AuditableEntity
{
    public Guid EntryId { get; set; }
    public string EntryType { get; set; } = string.Empty; // "Word" or "Phrase"
    public Guid SuggestedById { get; set; }
    public string Field { get; set; } = string.Empty;  // e.g. "Headword", "Definition"
    public string CurrentValue { get; set; } = string.Empty;
    public string SuggestedValue { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected
    public string? AdminNote { get; set; }
    public Guid? ReviewedById { get; set; }
    public DateTime? ReviewedAt { get; set; }

    // Navigation
    public User SuggestedBy { get; set; } = null!;
    public User? ReviewedBy { get; set; }
}
