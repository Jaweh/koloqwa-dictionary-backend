using AutoMapper;
using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.Common.Models;
using Koloqwa.Application.DTOs;
using Koloqwa.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Admin.Queries;

public record GetMySubmissionsQuery(Guid UserId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<SubmissionDto>>;

public class GetMySubmissionsQueryHandler : IRequestHandler<GetMySubmissionsQuery, PagedResult<SubmissionDto>>
{
    private readonly IApplicationDbContext _db;

    public GetMySubmissionsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<SubmissionDto>> Handle(GetMySubmissionsQuery request, CancellationToken ct)
    {
        var query = _db.SubmissionQueues
            .Include(s => s.Submitter)
            .Where(s => s.SubmitterId == request.UserId)
            .OrderByDescending(s => s.CreatedAt);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        // Resolve entry previews manually using EntryType + EntryId
        var wordIds = items
            .Where(s => s.EntryType == SubmissionType.Word)
            .Select(s => s.EntryId)
            .ToList();

        var phraseIds = items
            .Where(s => s.EntryType == SubmissionType.Phrase)
            .Select(s => s.EntryId)
            .ToList();

        var words = wordIds.Any()
            ? await _db.WordEntries
                .Where(w => wordIds.Contains(w.Id))
                .Select(w => new { w.Id, w.Headword })
                .ToDictionaryAsync(w => w.Id, w => w.Headword, ct)
            : new Dictionary<Guid, string>();

        var phrases = phraseIds.Any()
            ? await _db.PhraseEntries
                .Where(p => phraseIds.Contains(p.Id))
                .Select(p => new { p.Id, p.PhraseText })
                .ToDictionaryAsync(p => p.Id, p => p.PhraseText, ct)
            : new Dictionary<Guid, string>();

        var dtos = items.Select(s => new SubmissionDto(
            Id: s.Id,
            EntryType: s.EntryType.ToString(),
            EntryId: s.EntryId,
            EntryPreview: s.EntryType == SubmissionType.Word
                ? words.GetValueOrDefault(s.EntryId, "Unknown")
                : phrases.GetValueOrDefault(s.EntryId, "Unknown"),
            Status: s.Status.ToString(),
            SubmitterName: s.Submitter.DisplayName,
            SubmitterEmail: s.Submitter.Email,
            AdminNote: s.AdminNote,
            SubmittedAt: s.CreatedAt,
            ReviewedAt: s.ReviewedAt
        ));

        return new PagedResult<SubmissionDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}