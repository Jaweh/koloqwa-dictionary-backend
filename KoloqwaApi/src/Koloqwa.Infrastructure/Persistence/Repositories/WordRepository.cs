using Koloqwa.Application.Common.Interfaces.Repositories;
using Koloqwa.Application.Common.Models;
using Koloqwa.Domain.Entities;
using Koloqwa.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Infrastructure.Persistence.Repositories;

public class WordRepository : IWordRepository
{
    private readonly ApplicationDbContext _db;
    public WordRepository(ApplicationDbContext db) => _db = db;

    public async Task<PagedResult<WordEntry>> SearchAsync(
        string? query, string? category, string? languageCode,
        string? partOfSpeech, int page, int pageSize, CancellationToken ct = default)
    {
        var q = _db.WordEntries
            .Include(w => w.Language)
            .Include(w => w.Definitions.OrderBy(d => d.SortOrder))
            .Where(w => w.Status == EntryStatus.Approved)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category) &&
            Enum.TryParse<EntryCategory>(category, true, out var cat))
            q = q.Where(w => w.Category == cat);

        if (!string.IsNullOrWhiteSpace(languageCode))
            q = q.Where(w => w.Language != null && w.Language.Code == languageCode);

        if (!string.IsNullOrWhiteSpace(partOfSpeech) &&
            Enum.TryParse<PartOfSpeech>(partOfSpeech, true, out var pos))
            q = q.Where(w => w.PartOfSpeech == pos);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.ToLower().Trim();

            // Exact/partial match first, then fuzzy fallback
            var exactMatches = q.Where(w =>
                w.Headword.ToLower().Contains(term) ||
                w.Definitions.Any(d => d.Definition.ToLower().Contains(term)));

            var hasExact = await exactMatches.AnyAsync(ct);

            if (hasExact)
            {
                q = exactMatches;
            }
            else
            {
                // Fuzzy match using trigram similarity via raw SQL
                q = q.Where(w =>
                    EF.Functions.Like(w.Headword.ToLower(), $"%{term}%") ||
                    _db.WordEntries
                        .FromSqlRaw(
                            "SELECT * FROM \"WordEntries\" WHERE similarity(\"Headword\", {0}) > 0.2",
                            term)
                        .Select(x => x.Id)
                        .Contains(w.Id));
            }
        }

        var total = await q.CountAsync(ct);

        // When fuzzy searching, order by similarity score
        IQueryable<WordEntry> ordered;
        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.ToLower().Trim();
            ordered = q.OrderByDescending(w => w.Headword.ToLower().StartsWith(term))
                       .ThenBy(w => w.Headword);
        }
        else
        {
            ordered = q.OrderBy(w => w.Headword);
        }

        var items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<WordEntry>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<WordEntry?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await _db.WordEntries
            .Include(w => w.Language)
            .Include(w => w.Definitions.OrderBy(d => d.SortOrder))
                .ThenInclude(d => d.Examples)
            .FirstOrDefaultAsync(w => w.Slug == slug && w.Status == EntryStatus.Approved, ct);

    public async Task<WordEntry?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.WordEntries
            .Include(w => w.Language)
            .Include(w => w.Definitions.OrderBy(d => d.SortOrder))
                .ThenInclude(d => d.Examples)
            .FirstOrDefaultAsync(w => w.Id == id, ct);

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default) =>
        await _db.WordEntries.AnyAsync(w => w.Slug == slug, ct);

    public async Task AddAsync(WordEntry word, CancellationToken ct = default) =>
        await _db.WordEntries.AddAsync(word, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}