using Koloqwa.Domain.Common;

namespace Koloqwa.Domain.Entities;

public class PhraseMeaning : BaseEntity
{
    public Guid PhraseEntryId { get; set; }
    public int SortOrder { get; set; }
    public string Meaning { get; set; } = string.Empty;
    public string? ContextNote { get; set; }

    // Navigation
    public PhraseEntry PhraseEntry { get; set; } = null!;
}
