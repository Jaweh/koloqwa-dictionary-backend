using AutoMapper;
using Koloqwa.Application.Common.Interfaces.Repositories;
using Koloqwa.Application.DTOs;
using Koloqwa.Domain.Exceptions;
using MediatR;

namespace Koloqwa.Application.Features.Phrases.Queries;

public record GetPhraseBySlugQuery(string Slug) : IRequest<PhraseDetailDto>;

public class GetPhraseBySlugQueryHandler : IRequestHandler<GetPhraseBySlugQuery, PhraseDetailDto>
{
    private readonly IPhraseRepository _phrases;
    private readonly IMapper _mapper;

    public GetPhraseBySlugQueryHandler(IPhraseRepository phrases, IMapper mapper)
    {
        _phrases = phrases; _mapper = mapper;
    }

    public async Task<PhraseDetailDto> Handle(GetPhraseBySlugQuery request, CancellationToken ct)
    {
        var phrase = await _phrases.GetBySlugAsync(request.Slug, ct);

        if (phrase is null)
            throw new NotFoundException(nameof(PhraseDetailDto), request.Slug);

        return _mapper.Map<PhraseDetailDto>(phrase);
    }
}
