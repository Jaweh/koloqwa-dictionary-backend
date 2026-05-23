using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Admin.Queries;

public record AdminReportDto(
    Guid Id,
    Guid EntryId,
    string EntryType,
    string EntryPreview,
    string? EntrySlug,
    string Reason,
    string? Notes,
    string Status,
    string ReporterName,
    string ReporterEmail,
    DateTime ReportedAt,
    DateTime? ReviewedAt,
    string? ReviewedByName
);

public record GetAdminReportsQuery(
    string? Status,
    string? Reason,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<AdminReportDto>>;

public class GetAdminReportsQueryHandler
    : IRequestHandler<GetAdminReportsQuery, PagedResult<AdminReportDto>>
{
    private readonly IApplicationDbContext _db;
    public GetAdminReportsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<AdminReportDto>> Handle(
        GetAdminReportsQuery request, CancellationToken ct)
    {
        var query = _db.WordReports
            .Include(r => r.ReportedBy)
            .Include(r => r.ReviewedBy)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(r => r.Status == request.Status);

        if (!string.IsNullOrWhiteSpace(request.Reason))
            query = query.Where(r => r.Reason == request.Reason);

        query = query.OrderByDescending(r => r.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        // Resolve entry previews
        var wordIds = items.Where(r => r.EntryType == "Word").Select(r => r.EntryId).ToList();
        var phraseIds = items.Where(r => r.EntryType == "Phrase").Select(r => r.EntryId).ToList();

        var words = wordIds.Any()
            ? await _db.WordEntries
                .Where(w => wordIds.Contains(w.Id))
                .Select(w => new { w.Id, w.Headword, w.Slug })
                .ToDictionaryAsync(w => w.Id, ct)
            : new Dictionary<Guid, object>() as dynamic;

        var phrases = phraseIds.Any()
            ? await _db.PhraseEntries
                .Where(p => phraseIds.Contains(p.Id))
                .Select(p => new { p.Id, p.PhraseText, p.Slug })
                .ToDictionaryAsync(p => p.Id, ct)
            : new Dictionary<Guid, object>() as dynamic;

        var dtos = items.Select(r =>
        {
            string preview = "Unknown";
            string? slug = null;

            if (r.EntryType == "Word" && wordIds.Contains(r.EntryId))
            {
                var w = ((IDictionary<Guid, dynamic>)words)[r.EntryId];
                preview = w.Headword; slug = w.Slug;
            }
            else if (r.EntryType == "Phrase" && phraseIds.Contains(r.EntryId))
            {
                var p = ((IDictionary<Guid, dynamic>)phrases)[r.EntryId];
                preview = p.PhraseText; slug = p.Slug;
            }

            return new AdminReportDto(
                r.Id, r.EntryId, r.EntryType, preview, slug,
                r.Reason, r.Notes, r.Status,
                r.ReportedBy.DisplayName, r.ReportedBy.Email,
                r.CreatedAt, r.ReviewedAt, r.ReviewedBy?.DisplayName
            );
        });

        return new PagedResult<AdminReportDto>
        {
            Items = dtos, TotalCount = total, Page = request.Page, PageSize = request.PageSize
        };
    }
}
