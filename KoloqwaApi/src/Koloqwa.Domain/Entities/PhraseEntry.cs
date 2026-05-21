using Koloqwa.Domain.Common;
using Koloqwa.Domain.Enums;

namespace Koloqwa.Domain.Entities;

public class PhraseEntry : AuditableEntity
{
    public EntryCategory Category { get; set; } = EntryCategory.Vernacular;
    public Guid? LanguageId { get; set; }          // null for Vernacular entries
    public string PhraseText { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LiteralMeaning { get; set; }
    public List<string> Tags { get; set; } = new();
    public EntryStatus Status { get; set; } = EntryStatus.PendingReview;
    public Guid? SubmittedById { get; set; }
    public Guid? ReviewedById { get; set; }
    public DateTime? PublishedAt { get; set; }

    // Navigation
    public Language? Language { get; set; }
    public User? SubmittedBy { get; set; }
    public User? ReviewedBy { get; set; }
    public ICollection<PhraseMeaning> Meanings { get; set; } = new List<PhraseMeaning>();
    public ICollection<SubmissionQueue> Submissions { get; set; } = new List<SubmissionQueue>();
}
