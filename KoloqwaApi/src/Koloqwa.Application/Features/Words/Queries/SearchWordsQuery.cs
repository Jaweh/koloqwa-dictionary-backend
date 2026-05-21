using Koloqwa.Application.Common.Interfaces.Repositories;
using Koloqwa.Application.Common.Models;
using Koloqwa.Application.DTOs;
using MediatR;

namespace Koloqwa.Application.Features.Words.Queries;

public record SearchWordsQuery(
    string? Q,
    string? Category,
    string? LanguageCode,
    string? PartOfSpeech,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<WordSummaryDto>>;

public class SearchWordsQueryHandler : IRequestHandler<SearchWordsQuery, PagedResult<WordSummaryDto>>
{
    private readonly IWordRepository _words;

    public SearchWordsQueryHandler(IWordRepository words)
    {
        _words = words;
    }

    public async Task<PagedResult<WordSummaryDto>> Handle(SearchWordsQuery request, CancellationToken ct)
    {
        var result = await _words.SearchAsync(
            request.Q, request.Category, request.LanguageCode,
            request.PartOfSpeech, request.Page, request.PageSize, ct);

        var items = result.Items.Select(w => new WordSummaryDto(
            Id: w.Id,
            Headword: w.Headword,
            Slug: w.Slug,
            PartOfSpeech: w.PartOfSpeech.ToString(),
            Category: w.Category.ToString(),
            LanguageCode: w.Language?.Code,
            LanguageName: w.Language?.Name,
            FirstDefinition: w.Definitions.OrderBy(d => d.SortOrder)
                                          .Select(d => d.Definition)
                                          .FirstOrDefault() ?? string.Empty,
            Status: w.Status.ToString(),
            PublishedAt: w.PublishedAt
        ));

        return new PagedResult<WordSummaryDto>
        {
            Items = items,
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }
}