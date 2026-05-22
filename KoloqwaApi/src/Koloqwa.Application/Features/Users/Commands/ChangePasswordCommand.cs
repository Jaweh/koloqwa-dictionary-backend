using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Domain.Exceptions;
using MediatR;

namespace Koloqwa.Application.Features.Users.Commands;

public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IApplicationDbContext _db;
    public ChangePasswordCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object[] { request.UserId }, ct)
            ?? throw new NotFoundException("User", request.UserId);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new DomainException("Current password is incorrect.");

        if (request.NewPassword.Length < 8)
            throw new DomainException("New password must be at least 8 characters.");
        if (!request.NewPassword.Any(char.IsUpper))
            throw new DomainException("New password must contain an uppercase letter.");
        if (!request.NewPassword.Any(char.IsDigit))
            throw new DomainException("New password must contain a number.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}
