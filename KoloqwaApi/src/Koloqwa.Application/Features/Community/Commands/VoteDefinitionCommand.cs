using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Community.Commands;

public record VoteDefinitionCommand(Guid DefinitionId, Guid UserId) : IRequest<VoteResult>;
public record VoteResult(bool Voted, int TotalVotes);

public class VoteDefinitionCommandHandler : IRequestHandler<VoteDefinitionCommand, VoteResult>
{
    private readonly IApplicationDbContext _db;
    public VoteDefinitionCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<VoteResult> Handle(VoteDefinitionCommand request, CancellationToken ct)
    {
        var existing = await _db.DefinitionVotes
            .FirstOrDefaultAsync(v => v.DefinitionId == request.DefinitionId && v.UserId == request.UserId, ct);

        bool voted;
        if (existing != null)
        {
            // Toggle off
            _db.DefinitionVotes.Remove(existing);
            voted = false;
        }
        else
        {
            _db.DefinitionVotes.Add(new DefinitionVote
            {
                DefinitionId = request.DefinitionId,
                UserId = request.UserId
            });
            voted = true;
        }

        await _db.SaveChangesAsync(ct);

        var total = await _db.DefinitionVotes
            .CountAsync(v => v.DefinitionId == request.DefinitionId, ct);

        return new VoteResult(voted, total);
    }
}
