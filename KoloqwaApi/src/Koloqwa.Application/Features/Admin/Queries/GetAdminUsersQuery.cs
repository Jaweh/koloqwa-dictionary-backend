using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.Common.Models;
using Koloqwa.Application.DTOs;
using Koloqwa.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Admin.Queries;

public record GetAdminUsersQuery(string? Search, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<AdminUserDto>>;

public class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, PagedResult<AdminUserDto>>
{
    private readonly IApplicationDbContext _db;
    public GetAdminUsersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<AdminUserDto>> Handle(GetAdminUsersQuery request, CancellationToken ct)
    {
        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(term) ||
                u.DisplayName.ToLower().Contains(term));
        }

        query = query.OrderByDescending(u => u.CreatedAt);
        var total = await query.CountAsync(ct);
        var users = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var userIds = users.Select(u => (Guid?)u.Id).ToList();
        var submissionCounts = await _db.SubmissionQueues
            .Where(s => s.SubmitterId != null && userIds.Contains(s.SubmitterId))
            .GroupBy(s => s.SubmitterId)
            .Select(g => new { UserId = g.Key, Total = g.Count(), Approved = g.Count(s => s.Status == EntryStatus.Approved) })
            .ToDictionaryAsync(g => g.UserId!.Value, ct);

        var dtos = users.Select(u =>
        {
            submissionCounts.TryGetValue(u.Id, out var counts);
            return new AdminUserDto(
                u.Id, u.Email, u.DisplayName, u.Role.ToString(),
                u.IsActive, u.EmailVerified, counts?.Total ?? 0, counts?.Approved ?? 0, u.CreatedAt);
        });

        return new PagedResult<AdminUserDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}