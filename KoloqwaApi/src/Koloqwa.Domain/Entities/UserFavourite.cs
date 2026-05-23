using Koloqwa.Domain.Common;

namespace Koloqwa.Domain.Entities;

public class UserFavourite : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid EntryId { get; set; }
    public string EntryType { get; set; } = string.Empty; // "Word" or "Phrase"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
}
