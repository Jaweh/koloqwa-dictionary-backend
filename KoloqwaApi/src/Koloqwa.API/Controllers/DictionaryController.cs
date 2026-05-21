using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.Common.Models;
using Koloqwa.Application.DTOs;
using Koloqwa.Application.Features.Words.Commands;
using Koloqwa.Application.Features.Words.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koloqwa.API.Controllers;

[ApiController]
[Route("api/v1/words")]
[Produces("application/json")]
public class DictionaryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public DictionaryController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator; _currentUser = currentUser;
    }

    /// <summary>Search published words. No authentication required.</summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<WordSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] string? lang,
        [FromQuery] string? pos,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new SearchWordsQuery(q, lang, pos, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<WordSummaryDto>>.Ok(result));
    }

    /// <summary>Get a published word entry by slug.</summary>
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(ApiResponse<WordDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetWordBySlugQuery(slug), ct);
        return Ok(ApiResponse<WordDetailDto>.Ok(result));
    }

    /// <summary>Submit a new word entry (goes to approval queue).</summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Submit([FromBody] CreateWordRequest request, CancellationToken ct)
    {
        var id = await _mediator.Send(new CreateWordCommand(request, _currentUser.UserId!.Value), ct);
        return Accepted(ApiResponse<object>.Ok(new { id }, "Word submitted for review."));
    }
}
