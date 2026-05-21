using Koloqwa.Domain.Common;

namespace Koloqwa.Domain.Entities;

public class Language : BaseEntity
{
    public string Code { get; set; } = string.Empty;   // e.g. "kpe" (Kpelle)
    public string Name { get; set; } = string.Empty;   // e.g. "Kpelle"
    public string Region { get; set; } = string.Empty; // e.g. "Central Liberia"
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<WordEntry> Words { get; set; } = new List<WordEntry>();
    public ICollection<PhraseEntry> Phrases { get; set; } = new List<PhraseEntry>();
}
