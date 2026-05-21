using Koloqwa.Application.Common.Models;
using Koloqwa.Domain.Entities;

namespace Koloqwa.Application.Common.Interfaces.Repositories;

public interface IPhraseRepository
{
    /// <summary>Search published phrases with optional filters. Returns a paged result.</summary>
    Task<PagedResult<PhraseEntry>> SearchAsync(
        string? query,
        string? languageCode,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Get a published phrase entry by its URL slug, including meanings.</summary>
    Task<PhraseEntry?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>Get any phrase entry by ID regardless of status.</summary>
    Task<PhraseEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Check whether a slug is already taken.</summary>
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);

    /// <summary>Persist a new phrase entry.</summary>
    Task AddAsync(PhraseEntry phrase, CancellationToken ct = default);

    /// <summary>Save any tracked changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
