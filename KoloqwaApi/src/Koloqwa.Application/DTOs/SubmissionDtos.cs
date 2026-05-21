using Koloqwa.Domain.Enums;

namespace Koloqwa.Application.DTOs;

public record SubmissionDto(
    Guid Id,
    string EntryType,
    Guid EntryId,
    string EntryPreview,
    string Status,
    string SubmitterName,
    string SubmitterEmail,
    string? AdminNote,
    DateTime SubmittedAt,
    DateTime? ReviewedAt
);

public record EditBeforePublishRequest(
    string? Headword,       // for words
    string? PhraseText,     // for phrases
    string? Pronunciation,
    string? LiteralMeaning,
    string? AdminNote
);
