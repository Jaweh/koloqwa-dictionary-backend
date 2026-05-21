using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.Common.Models;
using Koloqwa.Application.DTOs;
using Koloqwa.Application.Features.Admin.Queries;
using Koloqwa.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.API.Controllers;

[ApiController]
[Route("api/v1/submissions")]
[Authorize]
[Produces("application/json")]
public class SubmissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly IApplicationDbContext _db;

    public SubmissionsController(IMediator mediator, ICurrentUserService currentUser, IApplicationDbContext db)
    {
        _mediator = mediator; _currentUser = currentUser; _db = db;
    }

    /// <summary>Get the current user's own submission history.</summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SubmissionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetMySubmissionsQuery(_currentUser.UserId!.Value, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<SubmissionDto>>.Ok(result));
    }

    /// <summary>Cancel a pending submission. Only the submitter can cancel their own pending submissions.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var submission = await _db.SubmissionQueues
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (submission is null)
            return NotFound(ApiResponse<object>.Fail("Submission not found."));

        if (submission.SubmitterId != _currentUser.UserId!.Value)
            return StatusCode(403, ApiResponse<object>.Fail("You can only cancel your own submissions."));

        if (submission.Status != EntryStatus.PendingReview)
            return BadRequest(ApiResponse<object>.Fail("Only pending submissions can be cancelled."));

        // Remove the submission queue entry
        _db.SubmissionQueues.Remove(submission);

        // Also remove the underlying word or phrase entry
        if (submission.EntryType == SubmissionType.Word)
        {
            var word = await _db.WordEntries.FindAsync(new object[] { submission.EntryId }, ct);
            if (word != null) _db.WordEntries.Remove(word);
        }
        else if (submission.EntryType == SubmissionType.Phrase)
        {
            var phrase = await _db.PhraseEntries.FindAsync(new object[] { submission.EntryId }, ct);
            if (phrase != null) _db.PhraseEntries.Remove(phrase);
        }

        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(null, "Submission cancelled successfully."));
    }
}