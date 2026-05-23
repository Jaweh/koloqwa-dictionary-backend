using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Community.Commands;

public record ToggleFavouriteCommand(Guid EntryId, string EntryType, Guid UserId) : IRequest<bool>;

public class ToggleFavouriteCommandHandler : IRequestHandler<ToggleFavouriteCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public ToggleFavouriteCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(ToggleFavouriteCommand request, CancellationToken ct)
    {
        var existing = await _db.UserFavourites
            .FirstOrDefaultAsync(f =>
                f.UserId == request.UserId &&
                f.EntryId == request.EntryId, ct);

        if (existing != null)
        {
            _db.UserFavourites.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return false; // removed from favourites
        }

        _db.UserFavourites.Add(new UserFavourite
        {
            UserId = request.UserId,
            EntryId = request.EntryId,
            EntryType = request.EntryType
        });

        await _db.SaveChangesAsync(ct);
        return true; // added to favourites
    }
}
