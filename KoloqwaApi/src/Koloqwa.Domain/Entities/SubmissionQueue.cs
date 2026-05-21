using Koloqwa.Domain.Common;
using Koloqwa.Domain.Enums;

namespace Koloqwa.Domain.Entities;

public class SubmissionQueue : BaseEntity
{
    public Guid SubmitterId { get; set; }
    public SubmissionType EntryType { get; set; }
    public Guid EntryId { get; set; }
    public EntryStatus Status { get; set; } = EntryStatus.PendingReview;
    public string? AdminNote { get; set; }
    public Guid? ReviewedById { get; set; }
    public DateTime? ReviewedAt { get; set; }

    // Navigation
    public User Submitter { get; set; } = null!;
    public User? ReviewedBy { get; set; }

    // Soft-resolved navigation (populated by service layer based on EntryType)
    public WordEntry? WordEntry { get; set; }
    public PhraseEntry? PhraseEntry { get; set; }
}
