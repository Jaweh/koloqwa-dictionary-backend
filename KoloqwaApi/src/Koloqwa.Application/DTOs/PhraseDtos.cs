using System.ComponentModel.DataAnnotations;

namespace Koloqwa.Application.DTOs;

public record CreatePhraseRequest(
    [Required] string Category,            // "Vernacular" or "Tribal"
    Guid? LanguageId,                      // Required when Category = Tribal
    [Required][MinLength(2)][MaxLength(500)] string PhraseText,
    string? LiteralMeaning,
    List<string>? Tags,
    [Required][MinLength(1)] List<CreateMeaningRequest> Meanings
);

public record CreateMeaningRequest(
    [Required][MinLength(5)] string Meaning,
    string? ContextNote
);

public record UpdatePhraseRequest(
    string? Category,
    Guid? LanguageId,
    string? PhraseText,
    string? LiteralMeaning,
    List<string>? Tags
);

public record PhraseSummaryDto(
    Guid Id,
    string PhraseText,
    string Slug,
    string Category,
    string? LanguageCode,
    string? LanguageName,
    string FirstMeaning,
    string Status,
    DateTime? PublishedAt
);

public record PhraseDetailDto(
    Guid Id,
    string PhraseText,
    string Slug,
    string? LiteralMeaning,
    List<string> Tags,
    string Category,
    string Status,
    string? LanguageCode,
    string? LanguageName,
    List<MeaningDto> Meanings,
    DateTime? PublishedAt,
    DateTime CreatedAt
);

public record MeaningDto(Guid Id, int SortOrder, string Meaning, string? ContextNote);
