using System.Text.Json;
using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Domain.Entities;

namespace Koloqwa.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AuditService(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db; _currentUser = currentUser;
    }

    public async Task LogAsync(string action, string entityType, Guid entityId,
        object? before, object? after, CancellationToken ct = default)
    {
        var diff = before is null && after is null ? null :
            JsonSerializer.Serialize(new { Before = before, After = after });

        _db.AuditLogs.Add(new AuditLog
        {
            ActorId = _currentUser.UserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            DiffJson = diff
        });

        await _db.SaveChangesAsync(ct);
    }
}
