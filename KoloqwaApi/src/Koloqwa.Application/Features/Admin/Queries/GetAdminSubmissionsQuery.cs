using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.Common.Models;
using Koloqwa.Application.DTOs;
using Koloqwa.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Admin.Queries;

public record GetAdminSubmissionsQuery(
    string? Status,
    string? EntryType,
    string? Search,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<AdminSubmissionDto>>;

public class GetAdminSubmissionsQueryHandler
    : IRequestHandler<GetAdminSubmissionsQuery, PagedResult<AdminSubmissionDto>>
{
    private readonly IApplicationDbContext _db;
    public GetAdminSubmissionsQueryHandler(IApplicationDbContext db) => _db = db;

    private record WordLookup(Guid Id, string Headword, string Category, string? LangName);
    private record PhraseLookup(Guid Id, string PhraseText, string Category, string? LangName);

    public async Task<PagedResult<AdminSubmissionDto>> Handle(
        GetAdminSubmissionsQuery request, CancellationToken ct)
    {
        var query = _db.SubmissionQueues
            .Include(s => s.Submitter)
            .Include(s => s.ReviewedBy)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<EntryStatus>(request.Status, true, out var status))
            query = query.Where(s => s.Status == status);

        if (!string.IsNullOrWhiteSpace(request.EntryType) &&
            Enum.TryParse<SubmissionType>(request.EntryType, true, out var entryType))
            query = query.Where(s => s.EntryType == entryType);

        query = query.OrderByDescending(s => s.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var wordIds = items
            .Where(s => s.EntryType == SubmissionType.Word)
            .Select(s => s.EntryId).ToList();

        var phraseIds = items
            .Where(s => s.EntryType == SubmissionType.Phrase)
            .Select(s => s.EntryId).ToList();

        var words = wordIds.Any()
            ? await _db.WordEntries
                .Include(w => w.Language)
                .Where(w => wordIds.Contains(w.Id))
                .Select(w => new WordLookup(
                    w.Id,
                    w.Headword,
                    w.Category.ToString(),
                    w.Language != null ? w.Language.Name : null))
                .ToDictionaryAsync(w => w.Id, ct)
            : new Dictionary<Guid, WordLookup>();

        var phrases = phraseIds.Any()
            ? await _db.PhraseEntries
                .Include(p => p.Language)
                .Where(p => phraseIds.Contains(p.Id))
                .Select(p => new PhraseLookup(
                    p.Id,
                    p.PhraseText,
                    p.Category.ToString(),
                    p.Language != null ? p.Language.Name : null))
                .ToDictionaryAsync(p => p.Id, ct)
            : new Dictionary<Guid, PhraseLookup>();

        var dtos = items.Select(s =>
        {
            string preview = "Unknown";
            string? category = null;
            string? langName = null;

            if (s.EntryType == SubmissionType.Word && words.TryGetValue(s.EntryId, out var w))
            {
                preview = w.Headword;
                category = w.Category;
                langName = w.LangName;
            }
            else if (s.EntryType == SubmissionType.Phrase && phrases.TryGetValue(s.EntryId, out var p))
            {
                preview = p.PhraseText;
                category = p.Category;
                langName = p.LangName;
            }

            return new AdminSubmissionDto(
                Id: s.Id,
                EntryType: s.EntryType.ToString(),
                EntryId: s.EntryId,
                EntryPreview: preview,
                Status: s.Status.ToString(),
                SubmitterName: s.Submitter.DisplayName,
                SubmitterEmail: s.Submitter.Email,
                AdminNote: s.AdminNote,
                Category: category,
                LanguageName: langName,
                SubmittedAt: s.CreatedAt,
                ReviewedAt: s.ReviewedAt,
                ReviewedByName: s.ReviewedBy?.DisplayName
            );
        });

        return new PagedResult<AdminSubmissionDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}