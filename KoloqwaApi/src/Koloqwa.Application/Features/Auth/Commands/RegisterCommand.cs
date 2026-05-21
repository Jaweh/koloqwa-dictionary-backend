using AutoMapper;
using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.DTOs;
using Koloqwa.Domain.Entities;
using Koloqwa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Auth.Commands;

public record RegisterCommand(RegisterRequest Request) : IRequest<AuthResponse>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordService _passwords;
    private readonly IJwtService _jwt;
    private readonly IMapper _mapper;

    public RegisterCommandHandler(IApplicationDbContext db, IPasswordService passwords,
        IJwtService jwt, IMapper mapper)
    {
        _db = db; _passwords = passwords; _jwt = jwt; _mapper = mapper;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        var req = request.Request;

        var emailExists = await _db.Users
            .AnyAsync(u => u.Email.ToLower() == req.Email.ToLower(), ct);
        if (emailExists)
            throw new ConflictException("An account with this email address already exists.");

        var user = new User
        {
            Email = req.Email.ToLower().Trim(),
            PasswordHash = _passwords.Hash(req.Password),
            DisplayName = req.DisplayName.Trim()
        };
        _db.Users.Add(user);

        var refreshTokenValue = _jwt.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenValue,  // hashed by service in prod
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        _db.RefreshTokens.Add(refreshToken);

        await _db.SaveChangesAsync(ct);

        var accessToken = _jwt.GenerateAccessToken(user);
        return new AuthResponse(
            accessToken,
            refreshTokenValue,
            DateTime.UtcNow.AddMinutes(15),
            _mapper.Map<UserDto>(user)
        );
    }
}
