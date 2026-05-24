using Koloqwa.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Auth.Commands;

public record ForgotPasswordCommand(string Email, string AppUrl) : IRequest;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IEmailService _email;

    public ForgotPasswordCommandHandler(IApplicationDbContext db, IEmailService email)
    {
        _db = db; _email = email;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim(), ct);

        if (user is null) return;

        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "-").Replace("/", "_").Replace("=", "");

        user.PasswordResetToken = token;
        user.PasswordResetExpiry = DateTime.UtcNow.AddHours(1);
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        var resetUrl = $"{request.AppUrl}/auth/reset-password?token={token}";
        await _email.SendPasswordResetEmailAsync(user.Email, user.DisplayName, resetUrl, ct);
    }
}