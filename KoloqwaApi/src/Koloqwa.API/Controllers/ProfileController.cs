using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.Common.Models;
using Koloqwa.Application.Features.Users.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koloqwa.API.Controllers;

[ApiController]
[Route("api/v1/profile")]
[Authorize]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly IApplicationDbContext _db;

    public ProfileController(IMediator mediator, ICurrentUserService currentUser, IApplicationDbContext db)
    {
        _mediator = mediator; _currentUser = currentUser; _db = db;
    }

    /// <summary>Get current user's profile.</summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object[] { _currentUser.UserId!.Value }, ct);
        if (user is null) return NotFound();
        return Ok(ApiResponse<object>.Ok(new {
            id = user.Id,
            email = user.Email,
            displayName = user.DisplayName,
            role = user.Role.ToString(),
            isActive = user.IsActive,
            createdAt = user.CreatedAt,
        }));
    }

    /// <summary>Update display name and/or email.</summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateProfileCommand(
            _currentUser.UserId!.Value,
            request.DisplayName,
            request.Email), ct);

        // Return updated user data so frontend can refresh auth state
        var user = await _db.Users.FindAsync(new object[] { _currentUser.UserId!.Value }, ct);
        return Ok(ApiResponse<object>.Ok(new {
            id = user!.Id,
            email = user.Email,
            displayName = user.DisplayName,
            role = user.Role.ToString(),
            isActive = user.IsActive,
            createdAt = user.CreatedAt,
        }, "Profile updated successfully."));
    }

    /// <summary>Change password.</summary>
    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ChangePasswordCommand(
            _currentUser.UserId!.Value,
            request.CurrentPassword,
            request.NewPassword), ct);
        return Ok(ApiResponse<object>.Ok(null, "Password changed successfully."));
    }
}

public record UpdateProfileRequest(string? DisplayName, string? Email);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
