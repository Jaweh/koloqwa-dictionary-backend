using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Domain.Entities;
using Koloqwa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Community.Commands;

public record ReportEntryCommand(
    Guid EntryId,
    string EntryType,
    string Reason,
    string? Notes,
    Guid UserId
) : IRequest;

public class ReportEntryCommandHandler : IRequestHandler<ReportEntryCommand>
{
    private readonly IApplicationDbContext _db;
    public ReportEntryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(ReportEntryCommand request, CancellationToken ct)
    {
        var validReasons = new[] { "Offensive", "IncorrectMeaning", "Spam", "Other" };
        if (!validReasons.Contains(request.Reason))
            throw new DomainException("Invalid report reason.");

        // Prevent duplicate reports from same user
        var existing = await _db.WordReports.AnyAsync(r =>
            r.EntryId == request.EntryId && r.ReportedById == request.UserId, ct);
        if (existing)
            throw new DomainException("You have already reported this entry.");

        _db.WordReports.Add(new WordReport
        {
            EntryId = request.EntryId,
            EntryType = request.EntryType,
            Reason = request.Reason,
            Notes = request.Notes?.Trim(),
            ReportedById = request.UserId,
            Status = "Pending",
            CreatedBy = request.UserId
        });

        await _db.SaveChangesAsync(ct);
    }
}
