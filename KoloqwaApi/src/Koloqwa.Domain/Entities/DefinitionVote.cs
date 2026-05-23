using Koloqwa.Domain.Common;

namespace Koloqwa.Domain.Entities;

public class DefinitionVote : BaseEntity
{
    public Guid DefinitionId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public WordDefinition Definition { get; set; } = null!;
    public User User { get; set; } = null!;
}
