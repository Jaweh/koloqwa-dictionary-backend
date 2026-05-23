using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Admin.Queries;

public record AdminSuggestionDto(
    Guid Id,
    Guid EntryId,
    string EntryType,
    string EntryPreview,
    string? EntrySlug,
    string Field,
    string CurrentValue,
    string SuggestedValue,
    string? Notes,
    string Status,
    string SuggesterName,
    string SuggesterEmail,
    DateTime SubmittedAt,
    DateTime? ReviewedAt,
    string? ReviewedByName,
    string? AdminNote
);

public record GetAdminSuggestionsQuery(
    string? Status,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<AdminSuggestionDto>>;

public class GetAdminSuggestionsQueryHandler
    : IRequestHandler<GetAdminSuggestionsQuery, PagedResult<AdminSuggestionDto>>
{
    private readonly IApplicationDbContext _db;
    public GetAdminSuggestionsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<AdminSuggestionDto>> Handle(
        GetAdminSuggestionsQuery request, CancellationToken ct)
    {
        var query = _db.WordSuggestions
            .Include(s => s.SuggestedBy)
            .Include(s => s.ReviewedBy)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(s => s.Status == request.Status);

        query = query.OrderByDescending(s => s.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var wordIds = items.Where(s => s.EntryType == "Word").Select(s => s.EntryId).ToList();
        var phraseIds = items.Where(s => s.EntryType == "Phrase").Select(s => s.EntryId).ToList();

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

        var dtos = items.Select(s =>
        {
            string preview = "Unknown";
            string? slug = null;

            if (s.EntryType == "Word" && wordIds.Contains(s.EntryId))
            {
                var w = ((IDictionary<Guid, dynamic>)words)[s.EntryId];
                preview = w.Headword; slug = w.Slug;
            }
            else if (s.EntryType == "Phrase" && phraseIds.Contains(s.EntryId))
            {
                var p = ((IDictionary<Guid, dynamic>)phrases)[s.EntryId];
                preview = p.PhraseText; slug = p.Slug;
            }

            return new AdminSuggestionDto(
                s.Id, s.EntryId, s.EntryType, preview, slug,
                s.Field, s.CurrentValue, s.SuggestedValue, s.Notes,
                s.Status, s.SuggestedBy.DisplayName, s.SuggestedBy.Email,
                s.CreatedAt, s.ReviewedAt, s.ReviewedBy?.DisplayName, s.AdminNote
            );
        });

        return new PagedResult<AdminSuggestionDto>
        {
            Items = dtos, TotalCount = total, Page = request.Page, PageSize = request.PageSize
        };
    }
}
