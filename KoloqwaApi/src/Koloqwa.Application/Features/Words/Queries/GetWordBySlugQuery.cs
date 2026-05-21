using AutoMapper;
using Koloqwa.Application.Common.Interfaces.Repositories;
using Koloqwa.Application.DTOs;
using Koloqwa.Domain.Exceptions;
using MediatR;

namespace Koloqwa.Application.Features.Words.Queries;

public record GetWordBySlugQuery(string Slug) : IRequest<WordDetailDto>;

public class GetWordBySlugQueryHandler : IRequestHandler<GetWordBySlugQuery, WordDetailDto>
{
    private readonly IWordRepository _words;
    private readonly IMapper _mapper;

    public GetWordBySlugQueryHandler(IWordRepository words, IMapper mapper)
    {
        _words = words; _mapper = mapper;
    }

    public async Task<WordDetailDto> Handle(GetWordBySlugQuery request, CancellationToken ct)
    {
        var word = await _words.GetBySlugAsync(request.Slug, ct);

        if (word is null)
            throw new NotFoundException(nameof(WordDetailDto), request.Slug);

        return _mapper.Map<WordDetailDto>(word);
    }
}
