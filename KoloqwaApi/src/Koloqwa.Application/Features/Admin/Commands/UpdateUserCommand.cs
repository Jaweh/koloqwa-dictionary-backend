using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Domain.Enums;
using Koloqwa.Domain.Exceptions;
using MediatR;

namespace Koloqwa.Application.Features.Admin.Commands;

public record UpdateUserRoleCommand(Guid UserId, string Role, Guid AdminId) : IRequest;
public record ToggleUserActiveCommand(Guid UserId, bool IsActive, Guid AdminId) : IRequest;

public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand>
{
    private readonly IApplicationDbContext _db;
    public UpdateUserRoleCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpdateUserRoleCommand request, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object[] { request.UserId }, ct)
            ?? throw new NotFoundException("User", request.UserId);

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            throw new DomainException($"Invalid role: {request.Role}");

        user.Role = role;
        user.UpdatedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(new Domain.Entities.AuditLog
        {
            ActorId = request.AdminId,
            Action = "User.RoleUpdate",
            EntityType = "User",
            EntityId = user.Id,
            DiffJson = $"Role changed to {role}"
        });

        await _db.SaveChangesAsync(ct);
    }
}

public class ToggleUserActiveCommandHandler : IRequestHandler<ToggleUserActiveCommand>
{
    private readonly IApplicationDbContext _db;
    public ToggleUserActiveCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(ToggleUserActiveCommand request, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object[] { request.UserId }, ct)
            ?? throw new NotFoundException("User", request.UserId);

        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(new Domain.Entities.AuditLog
        {
            ActorId = request.AdminId,
            Action = request.IsActive ? "User.Activate" : "User.Deactivate",
            EntityType = "User",
            EntityId = user.Id,
            DiffJson = request.IsActive ? "Activated by admin" : "Deactivated by admin"
        });

        await _db.SaveChangesAsync(ct);
    }
}
