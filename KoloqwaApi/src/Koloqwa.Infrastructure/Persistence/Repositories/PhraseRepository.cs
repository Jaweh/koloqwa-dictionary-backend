using Koloqwa.Application.Common.Interfaces.Repositories;
using Koloqwa.Application.Common.Models;
using Koloqwa.Domain.Entities;
using Koloqwa.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Infrastructure.Persistence.Repositories;

public class PhraseRepository : IPhraseRepository
{
    private readonly ApplicationDbContext _db;
    public PhraseRepository(ApplicationDbContext db) => _db = db;

    public async Task<PagedResult<PhraseEntry>> SearchAsync(
        string? query, string? category, string? languageCode,
        int page, int pageSize, CancellationToken ct = default)
    {
        var q = _db.PhraseEntries
            .Include(p => p.Language)
            .Include(p => p.Meanings.OrderBy(m => m.SortOrder))
            .Where(p => p.Status == EntryStatus.Approved)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category) &&
            Enum.TryParse<EntryCategory>(category, true, out var cat))
            q = q.Where(p => p.Category == cat);

        if (!string.IsNullOrWhiteSpace(languageCode))
            q = q.Where(p => p.Language != null && p.Language.Code == languageCode);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.ToLower().Trim();
            q = q.Where(p =>
                p.PhraseText.ToLower().Contains(term) ||
                p.Meanings.Any(m => m.Meaning.ToLower().Contains(term)));
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(p => p.PhraseText)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<PhraseEntry>
        {
            Items = items, TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<PhraseEntry?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await _db.PhraseEntries
            .Include(p => p.Language)
            .Include(p => p.Meanings.OrderBy(m => m.SortOrder))
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == EntryStatus.Approved, ct);

    public async Task<PhraseEntry?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.PhraseEntries
            .Include(p => p.Language)
            .Include(p => p.Meanings.OrderBy(m => m.SortOrder))
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default) =>
        await _db.PhraseEntries.AnyAsync(p => p.Slug == slug, ct);

    public async Task AddAsync(PhraseEntry phrase, CancellationToken ct = default) =>
        await _db.PhraseEntries.AddAsync(phrase, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}
