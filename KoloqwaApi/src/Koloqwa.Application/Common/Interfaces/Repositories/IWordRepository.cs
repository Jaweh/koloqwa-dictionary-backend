using Koloqwa.Application.Common.Models;
using Koloqwa.Domain.Entities;

namespace Koloqwa.Application.Common.Interfaces.Repositories;

public interface IWordRepository
{
    Task<PagedResult<WordEntry>> SearchAsync(
        string? query,
        string? category,       // "Vernacular" or "Tribal"
        string? languageCode,
        string? partOfSpeech,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<WordEntry?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<WordEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
    Task AddAsync(WordEntry word, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
