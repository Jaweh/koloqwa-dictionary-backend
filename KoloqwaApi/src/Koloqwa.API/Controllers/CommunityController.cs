using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.Common.Models;
using Koloqwa.Application.Features.Community.Commands;
using Koloqwa.Application.Features.Community.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koloqwa.API.Controllers;

[ApiController]
[Route("api/v1/community")]
[Produces("application/json")]
public class CommunityController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public CommunityController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator; _currentUser = currentUser;
    }

    /// <summary>Get vote counts, favourite status for an entry. Auth optional.</summary>
    [HttpGet("context/{entryType}/{entryId:guid}")]
    public async Task<IActionResult> GetContext(string entryType, Guid entryId, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetEntryContextQuery(entryId, entryType, _currentUser.UserId), ct);
        return Ok(ApiResponse<EntryContextDto>.Ok(result));
    }

    /// <summary>Suggest an edit to a word or phrase. Requires auth.</summary>
    [HttpPost("suggest")]
    [Authorize]
    public async Task<IActionResult> SuggestEdit([FromBody] SuggestEditRequest request, CancellationToken ct)
    {
        var id = await _mediator.Send(new SuggestEditCommand(
            request.EntryId, request.EntryType, request.Field,
            request.CurrentValue, request.SuggestedValue,
            request.Notes, _currentUser.UserId!.Value), ct);
        return Ok(ApiResponse<object>.Ok(new { id }, "Edit suggestion submitted."));
    }

    /// <summary>Report a word or phrase. Requires auth.</summary>
    [HttpPost("report")]
    [Authorize]
    public async Task<IActionResult> Report([FromBody] ReportEntryRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ReportEntryCommand(
            request.EntryId, request.EntryType,
            request.Reason, request.Notes,
            _currentUser.UserId!.Value), ct);
        return Ok(ApiResponse<object>.Ok(null, "Report submitted. Thank you."));
    }

    /// <summary>Toggle upvote on a definition. Requires auth.</summary>
    [HttpPost("vote/{definitionId:guid}")]
    [Authorize]
    public async Task<IActionResult> Vote(Guid definitionId, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new VoteDefinitionCommand(definitionId, _currentUser.UserId!.Value), ct);
        return Ok(ApiResponse<VoteResult>.Ok(result));
    }

    /// <summary>Toggle favourite on a word or phrase. Requires auth.</summary>
    [HttpPost("favourite")]
    [Authorize]
    public async Task<IActionResult> ToggleFavourite([FromBody] FavouriteRequest request, CancellationToken ct)
    {
        var isFavourited = await _mediator.Send(
            new ToggleFavouriteCommand(request.EntryId, request.EntryType, _currentUser.UserId!.Value), ct);
        return Ok(ApiResponse<object>.Ok(new { isFavourited },
            isFavourited ? "Added to favourites." : "Removed from favourites."));
    }

    /// <summary>Get current user's favourites. Requires auth.</summary>
    [HttpGet("favourites")]
    [Authorize]
    public async Task<IActionResult> GetFavourites(
        [FromQuery] int page = 1, CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetUserFavouritesQuery(_currentUser.UserId!.Value, page), ct);
        return Ok(ApiResponse<PagedResult<FavouriteItemDto>>.Ok(result));
    }
}

public record SuggestEditRequest(
    Guid EntryId, string EntryType, string Field,
    string CurrentValue, string SuggestedValue, string? Notes);

public record ReportEntryRequest(
    Guid EntryId, string EntryType, string Reason, string? Notes);

public record FavouriteRequest(Guid EntryId, string EntryType);
