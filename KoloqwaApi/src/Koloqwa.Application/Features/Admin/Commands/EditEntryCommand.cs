using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.DTOs;
using Koloqwa.Domain.Enums;
using Koloqwa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Admin.Commands;

public record EditWordEntryCommand(Guid WordId, EditWordEntryRequest Request, Guid AdminId) : IRequest;
public record EditPhraseEntryCommand(Guid PhraseId, EditPhraseEntryRequest Request, Guid AdminId) : IRequest;

public class EditWordEntryCommandHandler : IRequestHandler<EditWordEntryCommand>
{
    private readonly IApplicationDbContext _db;
    public EditWordEntryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(EditWordEntryCommand request, CancellationToken ct)
    {
        var word = await _db.WordEntries
            .Include(w => w.Definitions)
            .FirstOrDefaultAsync(w => w.Id == request.WordId, ct)
            ?? throw new NotFoundException("WordEntry", request.WordId);

        var req = request.Request;
        if (req.Headword != null) word.Headword = req.Headword.Trim();
        if (req.Pronunciation != null) word.Pronunciation = req.Pronunciation.Trim();
        if (req.Tags != null) word.Tags = req.Tags;
        if (req.PartOfSpeech != null && Enum.TryParse<PartOfSpeech>(req.PartOfSpeech, true, out var pos))
            word.PartOfSpeech = pos;

        if (req.Definition != null && word.Definitions.Any())
        {
            var def = word.Definitions.OrderBy(d => d.SortOrder).First();
            def.Definition = req.Definition.Trim();
            if (req.UsageNote != null) def.UsageNote = req.UsageNote.Trim();
        }

        word.UpdatedAt = DateTime.UtcNow;
        word.UpdatedBy = request.AdminId;

        _db.AuditLogs.Add(new Domain.Entities.AuditLog
        {
            ActorId = request.AdminId,
            Action = "WordEntry.Edit",
            EntityType = "Word",
            EntityId = word.Id,
            DiffJson = "Edited by admin"
        });

        await _db.SaveChangesAsync(ct);
    }
}

public class EditPhraseEntryCommandHandler : IRequestHandler<EditPhraseEntryCommand>
{
    private readonly IApplicationDbContext _db;
    public EditPhraseEntryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(EditPhraseEntryCommand request, CancellationToken ct)
    {
        var phrase = await _db.PhraseEntries
            .Include(p => p.Meanings)
            .FirstOrDefaultAsync(p => p.Id == request.PhraseId, ct)
            ?? throw new NotFoundException("PhraseEntry", request.PhraseId);

        var req = request.Request;
        if (req.PhraseText != null) phrase.PhraseText = req.PhraseText.Trim();
        if (req.LiteralMeaning != null) phrase.LiteralMeaning = req.LiteralMeaning.Trim();
        if (req.Tags != null) phrase.Tags = req.Tags;

        if (req.Meaning != null && phrase.Meanings.Any())
        {
            var meaning = phrase.Meanings.OrderBy(m => m.SortOrder).First();
            meaning.Meaning = req.Meaning.Trim();
            if (req.ContextNote != null) meaning.ContextNote = req.ContextNote.Trim();
        }

        phrase.UpdatedAt = DateTime.UtcNow;
        phrase.UpdatedBy = request.AdminId;

        _db.AuditLogs.Add(new Domain.Entities.AuditLog
        {
            ActorId = request.AdminId,
            Action = "PhraseEntry.Edit",
            EntityType = "Phrase",
            EntityId = phrase.Id,
            DiffJson = "Edited by admin"
        });

        await _db.SaveChangesAsync(ct);
    }
}
