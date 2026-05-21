using Koloqwa.Domain.Common;

namespace Koloqwa.Domain.Entities;

public class WordExample : BaseEntity
{
    public Guid WordDefinitionId { get; set; }
    public string Sentence { get; set; } = string.Empty;
    public string? Translation { get; set; }

    // Navigation
    public WordDefinition WordDefinition { get; set; } = null!;
}
