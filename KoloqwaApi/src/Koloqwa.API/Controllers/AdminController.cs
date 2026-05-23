using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.Common.Models;
using Koloqwa.Application.DTOs;
using Koloqwa.Application.Features.Admin.Commands;
using Koloqwa.Application.Features.Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly IApplicationDbContext _db;

    public AdminController(IMediator mediator, ICurrentUserService currentUser, IApplicationDbContext db)
    {
        _mediator = mediator; _currentUser = currentUser; _db = db;
    }

    // ── Stats ─────────────────────────────────────────────────────────────────

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAdminStatsQuery(), ct);
        return Ok(ApiResponse<AdminStatsDto>.Ok(result));
    }

    [HttpGet("analytics")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAnalytics([FromQuery] int days = 30, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAnalyticsQuery(days), ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // ── Submissions ───────────────────────────────────────────────────────────

    [HttpGet("submissions")]
    public async Task<IActionResult> GetSubmissions(
        [FromQuery] string? status,
        [FromQuery] string? entryType,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAdminSubmissionsQuery(status, entryType, search, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<AdminSubmissionDto>>.Ok(result));
    }

    [HttpGet("submissions/{id:guid}/detail")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct)
    {
        var submission = await _db.SubmissionQueues
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (submission is null)
            return NotFound(ApiResponse<object>.Fail("Submission not found."));

        if (submission.EntryType == Domain.Enums.SubmissionType.Word)
        {
            var word = await _db.WordEntries
                .Include(w => w.Definitions)
                    .ThenInclude(d => d.Examples)
                .Include(w => w.Language)
                .FirstOrDefaultAsync(w => w.Id == submission.EntryId, ct);

            if (word is null)
                return NotFound(ApiResponse<object>.Fail("Entry not found."));

            var def = word.Definitions.OrderBy(d => d.SortOrder).FirstOrDefault();
            var ex = def?.Examples.FirstOrDefault();

            return Ok(ApiResponse<object>.Ok(new
            {
                headword = word.Headword,
                pronunciation = word.Pronunciation,
                partOfSpeech = word.PartOfSpeech.ToString(),
                definition = def?.Definition,
                usageNote = def?.UsageNote,
                exampleSentence = ex?.Sentence,
                tags = word.Tags,
            }));
        }
        else
        {
            var phrase = await _db.PhraseEntries
                .Include(p => p.Meanings)
                .Include(p => p.Language)
                .FirstOrDefaultAsync(p => p.Id == submission.EntryId, ct);

            if (phrase is null)
                return NotFound(ApiResponse<object>.Fail("Entry not found."));

            var meaning = phrase.Meanings.OrderBy(m => m.SortOrder).FirstOrDefault();

            return Ok(ApiResponse<object>.Ok(new
            {
                phraseText = phrase.PhraseText,
                literalMeaning = phrase.LiteralMeaning,
                meaning = meaning?.Meaning,
                contextNote = meaning?.ContextNote,
                tags = phrase.Tags,
            }));
        }
    }

    [HttpPost("submissions/{id:guid}/review")]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewSubmissionRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ReviewSubmissionCommand(
            id, request.Action, request.AdminNote, _currentUser.UserId!.Value), ct);
        return Ok(ApiResponse<object>.Ok(null, $"Submission {request.Action}d successfully."));
    }

    [HttpDelete("submissions/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteEntryCommand(id, _currentUser.UserId!.Value), ct);
        return Ok(ApiResponse<object>.Ok(null, "Entry deleted successfully."));
    }

    [HttpPut("submissions/{id:guid}/word")]
    public async Task<IActionResult> EditWord(Guid id, [FromBody] EditWordEntryRequest request, CancellationToken ct)
    {
        await _mediator.Send(new EditWordEntryCommand(id, request, _currentUser.UserId!.Value), ct);
        return Ok(ApiResponse<object>.Ok(null, "Word updated."));
    }

    [HttpPut("submissions/{id:guid}/phrase")]
    public async Task<IActionResult> EditPhrase(Guid id, [FromBody] EditPhraseEntryRequest request, CancellationToken ct)
    {
        await _mediator.Send(new EditPhraseEntryCommand(id, request, _currentUser.UserId!.Value), ct);
        return Ok(ApiResponse<object>.Ok(null, "Phrase updated."));
    }

    // ── Users ─────────────────────────────────────────────────────────────────

    [HttpGet("users")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAdminUsersQuery(search, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<AdminUserDto>>.Ok(result));
    }

    [HttpPut("users/{id:guid}/role")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateUserRoleCommand(id, request.Role, _currentUser.UserId!.Value), ct);
        return Ok(ApiResponse<object>.Ok(null, "Role updated."));
    }

    [HttpPut("users/{id:guid}/active")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ToggleActive(Guid id, [FromBody] ToggleUserActiveRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ToggleUserActiveCommand(id, request.IsActive, _currentUser.UserId!.Value), ct);
        return Ok(ApiResponse<object>.Ok(null, request.IsActive ? "User activated." : "User deactivated."));
    }

    // ── Reports ───────────────────────────────────────────────────────────────

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports(
        [FromQuery] string? status,
        [FromQuery] string? reason,
        [FromQuery] int page = 1,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAdminReportsQuery(status, reason, page), ct);
        return Ok(ApiResponse<PagedResult<AdminReportDto>>.Ok(result));
    }

    [HttpPost("reports/{id:guid}/review")]
    public async Task<IActionResult> ReviewReport(Guid id, [FromBody] ReviewReportRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ReviewReportCommand(id, request.Action, _currentUser.UserId!.Value), ct);
        return Ok(ApiResponse<object>.Ok(null, $"Report {request.Action.ToLower()}d."));
    }

    // ── Suggestions ───────────────────────────────────────────────────────────

    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAdminSuggestionsQuery(status, page), ct);
        return Ok(ApiResponse<PagedResult<AdminSuggestionDto>>.Ok(result));
    }

    [HttpPost("suggestions/{id:guid}/review")]
    public async Task<IActionResult> ReviewSuggestion(Guid id, [FromBody] ReviewSuggestionRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ReviewSuggestionCommand(id, request.Action, request.AdminNote, _currentUser.UserId!.Value), ct);
        return Ok(ApiResponse<object>.Ok(null, $"Suggestion {request.Action.ToLower()}ed."));
    }
}

public record ReviewSubmissionRequest(string Action, string? AdminNote);
public record ReviewReportRequest(string Action);
public record ReviewSuggestionRequest(string Action, string? AdminNote);