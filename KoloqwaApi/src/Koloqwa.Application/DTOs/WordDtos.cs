using System.ComponentModel.DataAnnotations;
using Koloqwa.Domain.Enums;

namespace Koloqwa.Application.DTOs;

public record CreateWordRequest(
    [Required] Guid LanguageId,
    [Required][MinLength(1)][MaxLength(200)] string Headword,
    [Required] PartOfSpeech PartOfSpeech,
    string? Pronunciation,
    List<string>? Tags,
    [Required][MinLength(1)] List<CreateDefinitionRequest> Definitions
);

public record CreateDefinitionRequest(
    [Required][MinLength(5)] string Definition,
    string? UsageNote,
    string? Register,
    List<CreateExampleRequest>? Examples
);

public record CreateExampleRequest(
    [Required] string Sentence,
    string? Translation
);

public record UpdateWordRequest(
    Guid? LanguageId,
    string? Headword,
    PartOfSpeech? PartOfSpeech,
    string? Pronunciation,
    List<string>? Tags
);

public record WordSummaryDto(
    Guid Id,
    string Headword,
    string Slug,
    string PartOfSpeech,
    string LanguageCode,
    string LanguageName,
    string FirstDefinition,
    string Status,
    DateTime? PublishedAt
);

public record WordDetailDto(
    Guid Id,
    string Headword,
    string Slug,
    string PartOfSpeech,
    string? Pronunciation,
    string? AudioUrl,
    List<string> Tags,
    string Status,
    string LanguageCode,
    string LanguageName,
    List<DefinitionDto> Definitions,
    DateTime? PublishedAt,
    DateTime CreatedAt
);

public record DefinitionDto(
    Guid Id,
    int SortOrder,
    string Definition,
    string? UsageNote,
    string? Register,
    List<ExampleDto> Examples
);

public record ExampleDto(Guid Id, string Sentence, string? Translation);
