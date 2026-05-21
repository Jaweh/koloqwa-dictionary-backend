using Koloqwa.Application.Common.Models;
using Koloqwa.Domain.Entities;

namespace Koloqwa.Application.Common.Interfaces.Repositories;

public interface IWordRepository
{
    /// <summary>Search published words with optional filters. Returns a paged result.</summary>
    Task<PagedResult<WordEntry>> SearchAsync(
        string? query,
        string? languageCode,
        string? partOfSpeech,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Get a published word entry by its URL slug, including definitions and examples.</summary>
    Task<WordEntry?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>Get any word entry by ID regardless of status (used by admin/submission workflow).</summary>
    Task<WordEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Check whether a slug is already taken.</summary>
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);

    /// <summary>Persist a new word entry.</summary>
    Task AddAsync(WordEntry word, CancellationToken ct = default);

    /// <summary>Save any tracked changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
