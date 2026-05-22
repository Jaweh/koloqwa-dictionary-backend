using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Users.Commands;

public record UpdateProfileCommand(
    Guid UserId,
    string? DisplayName,
    string? Email
) : IRequest;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand>
{
    private readonly IApplicationDbContext _db;
    public UpdateProfileCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object[] { request.UserId }, ct)
            ?? throw new NotFoundException("User", request.UserId);

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            if (request.DisplayName.Trim().Length < 2)
                throw new DomainException("Display name must be at least 2 characters.");
            user.DisplayName = request.DisplayName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailTaken = await _db.Users
                .AnyAsync(u => u.Email == request.Email.Trim().ToLower() && u.Id != request.UserId, ct);
            if (emailTaken)
                throw new DomainException("This email address is already in use.");
            user.Email = request.Email.Trim().ToLower();
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}
