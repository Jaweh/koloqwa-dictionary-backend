using AutoMapper;
using Koloqwa.Application.Common.Interfaces.Repositories;
using Koloqwa.Application.Common.Models;
using Koloqwa.Application.DTOs;
using MediatR;

namespace Koloqwa.Application.Features.Phrases.Queries;

public record SearchPhrasesQuery(
    string? Q,
    string? LanguageCode,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<PhraseSummaryDto>>;

public class SearchPhrasesQueryHandler : IRequestHandler<SearchPhrasesQuery, PagedResult<PhraseSummaryDto>>
{
    private readonly IPhraseRepository _phrases;
    private readonly IMapper _mapper;

    public SearchPhrasesQueryHandler(IPhraseRepository phrases, IMapper mapper)
    {
        _phrases = phrases; _mapper = mapper;
    }

    public async Task<PagedResult<PhraseSummaryDto>> Handle(SearchPhrasesQuery request, CancellationToken ct)
    {
        var result = await _phrases.SearchAsync(
            request.Q, request.LanguageCode,
            request.Page, request.PageSize, ct);

        return new PagedResult<PhraseSummaryDto>
        {
            Items = _mapper.Map<IEnumerable<PhraseSummaryDto>>(result.Items),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }
}
