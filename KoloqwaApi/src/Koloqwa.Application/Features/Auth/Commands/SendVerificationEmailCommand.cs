using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Auth.Commands;

public record SendVerificationEmailCommand(Guid UserId, string AppUrl) : IRequest;

public class SendVerificationEmailCommandHandler : IRequestHandler<SendVerificationEmailCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IEmailService _email;

    public SendVerificationEmailCommandHandler(IApplicationDbContext db, IEmailService email)
    {
        _db = db; _email = email;
    }

    public async Task Handle(SendVerificationEmailCommand request, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object[] { request.UserId }, ct)
            ?? throw new NotFoundException("User", request.UserId);

        if (user.EmailVerified)
            throw new DomainException("Email is already verified.");

        // Generate a secure token
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "-").Replace("/", "_").Replace("=", "");

        user.EmailVerificationToken = token;
        user.EmailVerificationExpiry = DateTime.UtcNow.AddHours(24);
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        var verificationUrl = $"{request.AppUrl}/auth/verify-email?token={token}";
        await _email.SendVerificationEmailAsync(user.Email, user.DisplayName, verificationUrl, ct);
    }
}
