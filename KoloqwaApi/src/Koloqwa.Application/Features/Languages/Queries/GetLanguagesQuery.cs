using Koloqwa.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Languages.Queries;

public record LanguageDto(Guid Id, string Code, string Name);

public record GetLanguagesQuery : IRequest<IEnumerable<LanguageDto>>;

public class GetLanguagesQueryHandler : IRequestHandler<GetLanguagesQuery, IEnumerable<LanguageDto>>
{
    private readonly IApplicationDbContext _db;
    public GetLanguagesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<LanguageDto>> Handle(GetLanguagesQuery request, CancellationToken ct)
    {
        return await _db.Languages
            .Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .Select(l => new LanguageDto(l.Id, l.Code, l.Name))
            .ToListAsync(ct);
    }
}