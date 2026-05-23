using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Community.Queries;

public record FavouriteItemDto(
    Guid EntryId,
    string EntryType,
    string EntryPreview,
    string? PartOfSpeech,
    string? FirstMeaning,
    string? Slug,
    DateTime SavedAt
);

public record GetUserFavouritesQuery(Guid UserId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<FavouriteItemDto>>;

public class GetUserFavouritesQueryHandler : IRequestHandler<GetUserFavouritesQuery, PagedResult<FavouriteItemDto>>
{
    private readonly IApplicationDbContext _db;
    public GetUserFavouritesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<FavouriteItemDto>> Handle(GetUserFavouritesQuery request, CancellationToken ct)
    {
        var favs = await _db.UserFavourites
            .Where(f => f.UserId == request.UserId)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var total = await _db.UserFavourites.CountAsync(f => f.UserId == request.UserId, ct);

        var wordIds = favs.Where(f => f.EntryType == "Word").Select(f => f.EntryId).ToList();
        var phraseIds = favs.Where(f => f.EntryType == "Phrase").Select(f => f.EntryId).ToList();

        var words = wordIds.Any()
            ? await _db.WordEntries
                .Include(w => w.Definitions)
                .Where(w => wordIds.Contains(w.Id))
                .ToListAsync(ct)
            : new();

        var phrases = phraseIds.Any()
            ? await _db.PhraseEntries
                .Include(p => p.Meanings)
                .Where(p => phraseIds.Contains(p.Id))
                .ToListAsync(ct)
            : new();

        var items = favs.Select(f =>
        {
            if (f.EntryType == "Word")
            {
                var w = words.FirstOrDefault(x => x.Id == f.EntryId);
                return w == null ? null : new FavouriteItemDto(
                    f.EntryId, "Word", w.Headword,
                    w.PartOfSpeech.ToString(),
                    w.Definitions.OrderBy(d => d.SortOrder).FirstOrDefault()?.Definition,
                    w.Slug, f.CreatedAt);
            }
            else
            {
                var p = phrases.FirstOrDefault(x => x.Id == f.EntryId);
                return p == null ? null : new FavouriteItemDto(
                    f.EntryId, "Phrase", p.PhraseText, null,
                    p.Meanings.OrderBy(m => m.SortOrder).FirstOrDefault()?.Meaning,
                    p.Slug, f.CreatedAt);
            }
        }).Where(x => x != null).Cast<FavouriteItemDto>().ToList();

        return new PagedResult<FavouriteItemDto>
        {
            Items = items, TotalCount = total, Page = request.Page, PageSize = request.PageSize
        };
    }
}
