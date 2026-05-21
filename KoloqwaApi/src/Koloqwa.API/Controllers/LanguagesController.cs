using Koloqwa.Application.Common.Models;
using Koloqwa.Application.Features.Languages.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Koloqwa.API.Controllers;

[ApiController]
[Route("api/v1/languages")]
[Produces("application/json")]
public class LanguagesController : ControllerBase
{
    private readonly IMediator _mediator;
    public LanguagesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get all active languages.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLanguagesQuery(), ct);
        return Ok(ApiResponse<IEnumerable<LanguageDto>>.Ok(result));
    }
}