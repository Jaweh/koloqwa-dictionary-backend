using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.Common.Models;
using Koloqwa.Application.DTOs;
using Koloqwa.Application.Features.Phrases.Commands;
using Koloqwa.Application.Features.Phrases.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koloqwa.API.Controllers;

[ApiController]
[Route("api/v1/phrases")]
[Produces("application/json")]
public class PhraseController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public PhraseController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator; _currentUser = currentUser;
    }

    /// <summary>Search published phrases. No authentication required.</summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PhraseSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] string? lang,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new SearchPhrasesQuery(q, lang, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<PhraseSummaryDto>>.Ok(result));
    }

    /// <summary>Get a published phrase entry by slug. No authentication required.</summary>
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(ApiResponse<PhraseDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPhraseBySlugQuery(slug), ct);
        return Ok(ApiResponse<PhraseDetailDto>.Ok(result));
    }

    /// <summary>Submit a new phrase entry (goes to approval queue).</summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Submit([FromBody] CreatePhraseRequest request, CancellationToken ct)
    {
        var id = await _mediator.Send(new CreatePhraseCommand(request, _currentUser.UserId!.Value), ct);
        return Accepted(ApiResponse<object>.Ok(new { id }, "Phrase submitted for review."));
    }
}
