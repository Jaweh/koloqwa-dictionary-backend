using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.DTOs;
using Koloqwa.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Admin.Queries;

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IApplicationDbContext _db;
    public GetDashboardStatsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;

        var totalWords = await _db.WordEntries.CountAsync(w => w.Status == EntryStatus.Approved, ct);
        var totalPhrases = await _db.PhraseEntries.CountAsync(p => p.Status == EntryStatus.Approved, ct);
        var pending = await _db.SubmissionQueues.CountAsync(s => s.Status == EntryStatus.PendingReview, ct);
        var totalUsers = await _db.Users.CountAsync(u => u.IsActive, ct);
        var publishedToday = await _db.WordEntries.CountAsync(w =>
            w.PublishedAt.HasValue && w.PublishedAt.Value.Date == today, ct)
            + await _db.PhraseEntries.CountAsync(p =>
            p.PublishedAt.HasValue && p.PublishedAt.Value.Date == today, ct);

        return new DashboardStatsDto(totalWords, totalPhrases, pending, totalUsers, publishedToday);
    }
}
