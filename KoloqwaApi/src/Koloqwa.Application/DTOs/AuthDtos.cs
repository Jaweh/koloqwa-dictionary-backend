using System.ComponentModel.DataAnnotations;

namespace Koloqwa.Application.DTOs;

public record RegisterRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(8)] string Password,
    [Required][MinLength(2)][MaxLength(80)] string DisplayName
);

public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password
);

public record RefreshTokenRequest([Required] string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    UserDto User
);

public record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required][MinLength(8)] string NewPassword
);
