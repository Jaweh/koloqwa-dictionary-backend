namespace Koloqwa.Application.DTOs;

public record UpdateUserRoleRequest(string Role);
public record UpdateUserStatusRequest(bool IsActive);

public record DashboardStatsDto(
    int TotalWords,
    int TotalPhrases,
    int PendingSubmissions,
    int TotalUsers,
    int PublishedToday
);
