using AutoMapper;
using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.DTOs;
using Koloqwa.Domain.Entities;
using Koloqwa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Auth.Commands;

public record LoginCommand(LoginRequest Request) : IRequest<AuthResponse>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordService _passwords;
    private readonly IJwtService _jwt;
    private readonly IMapper _mapper;

    public LoginCommandHandler(IApplicationDbContext db, IPasswordService passwords,
        IJwtService jwt, IMapper mapper)
    {
        _db = db; _passwords = passwords; _jwt = jwt; _mapper = mapper;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var req = request.Request;

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == req.Email.ToLower().Trim(), ct);

        if (user is null || !_passwords.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedException("Your account has been suspended.");

        // Revoke old refresh tokens for this user (single session strategy)
        var oldTokens = await _db.RefreshTokens
            .Where(t => t.UserId == user.Id && t.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var t in oldTokens) t.RevokedAt = DateTime.UtcNow;

        var refreshTokenValue = _jwt.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync(ct);

        return new AuthResponse(
            _jwt.GenerateAccessToken(user),
            refreshTokenValue,
            DateTime.UtcNow.AddMinutes(15),
            _mapper.Map<UserDto>(user)
        );
    }
}
