using System.Security.Claims;
using Koloqwa.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Koloqwa.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http) => _http = http;

    public Guid? UserId
    {
        get
        {
            var value = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Role =>
        _http.HttpContext?.User.FindFirstValue(ClaimTypes.Role);

    public bool IsAuthenticated =>
        _http.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
