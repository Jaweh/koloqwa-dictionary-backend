using AutoMapper;
using Koloqwa.Application.Common.Interfaces.Repositories;
using Koloqwa.Application.Common.Models;
using Koloqwa.Application.DTOs;
using MediatR;

namespace Koloqwa.Application.Features.Words.Queries;

public record SearchWordsQuery(
    string? Q,
    string? LanguageCode,
    string? PartOfSpeech,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<WordSummaryDto>>;

public class SearchWordsQueryHandler : IRequestHandler<SearchWordsQuery, PagedResult<WordSummaryDto>>
{
    private readonly IWordRepository _words;
    private readonly IMapper _mapper;

    public SearchWordsQueryHandler(IWordRepository words, IMapper mapper)
    {
        _words = words; _mapper = mapper;
    }

    public async Task<PagedResult<WordSummaryDto>> Handle(SearchWordsQuery request, CancellationToken ct)
    {
        var result = await _words.SearchAsync(
            request.Q, request.LanguageCode, request.PartOfSpeech,
            request.Page, request.PageSize, ct);

        return new PagedResult<WordSummaryDto>
        {
            Items = _mapper.Map<IEnumerable<WordSummaryDto>>(result.Items),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }
}
