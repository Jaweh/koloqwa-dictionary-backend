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

        // Anonymize submissions — keep the entries, remove the link to this user
        var submissions = await _db.SubmissionQueues
            .Where(s => s.SubmitterId == request.UserId)
            .ToListAsync(ct);

        foreach (var s in submissions)
            s.SubmitterId = Guid.Empty;

        // Anonymize word entries submitted by this user
        var words = await _db.WordEntries
            .Where(w => w.SubmittedById == request.UserId)
            .ToListAsync(ct);

        foreach (var w in words)
            w.SubmittedById = null;

        // Anonymize phrase entries submitted by this user
        var phrases = await _db.PhraseEntries
            .Where(p => p.SubmittedById == request.UserId)
            .ToListAsync(ct);

        foreach (var p in phrases)
            p.SubmittedById = null;

        // Remove refresh tokens
        var tokens = await _db.RefreshTokens
            .Where(t => t.UserId == request.UserId)
            .ToListAsync(ct);
        _db.RefreshTokens.RemoveRange(tokens);

        _db.Users.Remove(user);
        await _db.SaveChangesAsync(ct);
    }
}