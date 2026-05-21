using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Domain.Enums;
using Koloqwa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Admin.Commands;

public record DeleteEntryCommand(Guid SubmissionId, Guid AdminId) : IRequest;

public class DeleteEntryCommandHandler : IRequestHandler<DeleteEntryCommand>
{
    private readonly IApplicationDbContext _db;
    public DeleteEntryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteEntryCommand request, CancellationToken ct)
    {
        var submission = await _db.SubmissionQueues
            .FirstOrDefaultAsync(s => s.Id == request.SubmissionId, ct)
            ?? throw new NotFoundException("Submission", request.SubmissionId);

        if (submission.EntryType == SubmissionType.Word)
        {
            var word = await _db.WordEntries.FindAsync(new object[] { submission.EntryId }, ct);
            if (word != null) _db.WordEntries.Remove(word);
        }
        else
        {
            var phrase = await _db.PhraseEntries.FindAsync(new object[] { submission.EntryId }, ct);
            if (phrase != null) _db.PhraseEntries.Remove(phrase);
        }

        _db.AuditLogs.Add(new Domain.Entities.AuditLog
        {
            ActorId = request.AdminId,
            Action = "Entry.Delete",
            EntityType = submission.EntryType.ToString(),
            EntityId = submission.EntryId,
            DiffJson = "Deleted by admin"
        });

        _db.SubmissionQueues.Remove(submission);
        await _db.SaveChangesAsync(ct);
    }
}
