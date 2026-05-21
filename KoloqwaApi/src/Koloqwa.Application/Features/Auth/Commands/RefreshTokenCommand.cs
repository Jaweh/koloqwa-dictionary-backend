using AutoMapper;
using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.DTOs;
using Koloqwa.Domain.Entities;
using Koloqwa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string Token) : IRequest<AuthResponse>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IJwtService _jwt;
    private readonly IMapper _mapper;

    public RefreshTokenCommandHandler(IApplicationDbContext db, IJwtService jwt, IMapper mapper)
    {
        _db = db; _jwt = jwt; _mapper = mapper;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var existing = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == request.Token, ct);

        if (existing is null || !existing.IsActive)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        // Rotate: revoke old, issue new
        existing.RevokedAt = DateTime.UtcNow;

        var newTokenValue = _jwt.GenerateRefreshToken();
        var newToken = new RefreshToken
        {
            UserId = existing.UserId,
            TokenHash = newTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        _db.RefreshTokens.Add(newToken);
        await _db.SaveChangesAsync(ct);

        return new AuthResponse(
            _jwt.GenerateAccessToken(existing.User),
            newTokenValue,
            DateTime.UtcNow.AddMinutes(15),
            _mapper.Map<UserDto>(existing.User)
        );
    }
}
