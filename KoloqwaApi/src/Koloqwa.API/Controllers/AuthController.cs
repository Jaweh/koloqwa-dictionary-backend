using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.Common.Models;
using Koloqwa.Application.DTOs;
using Koloqwa.Application.Features.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Koloqwa.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly IConfiguration _config;

    public AuthController(IMediator mediator, ICurrentUserService currentUser, IConfiguration config)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _config = config;
    }

    /// <summary>Register a new user account.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var appUrl = _config["AppUrl"] ?? "http://localhost:3000";
        var result = await _mediator.Send(new RegisterCommand(request, appUrl), ct);
        return CreatedAtAction(nameof(Register), ApiResponse<AuthResponse>.Ok(result, "Account created successfully. Please check your email to verify your account."));
    }

    /// <summary>Authenticate and receive JWT tokens.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new LoginCommand(request), ct);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    /// <summary>Rotate refresh token and receive new access token.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken), ct);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    /// <summary>Verify email address using token from email link.</summary>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token, CancellationToken ct)
    {
        await _mediator.Send(new VerifyEmailCommand(token), ct);
        return Ok(ApiResponse<object>.Ok(null, "Email verified successfully. You can now submit words and phrases."));
    }

    /// <summary>Resend verification email to current user.</summary>
    [HttpPost("resend-verification")]
    [Authorize]
    public async Task<IActionResult> ResendVerification(CancellationToken ct)
    {
        var appUrl = _config["AppUrl"] ?? "http://localhost:3000";
        await _mediator.Send(new SendVerificationEmailCommand(_currentUser.UserId!.Value, appUrl), ct);
        return Ok(ApiResponse<object>.Ok(null, "Verification email sent. Please check your inbox."));
    }
}