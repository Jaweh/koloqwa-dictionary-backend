using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Auth.Commands;

public record VerifyEmailCommand(string Token) : IRequest;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand>
{
    private readonly IApplicationDbContext _db;
    public VerifyEmailCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(VerifyEmailCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u =>
                u.EmailVerificationToken == request.Token &&
                u.EmailVerificationExpiry > DateTime.UtcNow, ct)
            ?? throw new DomainException("This verification link is invalid or has expired.");

        user.EmailVerified = true;
        user.EmailVerifiedAt = DateTime.UtcNow;
        user.EmailVerificationToken = null;
        user.EmailVerificationExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }
}
