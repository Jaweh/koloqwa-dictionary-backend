using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.DTOs;
using Koloqwa.Domain.Enums;
using Koloqwa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Admin.Commands;

public record ReviewSubmissionCommand(
    Guid SubmissionId,
    ReviewSubmissionRequest Request,
    Guid ReviewerId
) : IRequest<Unit>;

public class ReviewSubmissionCommandHandler : IRequestHandler<ReviewSubmissionCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly IAuditService _audit;

    public ReviewSubmissionCommandHandler(IApplicationDbContext db, IAuditService audit)
    {
        _db = db; _audit = audit;
    }

    public async Task<Unit> Handle(ReviewSubmissionCommand request, CancellationToken ct)
    {
        var submission = await _db.SubmissionQueues
            .Include(s => s.WordEntry)
            .Include(s => s.PhraseEntry)
            .FirstOrDefaultAsync(s => s.Id == request.SubmissionId, ct);

        if (submission is null)
            throw new NotFoundException(nameof(Domain.Entities.SubmissionQueue), request.SubmissionId);

        if (submission.Status != EntryStatus.PendingReview)
            throw new DomainException("This submission has already been reviewed.");

        var decision = request.Request.Decision.ToLower();
        if (decision != "approve" && decision != "reject")
            throw new DomainException("Decision must be 'approve' or 'reject'.");

        var newStatus = decision == "approve" ? EntryStatus.Approved : EntryStatus.Rejected;

        submission.Status = newStatus;
        submission.AdminNote = request.Request.AdminNote;
        submission.ReviewedById = request.ReviewerId;
        submission.ReviewedAt = DateTime.UtcNow;

        // Update the actual entry status
        if (submission.EntryType == SubmissionType.Word && submission.WordEntry is not null)
        {
            submission.WordEntry.Status = newStatus;
            submission.WordEntry.ReviewedById = request.ReviewerId;
            if (newStatus == EntryStatus.Approved)
                submission.WordEntry.PublishedAt = DateTime.UtcNow;
        }
        else if (submission.EntryType == SubmissionType.Phrase && submission.PhraseEntry is not null)
        {
            submission.PhraseEntry.Status = newStatus;
            submission.PhraseEntry.ReviewedById = request.ReviewerId;
            if (newStatus == EntryStatus.Approved)
                submission.PhraseEntry.PublishedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            action: decision == "approve" ? "Approved" : "Rejected",
            entityType: submission.EntryType.ToString(),
            entityId: submission.EntryId,
            before: null,
            after: new { Status = newStatus.ToString(), Note = request.Request.AdminNote },
            ct);

        return Unit.Value;
    }
}
