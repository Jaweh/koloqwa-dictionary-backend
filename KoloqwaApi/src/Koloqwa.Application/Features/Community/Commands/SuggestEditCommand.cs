using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Domain.Entities;
using Koloqwa.Domain.Exceptions;
using MediatR;

namespace Koloqwa.Application.Features.Community.Commands;

public record SuggestEditCommand(
    Guid EntryId,
    string EntryType,
    string Field,
    string CurrentValue,
    string SuggestedValue,
    string? Notes,
    Guid UserId
) : IRequest<Guid>;

public class SuggestEditCommandHandler : IRequestHandler<SuggestEditCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    public SuggestEditCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Guid> Handle(SuggestEditCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.SuggestedValue))
            throw new DomainException("Suggested value cannot be empty.");

        var suggestion = new WordSuggestion
        {
            EntryId = request.EntryId,
            EntryType = request.EntryType,
            Field = request.Field,
            CurrentValue = request.CurrentValue,
            SuggestedValue = request.SuggestedValue.Trim(),
            Notes = request.Notes?.Trim(),
            SuggestedById = request.UserId,
            Status = "Pending",
            CreatedBy = request.UserId
        };

        _db.WordSuggestions.Add(suggestion);
        await _db.SaveChangesAsync(ct);
        return suggestion.Id;
    }
}
