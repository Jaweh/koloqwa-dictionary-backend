using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Admin.Queries;

public record AnalyticsDto(
    List<DailyCountDto> SubmissionsOverTime,
    List<DailyCountDto> ApprovalsOverTime,
    List<DailyCountDto> RegistrationsOverTime,
    ApprovalRateDto ApprovalRate,
    List<ContributorDto> TopContributors,
    List<ContributorDto> TopApproved,
    List<CategoryBreakdownDto> CategoryBreakdown,
    List<TribeBreakdownDto> TribeBreakdown,
    ModerationStatsDto ModerationStats
);

public record DailyCountDto(string Date, int Count);
public record ApprovalRateDto(int Total, int Approved, int Rejected, int Pending, double ApprovalPercent);
public record ContributorDto(string DisplayName, string Email, int Count);
public record CategoryBreakdownDto(string Category, string EntryType, int Count);
public record TribeBreakdownDto(string LanguageName, string LanguageCode, int Count);
public record ModerationStatsDto(double AvgHoursToReview, int ReviewedLast7Days, int ReviewedLast30Days);

public record GetAnalyticsQuery(int Days = 30) : IRequest<AnalyticsDto>;

public class GetAnalyticsQueryHandler : IRequestHandler<GetAnalyticsQuery, AnalyticsDto>
{
    private readonly IApplicationDbContext _db;
    public GetAnalyticsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<AnalyticsDto> Handle(GetAnalyticsQuery request, CancellationToken ct)
    {
        var since = DateTime.UtcNow.AddDays(-request.Days).Date;
        var now = DateTime.UtcNow;

        // ── Submissions over time ─────────────────────────────────────────────
        var submissionsRaw = await _db.SubmissionQueues
            .Where(s => s.CreatedAt >= since)
            .GroupBy(s => s.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync(ct);

        var submissionsOverTime = submissionsRaw
            .Select(x => new DailyCountDto(x.Date.ToString("yyyy-MM-dd"), x.Count))
            .ToList();

        // ── Approvals over time ───────────────────────────────────────────────
        var approvalsRaw = await _db.SubmissionQueues
            .Where(s => s.ReviewedAt != null && s.Status == EntryStatus.Approved && s.ReviewedAt >= since)
            .GroupBy(s => s.ReviewedAt!.Value.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync(ct);

        var approvalsOverTime = approvalsRaw
            .Select(x => new DailyCountDto(x.Date.ToString("yyyy-MM-dd"), x.Count))
            .ToList();

        // ── Registrations over time ───────────────────────────────────────────
        var registrationsRaw = await _db.Users
            .Where(u => u.CreatedAt >= since)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync(ct);

        var registrationsOverTime = registrationsRaw
            .Select(x => new DailyCountDto(x.Date.ToString("yyyy-MM-dd"), x.Count))
            .ToList();

        // ── Approval rate ─────────────────────────────────────────────────────
        var total    = await _db.SubmissionQueues.CountAsync(ct);
        var approved = await _db.SubmissionQueues.CountAsync(s => s.Status == EntryStatus.Approved, ct);
        var rejected = await _db.SubmissionQueues.CountAsync(s => s.Status == EntryStatus.Rejected, ct);
        var pending  = await _db.SubmissionQueues.CountAsync(s => s.Status == EntryStatus.PendingReview, ct);
        var approvalPercent = total > 0 ? Math.Round((double)approved / total * 100, 1) : 0;

        var approvalRate = new ApprovalRateDto(total, approved, rejected, pending, approvalPercent);

        // ── Top contributors by submission count ──────────────────────────────
        var topContributorsRaw = await _db.SubmissionQueues
            .Include(s => s.Submitter)
            .GroupBy(s => new { s.SubmitterId, s.Submitter.DisplayName, s.Submitter.Email })
            .Select(g => new { g.Key.DisplayName, g.Key.Email, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync(ct);

        var topContributors = topContributorsRaw
            .Select(x => new ContributorDto(x.DisplayName, x.Email, x.Count))
            .ToList();

        // ── Top contributors by approved entries ──────────────────────────────
        var topApprovedRaw = await _db.SubmissionQueues
            .Include(s => s.Submitter)
            .Where(s => s.Status == EntryStatus.Approved)
            .GroupBy(s => new { s.SubmitterId, s.Submitter.DisplayName, s.Submitter.Email })
            .Select(g => new { g.Key.DisplayName, g.Key.Email, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync(ct);

        var topApproved = topApprovedRaw
            .Select(x => new ContributorDto(x.DisplayName, x.Email, x.Count))
            .ToList();

        // ── Category breakdown ────────────────────────────────────────────────
        var wordCats = await _db.WordEntries
            .GroupBy(w => w.Category)
            .Select(g => new { Category = g.Key.ToString(), EntryType = "Word", Count = g.Count() })
            .ToListAsync(ct);

        var phraseCats = await _db.PhraseEntries
            .GroupBy(p => p.Category)
            .Select(g => new { Category = g.Key.ToString(), EntryType = "Phrase", Count = g.Count() })
            .ToListAsync(ct);

        var categoryBreakdown = wordCats
            .Concat(phraseCats)
            .Select(x => new CategoryBreakdownDto(x.Category, x.EntryType, x.Count))
            .OrderByDescending(x => x.Count)
            .ToList();

        // ── Tribe breakdown ───────────────────────────────────────────────────
        var wordTribes = await _db.WordEntries
            .Include(w => w.Language)
            .Where(w => w.Language != null)
            .GroupBy(w => new { w.Language!.Name, w.Language.Code })
            .Select(g => new { g.Key.Name, g.Key.Code, Count = g.Count() })
            .ToListAsync(ct);

        var phraseTribes = await _db.PhraseEntries
            .Include(p => p.Language)
            .Where(p => p.Language != null)
            .GroupBy(p => new { p.Language!.Name, p.Language.Code })
            .Select(g => new { g.Key.Name, g.Key.Code, Count = g.Count() })
            .ToListAsync(ct);

        var tribeBreakdown = wordTribes
            .Concat(phraseTribes)
            .GroupBy(x => new { x.Name, x.Code })
            .Select(g => new TribeBreakdownDto(g.Key.Name, g.Key.Code, g.Sum(x => x.Count)))
            .OrderByDescending(x => x.Count)
            .ToList();

        // ── Moderation stats ──────────────────────────────────────────────────
        var reviewedEntries = await _db.SubmissionQueues
            .Where(s => s.ReviewedAt != null)
            .Select(s => new { s.CreatedAt, ReviewedAt = s.ReviewedAt!.Value })
            .ToListAsync(ct);

        var avgHours = reviewedEntries.Any()
            ? reviewedEntries.Average(s => (s.ReviewedAt - s.CreatedAt).TotalHours)
            : 0;

        var reviewedLast7  = await _db.SubmissionQueues
            .CountAsync(s => s.ReviewedAt != null && s.ReviewedAt >= now.AddDays(-7), ct);
        var reviewedLast30 = await _db.SubmissionQueues
            .CountAsync(s => s.ReviewedAt != null && s.ReviewedAt >= now.AddDays(-30), ct);

        var moderationStats = new ModerationStatsDto(
            Math.Round(avgHours, 1), reviewedLast7, reviewedLast30);

        return new AnalyticsDto(
            submissionsOverTime, approvalsOverTime, registrationsOverTime,
            approvalRate, topContributors, topApproved,
            categoryBreakdown, tribeBreakdown, moderationStats
        );
    }
}
