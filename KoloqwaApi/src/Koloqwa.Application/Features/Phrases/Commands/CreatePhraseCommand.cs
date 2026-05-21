using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.Common.Interfaces.Repositories;
using Koloqwa.Application.DTOs;
using Koloqwa.Domain.Entities;
using Koloqwa.Domain.Enums;
using Koloqwa.Domain.Exceptions;
using MediatR;

namespace Koloqwa.Application.Features.Phrases.Commands;

public record CreatePhraseCommand(CreatePhraseRequest Request, Guid SubmitterId) : IRequest<Guid>;

public class CreatePhraseCommandHandler : IRequestHandler<CreatePhraseCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly IPhraseRepository _phrases;
    private readonly ISlugService _slugs;

    public CreatePhraseCommandHandler(
        IApplicationDbContext db, IPhraseRepository phrases, ISlugService slugs)
    {
        _db = db; _phrases = phrases; _slugs = slugs;
    }

    public async Task<Guid> Handle(CreatePhraseCommand request, CancellationToken ct)
    {
        var req = request.Request;

        var language = await _db.Languages.FindAsync(new object[] { req.LanguageId }, ct);
        if (language is null)
            throw new NotFoundException(nameof(Language), req.LanguageId);

        var slug = await _slugs.GenerateUniqueAsync(req.PhraseText,
            async s => await _phrases.SlugExistsAsync(s, ct));

        var phrase = new PhraseEntry
        {
            LanguageId = req.LanguageId,
            PhraseText = req.PhraseText.Trim(),
            Slug = slug,
            LiteralMeaning = req.LiteralMeaning,
            Tags = req.Tags ?? new List<string>(),
            Status = EntryStatus.PendingReview,
            SubmittedById = request.SubmitterId,
            CreatedBy = request.SubmitterId
        };

        int order = 0;
        foreach (var m in req.Meanings)
        {
            phrase.Meanings.Add(new PhraseMeaning
            {
                Meaning = m.Meaning.Trim(),
                ContextNote = m.ContextNote,
                SortOrder = order++
            });
        }

        await _phrases.AddAsync(phrase, ct);

        _db.SubmissionQueues.Add(new SubmissionQueue
        {
            SubmitterId = request.SubmitterId,
            EntryType = SubmissionType.Phrase,
            EntryId = phrase.Id,
            Status = EntryStatus.PendingReview
        });

        await _phrases.SaveChangesAsync(ct);
        return phrase.Id;
    }
}
