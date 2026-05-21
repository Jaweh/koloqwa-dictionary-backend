namespace Koloqwa.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, Guid entityId,
        object? before, object? after, CancellationToken cancellationToken = default);
}
