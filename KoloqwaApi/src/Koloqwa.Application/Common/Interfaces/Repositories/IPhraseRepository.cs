using Koloqwa.Application.Common.Models;
using Koloqwa.Domain.Entities;

namespace Koloqwa.Application.Common.Interfaces.Repositories;

public interface IPhraseRepository
{
    Task<PagedResult<PhraseEntry>> SearchAsync(
        string? query,
        string? category,       // "Vernacular" or "Tribal"
        string? languageCode,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<PhraseEntry?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<PhraseEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
    Task AddAsync(PhraseEntry phrase, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
