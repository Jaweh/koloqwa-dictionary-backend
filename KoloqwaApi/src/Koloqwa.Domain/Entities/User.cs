using Koloqwa.Domain.Common;
using Koloqwa.Domain.Enums;

namespace Koloqwa.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;
    public bool EmailVerified { get; set; } = false;
    public DateTime? EmailVerifiedAt { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationExpiry { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpiry { get; set; }

    // Navigation
    public ICollection<WordEntry> SubmittedWords { get; set; } = new List<WordEntry>();
    public ICollection<PhraseEntry> SubmittedPhrases { get; set; } = new List<PhraseEntry>();
    public ICollection<SubmissionQueue> Submissions { get; set; } = new List<SubmissionQueue>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}