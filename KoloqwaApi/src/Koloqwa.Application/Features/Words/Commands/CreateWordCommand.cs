using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.Common.Interfaces.Repositories;
using Koloqwa.Application.DTOs;
using Koloqwa.Domain.Entities;
using Koloqwa.Domain.Enums;
using Koloqwa.Domain.Exceptions;
using MediatR;

namespace Koloqwa.Application.Features.Words.Commands;

public record CreateWordCommand(CreateWordRequest Request, Guid SubmitterId) : IRequest<Guid>;

public class CreateWordCommandHandler : IRequestHandler<CreateWordCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly IWordRepository _words;
    private readonly ISlugService _slugs;

    public CreateWordCommandHandler(IApplicationDbContext db, IWordRepository words, ISlugService slugs)
    {
        _db = db; _words = words; _slugs = slugs;
    }

    public async Task<Guid> Handle(CreateWordCommand request, CancellationToken ct)
    {
        var req = request.Request;

        // Block unverified users
        var user = await _db.Users.FindAsync(new object[] { request.SubmitterId }, ct);
        if (user != null && !user.EmailVerified)
            throw new DomainException("Please verify your email address before submitting words.");

        var category = Enum.Parse<EntryCategory>(req.Category, true);

        // Validate language for tribal entries
        if (category == EntryCategory.Tribal)
        {
            if (req.LanguageId is null)
                throw new DomainException("LanguageId is required for Tribal entries.");
            var language = await _db.Languages.FindAsync(new object[] { req.LanguageId }, ct);
            if (language is null)
                throw new NotFoundException(nameof(Language), req.LanguageId);
        }

        var slug = await _slugs.GenerateUniqueAsync(req.Headword,
            async s => await _words.SlugExistsAsync(s, ct));

        var word = new WordEntry
        {
            Category = category,
            LanguageId = category == EntryCategory.Tribal ? req.LanguageId : null,
            Headword = req.Headword.Trim(),
            Slug = slug,
            PartOfSpeech = req.PartOfSpeech,
            Pronunciation = req.Pronunciation,
            Tags = req.Tags ?? new List<string>(),
            Status = EntryStatus.PendingReview,
            SubmittedById = request.SubmitterId,
            CreatedBy = request.SubmitterId
        };

        int order = 0;
        foreach (var def in req.Definitions)
        {
            var definition = new WordDefinition
            {
                Definition = def.Definition.Trim(),
                UsageNote = def.UsageNote,
                Register = def.Register,
                SortOrder = order++
            };
            foreach (var ex in def.Examples ?? Enumerable.Empty<CreateExampleRequest>())
            {
                definition.Examples.Add(new WordExample
                {
                    Sentence = ex.Sentence.Trim(),
                    Translation = ex.Translation
                });
            }
            word.Definitions.Add(definition);
        }

        await _words.AddAsync(word, ct);

        _db.SubmissionQueues.Add(new SubmissionQueue
        {
            SubmitterId = request.SubmitterId,
            EntryType = SubmissionType.Word,
            EntryId = word.Id,
            Status = EntryStatus.PendingReview
        });

        await _words.SaveChangesAsync(ct);
        return word.Id;
    }
}