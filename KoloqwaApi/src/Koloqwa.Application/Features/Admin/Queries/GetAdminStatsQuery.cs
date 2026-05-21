using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.DTOs;
using Koloqwa.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Admin.Queries;

public record GetAdminStatsQuery : IRequest<AdminStatsDto>;

public class GetAdminStatsQueryHandler : IRequestHandler<GetAdminStatsQuery, AdminStatsDto>
{
    private readonly IApplicationDbContext _db;
    public GetAdminStatsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<AdminStatsDto> Handle(GetAdminStatsQuery request, CancellationToken ct)
    {
        var totalWords    = await _db.WordEntries.CountAsync(ct);
        var totalPhrases  = await _db.PhraseEntries.CountAsync(ct);
        var pending       = await _db.SubmissionQueues.CountAsync(s => s.Status == EntryStatus.PendingReview, ct);
        var approved      = await _db.SubmissionQueues.CountAsync(s => s.Status == EntryStatus.Approved, ct);
        var rejected      = await _db.SubmissionQueues.CountAsync(s => s.Status == EntryStatus.Rejected, ct);
        var totalUsers    = await _db.Users.CountAsync(ct);
        var activeUsers   = await _db.Users.CountAsync(u => u.IsActive, ct);

        return new AdminStatsDto(totalWords, totalPhrases, pending, approved, rejected, totalUsers, activeUsers);
    }
}
