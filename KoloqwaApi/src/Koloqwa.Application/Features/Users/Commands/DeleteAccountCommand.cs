using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Domain.Enums;
using Koloqwa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Users.Commands;

public record DeleteAccountCommand(Guid UserId) : IRequest;

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand>
{
    private readonly IApplicationDbContext _db;
    public DeleteAccountCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteAccountCommand request, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object[] { request.UserId }, ct)
            ?? throw new NotFoundException("User", request.UserId);

        if (user.Role == UserRole.Admin || user.Role == UserRole.SuperAdmin)
            throw new DomainException("Admin accounts cannot be deleted. Contact a SuperAdmin to remove your account.");

        // Anonymize submissions — set SubmitterId to null
        await _db.SubmissionQueues
            .Where(s => s.SubmitterId == request.UserId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.SubmitterId, (Guid?)null), ct);

        // Anonymize word entries
        await _db.WordEntries
            .Where(w => w.SubmittedById == request.UserId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.SubmittedById, (Guid?)null), ct);

        // Anonymize phrase entries
        await _db.PhraseEntries
            .Where(p => p.SubmittedById == request.UserId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.SubmittedById, (Guid?)null), ct);

        // Remove refresh tokens
        await _db.RefreshTokens
            .Where(t => t.UserId == request.UserId)
            .ExecuteDeleteAsync(ct);

        _db.Users.Remove(user);
        await _db.SaveChangesAsync(ct);
    }
}