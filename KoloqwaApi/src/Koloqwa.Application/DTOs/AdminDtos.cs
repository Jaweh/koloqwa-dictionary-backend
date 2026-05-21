namespace Koloqwa.Application.DTOs;

public record AdminSubmissionDto(
    Guid Id,
    string EntryType,
    Guid EntryId,
    string EntryPreview,
    string Status,
    string SubmitterName,
    string SubmitterEmail,
    string? AdminNote,
    string? Category,
    string? LanguageName,
    DateTime SubmittedAt,
    DateTime? ReviewedAt,
    string? ReviewedByName
);

public record ReviewSubmissionRequest(
    string Action,          // "Approve" or "Reject"
    string? AdminNote
);

public record EditWordEntryRequest(
    string? Headword,
    string? Pronunciation,
    string? PartOfSpeech,
    List<string>? Tags,
    string? Definition,
    string? UsageNote
);

public record EditPhraseEntryRequest(
    string? PhraseText,
    string? LiteralMeaning,
    List<string>? Tags,
    string? Meaning,
    string? ContextNote
);

public record AdminUserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string Role,
    bool IsActive,
    int SubmissionCount,
    int ApprovedCount,
    DateTime CreatedAt
);

public record UpdateUserRoleRequest(string Role);
public record ToggleUserActiveRequest(bool IsActive);

public record AdminStatsDto(
    int TotalWords,
    int TotalPhrases,
    int PendingReview,
    int ApprovedTotal,
    int RejectedTotal,
    int TotalUsers,
    int ActiveUsers
);
