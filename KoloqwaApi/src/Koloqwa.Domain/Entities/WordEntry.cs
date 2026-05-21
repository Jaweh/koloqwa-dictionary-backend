using Koloqwa.Domain.Common;
using Koloqwa.Domain.Enums;

namespace Koloqwa.Domain.Entities;

public class WordEntry : AuditableEntity
{
    public Guid LanguageId { get; set; }
    public string Headword { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public PartOfSpeech PartOfSpeech { get; set; }
    public string? Pronunciation { get; set; }
    public string? AudioUrl { get; set; }
    public List<string> Tags { get; set; } = new();
    public EntryStatus Status { get; set; } = EntryStatus.PendingReview;
    public Guid? SubmittedById { get; set; }
    public Guid? ReviewedById { get; set; }
    public DateTime? PublishedAt { get; set; }

    // Navigation
    public Language Language { get; set; } = null!;
    public User? SubmittedBy { get; set; }
    public User? ReviewedBy { get; set; }
    public ICollection<WordDefinition> Definitions { get; set; } = new List<WordDefinition>();
    public ICollection<SubmissionQueue> Submissions { get; set; } = new List<SubmissionQueue>();
}
