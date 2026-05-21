using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Domain.Enums;
using Koloqwa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Admin.Commands;

public record ReviewSubmissionCommand(
    Guid SubmissionId,
    string Action,
    string? AdminNote,
    Guid ReviewerId
) : IRequest;

public class ReviewSubmissionCommandHandler : IRequestHandler<ReviewSubmissionCommand>
{
    private readonly IApplicationDbContext _db;
    public ReviewSubmissionCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(ReviewSubmissionCommand request, CancellationToken ct)
    {
        var submission = await _db.SubmissionQueues
            .FirstOrDefaultAsync(s => s.Id == request.SubmissionId, ct)
            ?? throw new NotFoundException("Submission", request.SubmissionId);

        if (submission.Status != EntryStatus.PendingReview)
            throw new DomainException("Only pending submissions can be reviewed.");

        var newStatus = request.Action.ToLower() == "approve"
            ? EntryStatus.Approved
            : EntryStatus.Rejected;

        submission.Status = newStatus;
        submission.AdminNote = request.AdminNote;
        submission.ReviewedById = request.ReviewerId;
        submission.ReviewedAt = DateTime.UtcNow;
        submission.UpdatedAt = DateTime.UtcNow;

        // Update the underlying entry status
        if (submission.EntryType == SubmissionType.Word)
        {
            var word = await _db.WordEntries.FindAsync(new object[] { submission.EntryId }, ct);
            if (word != null)
            {
                word.Status = newStatus;
                word.ReviewedById = request.ReviewerId;
                if (newStatus == EntryStatus.Approved)
                    word.PublishedAt = DateTime.UtcNow;
                word.UpdatedAt = DateTime.UtcNow;
            }
        }
        else if (submission.EntryType == SubmissionType.Phrase)
        {
            var phrase = await _db.PhraseEntries.FindAsync(new object[] { submission.EntryId }, ct);
            if (phrase != null)
            {
                phrase.Status = newStatus;
                phrase.ReviewedById = request.ReviewerId;
                if (newStatus == EntryStatus.Approved)
                    phrase.PublishedAt = DateTime.UtcNow;
                phrase.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Audit log
        _db.AuditLogs.Add(new Domain.Entities.AuditLog
        {
            ActorId = request.ReviewerId,
            Action = $"Submission.{request.Action}",
            EntityType = submission.EntryType.ToString(),
            EntityId = submission.EntryId,
            DiffJson = request.AdminNote ?? $"{request.Action}d by admin"
        });

        await _db.SaveChangesAsync(ct);
    }
}
