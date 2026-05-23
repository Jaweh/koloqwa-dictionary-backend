using Koloqwa.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Community.Queries;

// Returns vote counts, favourite status, and report status for the current user
public record GetEntryContextQuery(
    Guid EntryId,
    string EntryType,
    Guid? UserId
) : IRequest<EntryContextDto>;

public record DefinitionVoteDto(Guid DefinitionId, int VoteCount, bool UserHasVoted);

public record EntryContextDto(
    bool IsFavourited,
    bool HasReported,
    List<DefinitionVoteDto> DefinitionVotes
);

public class GetEntryContextQueryHandler : IRequestHandler<GetEntryContextQuery, EntryContextDto>
{
    private readonly IApplicationDbContext _db;
    public GetEntryContextQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<EntryContextDto> Handle(GetEntryContextQuery request, CancellationToken ct)
    {
        bool isFavourited = false;
        bool hasReported = false;

        if (request.UserId.HasValue)
        {
            isFavourited = await _db.UserFavourites.AnyAsync(f =>
                f.UserId == request.UserId.Value &&
                f.EntryId == request.EntryId, ct);

            hasReported = await _db.WordReports.AnyAsync(r =>
                r.ReportedById == request.UserId.Value &&
                r.EntryId == request.EntryId, ct);
        }

        // Get definition IDs for this entry
        List<DefinitionVoteDto> defVotes = new();

        if (request.EntryType == "Word")
        {
            var defIds = await _db.WordDefinitions
                .Where(d => d.WordEntryId == request.EntryId)
                .Select(d => d.Id)
                .ToListAsync(ct);

            foreach (var defId in defIds)
            {
                var count = await _db.DefinitionVotes.CountAsync(v => v.DefinitionId == defId, ct);
                var userVoted = request.UserId.HasValue &&
                    await _db.DefinitionVotes.AnyAsync(v =>
                        v.DefinitionId == defId && v.UserId == request.UserId.Value, ct);
                defVotes.Add(new DefinitionVoteDto(defId, count, userVoted));
            }
        }

        return new EntryContextDto(isFavourited, hasReported, defVotes);
    }
}
