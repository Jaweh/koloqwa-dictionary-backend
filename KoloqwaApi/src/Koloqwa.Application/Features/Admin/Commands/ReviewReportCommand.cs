using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Admin.Commands;

public record ReviewReportCommand(
    Guid ReportId,
    string Action, // "Dismiss" or "Delete"
    Guid AdminId
) : IRequest;

public class ReviewReportCommandHandler : IRequestHandler<ReviewReportCommand>
{
    private readonly IApplicationDbContext _db;
    public ReviewReportCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(ReviewReportCommand request, CancellationToken ct)
    {
        var report = await _db.WordReports
            .FirstOrDefaultAsync(r => r.Id == request.ReportId, ct)
            ?? throw new NotFoundException("Report", request.ReportId);

        if (request.Action == "Delete")
        {
            // Hard delete the reported entry
            if (report.EntryType == "Word")
            {
                var word = await _db.WordEntries.FindAsync(new object[] { report.EntryId }, ct);
                if (word != null) _db.WordEntries.Remove(word);

                var submission = await _db.SubmissionQueues
                    .FirstOrDefaultAsync(s => s.EntryId == report.EntryId, ct);
                if (submission != null) _db.SubmissionQueues.Remove(submission);
            }
            else
            {
                var phrase = await _db.PhraseEntries.FindAsync(new object[] { report.EntryId }, ct);
                if (phrase != null) _db.PhraseEntries.Remove(phrase);

                var submission = await _db.SubmissionQueues
                    .FirstOrDefaultAsync(s => s.EntryId == report.EntryId, ct);
                if (submission != null) _db.SubmissionQueues.Remove(submission);
            }

            // Mark all reports for this entry as reviewed
            var relatedReports = await _db.WordReports
                .Where(r => r.EntryId == report.EntryId)
                .ToListAsync(ct);
            foreach (var r in relatedReports)
            {
                r.Status = "Reviewed";
                r.ReviewedById = request.AdminId;
                r.ReviewedAt = DateTime.UtcNow;
            }
        }
        else
        {
            report.Status = "Dismissed";
            report.ReviewedById = request.AdminId;
            report.ReviewedAt = DateTime.UtcNow;
        }

        _db.AuditLogs.Add(new Domain.Entities.AuditLog
        {
            ActorId = request.AdminId,
            Action = $"Report.{request.Action}",
            EntityType = report.EntryType,
            EntityId = report.EntryId,
            DiffJson = $"Report {request.Action.ToLower()}d by admin"
        });

        await _db.SaveChangesAsync(ct);
    }
}
