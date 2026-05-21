using Koloqwa.Domain.Common;

namespace Koloqwa.Domain.Entities;

public class WordDefinition : BaseEntity
{
    public Guid WordEntryId { get; set; }
    public int SortOrder { get; set; }
    public string Definition { get; set; } = string.Empty;
    public string? UsageNote { get; set; }
    public string? Register { get; set; } // formal, informal, slang, etc.

    // Navigation
    public WordEntry WordEntry { get; set; } = null!;
    public ICollection<WordExample> Examples { get; set; } = new List<WordExample>();
}
