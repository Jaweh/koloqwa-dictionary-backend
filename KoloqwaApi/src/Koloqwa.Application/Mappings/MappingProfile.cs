using AutoMapper;
using Koloqwa.Application.DTOs;
using Koloqwa.Domain.Entities;

namespace Koloqwa.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User
        CreateMap<User, UserDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));

        // Word
        CreateMap<WordEntry, WordSummaryDto>()
            .ForMember(d => d.PartOfSpeech, o => o.MapFrom(s => s.PartOfSpeech.ToString()))
            .ForMember(d => d.LanguageCode, o => o.MapFrom(s => s.Language.Code))
            .ForMember(d => d.LanguageName, o => o.MapFrom(s => s.Language.Name))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.FirstDefinition,
                o => o.MapFrom(s => s.Definitions.OrderBy(d => d.SortOrder)
                                                  .Select(d => d.Definition)
                                                  .FirstOrDefault() ?? string.Empty));

        CreateMap<WordEntry, WordDetailDto>()
            .ForMember(d => d.PartOfSpeech, o => o.MapFrom(s => s.PartOfSpeech.ToString()))
            .ForMember(d => d.LanguageCode, o => o.MapFrom(s => s.Language.Code))
            .ForMember(d => d.LanguageName, o => o.MapFrom(s => s.Language.Name))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<WordDefinition, DefinitionDto>();
        CreateMap<WordExample, ExampleDto>();

        // Phrase
        CreateMap<PhraseEntry, PhraseSummaryDto>()
            .ForMember(d => d.LanguageCode, o => o.MapFrom(s => s.Language.Code))
            .ForMember(d => d.LanguageName, o => o.MapFrom(s => s.Language.Name))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.FirstMeaning,
                o => o.MapFrom(s => s.Meanings.OrderBy(m => m.SortOrder)
                                              .Select(m => m.Meaning)
                                              .FirstOrDefault() ?? string.Empty));

        CreateMap<PhraseEntry, PhraseDetailDto>()
            .ForMember(d => d.LanguageCode, o => o.MapFrom(s => s.Language.Code))
            .ForMember(d => d.LanguageName, o => o.MapFrom(s => s.Language.Name))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<PhraseMeaning, MeaningDto>();

        // Submission
        CreateMap<SubmissionQueue, SubmissionDto>()
            .ForMember(d => d.EntryType, o => o.MapFrom(s => s.EntryType.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.SubmitterName, o => o.MapFrom(s => s.Submitter.DisplayName))
            .ForMember(d => d.SubmitterEmail, o => o.MapFrom(s => s.Submitter.Email))
            .ForMember(d => d.EntryPreview, o => o.MapFrom(s =>
                s.EntryType == Domain.Enums.SubmissionType.Word
                    ? (s.WordEntry != null ? s.WordEntry.Headword : "")
                    : (s.PhraseEntry != null ? s.PhraseEntry.PhraseText : "")));
    }
}
