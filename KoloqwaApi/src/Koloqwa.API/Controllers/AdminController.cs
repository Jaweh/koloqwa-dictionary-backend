using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.Common.Models;
using Koloqwa.Application.DTOs;
using Koloqwa.Application.Features.Admin.Commands;
using Koloqwa.Application.Features.Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koloqwa.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public AdminController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator; _currentUser = currentUser;
    }

    /// <summary>Get paginated submissions queue.</summary>
    [HttpGet("submissions")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SubmissionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubmissions(
        [FromQuery] string? status,
        [FromQuery] string? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetSubmissionsQuery(status, type, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<SubmissionDto>>.Ok(result));
    }

    /// <summary>Approve or reject a submission.</summary>
    [HttpPost("submissions/{id}/review")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReviewSubmission(
        Guid id, [FromBody] ReviewSubmissionRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ReviewSubmissionCommand(id, request, _currentUser.UserId!.Value), ct);
        return Ok(ApiResponse<object>.Ok(null, $"Submission {request.Decision}d successfully."));
    }

    /// <summary>Get dashboard statistics.</summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDashboardStatsQuery(), ct);
        return Ok(ApiResponse<DashboardStatsDto>.Ok(result));
    }
}
