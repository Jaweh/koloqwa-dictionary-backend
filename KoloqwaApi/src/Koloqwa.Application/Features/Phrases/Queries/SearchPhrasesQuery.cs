using Koloqwa.Application.Common.Interfaces.Repositories;
using Koloqwa.Application.Common.Models;
using Koloqwa.Application.DTOs;
using MediatR;

namespace Koloqwa.Application.Features.Phrases.Queries;

public record SearchPhrasesQuery(
    string? Q,
    string? Category,
    string? LanguageCode,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<PhraseSummaryDto>>;

public class SearchPhrasesQueryHandler : IRequestHandler<SearchPhrasesQuery, PagedResult<PhraseSummaryDto>>
{
    private readonly IPhraseRepository _phrases;

    public SearchPhrasesQueryHandler(IPhraseRepository phrases)
    {
        _phrases = phrases;
    }

    public async Task<PagedResult<PhraseSummaryDto>> Handle(SearchPhrasesQuery request, CancellationToken ct)
    {
        var result = await _phrases.SearchAsync(
            request.Q, request.Category, request.LanguageCode,
            request.Page, request.PageSize, ct);

        var items = result.Items.Select(p => new PhraseSummaryDto(
            Id: p.Id,
            PhraseText: p.PhraseText,
            Slug: p.Slug,
            Category: p.Category.ToString(),
            LanguageCode: p.Language?.Code,
            LanguageName: p.Language?.Name,
            FirstMeaning: p.Meanings.OrderBy(m => m.SortOrder)
                                    .Select(m => m.Meaning)
                                    .FirstOrDefault() ?? string.Empty,
            Status: p.Status.ToString(),
            PublishedAt: p.PublishedAt
        ));

        return new PagedResult<PhraseSummaryDto>
        {
            Items = items,
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }
}