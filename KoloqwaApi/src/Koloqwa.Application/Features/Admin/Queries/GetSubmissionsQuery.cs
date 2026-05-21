using AutoMapper;
using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.Common.Models;
using Koloqwa.Application.DTOs;
using Koloqwa.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Admin.Queries;

public record GetSubmissionsQuery(
    string? Status,
    string? Type,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<SubmissionDto>>;

public class GetSubmissionsQueryHandler : IRequestHandler<GetSubmissionsQuery, PagedResult<SubmissionDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetSubmissionsQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db; _mapper = mapper;
    }

    public async Task<PagedResult<SubmissionDto>> Handle(GetSubmissionsQuery request, CancellationToken ct)
    {
        var query = _db.SubmissionQueues
            .Include(s => s.Submitter)
            .Include(s => s.WordEntry)
            .Include(s => s.PhraseEntry)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<EntryStatus>(request.Status, true, out var status))
            query = query.Where(s => s.Status == status);

        if (!string.IsNullOrWhiteSpace(request.Type) &&
            Enum.TryParse<SubmissionType>(request.Type, true, out var type))
            query = query.Where(s => s.EntryType == type);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<SubmissionDto>
        {
            Items = _mapper.Map<IEnumerable<SubmissionDto>>(items),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
