using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Features.Admin.Commands;

public record ReviewSuggestionCommand(
    Guid SuggestionId,
    string Action, // "Accept" or "Reject"
    string? AdminNote,
    Guid AdminId
) : IRequest;

public class ReviewSuggestionCommandHandler : IRequestHandler<ReviewSuggestionCommand>
{
    private readonly IApplicationDbContext _db;
    public ReviewSuggestionCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(ReviewSuggestionCommand request, CancellationToken ct)
    {
        var suggestion = await _db.WordSuggestions
            .FirstOrDefaultAsync(s => s.Id == request.SuggestionId, ct)
            ?? throw new NotFoundException("Suggestion", request.SuggestionId);

        suggestion.Status = request.Action == "Accept" ? "Accepted" : "Rejected";
        suggestion.AdminNote = request.AdminNote;
        suggestion.ReviewedById = request.AdminId;
        suggestion.ReviewedAt = DateTime.UtcNow;
        suggestion.UpdatedAt = DateTime.UtcNow;

        // If accepted, apply the change to the actual entry
        if (request.Action == "Accept")
        {
            if (suggestion.EntryType == "Word")
            {
                var word = await _db.WordEntries
                    .Include(w => w.Definitions)
                    .FirstOrDefaultAsync(w => w.Id == suggestion.EntryId, ct);

                if (word != null)
                {
                    switch (suggestion.Field)
                    {
                        case "Headword":
                            word.Headword = suggestion.SuggestedValue;
                            break;
                        case "Pronunciation":
                            word.Pronunciation = suggestion.SuggestedValue;
                            break;
                        default:
                            // Definition fields
                            if (suggestion.Field.StartsWith("Definition"))
                            {
                                var def = word.Definitions.OrderBy(d => d.SortOrder).FirstOrDefault();
                                if (def != null) def.Definition = suggestion.SuggestedValue;
                            }
                            break;
                    }
                    word.UpdatedAt = DateTime.UtcNow;
                    word.UpdatedBy = request.AdminId;
                }
            }
            else
            {
                var phrase = await _db.PhraseEntries
                    .Include(p => p.Meanings)
                    .FirstOrDefaultAsync(p => p.Id == suggestion.EntryId, ct);

                if (phrase != null)
                {
                    switch (suggestion.Field)
                    {
                        case "Phrase":
                            phrase.PhraseText = suggestion.SuggestedValue;
                            break;
                        case "Literal meaning":
                            phrase.LiteralMeaning = suggestion.SuggestedValue;
                            break;
                        default:
                            if (suggestion.Field.StartsWith("Meaning"))
                            {
                                var meaning = phrase.Meanings.OrderBy(m => m.SortOrder).FirstOrDefault();
                                if (meaning != null) meaning.Meaning = suggestion.SuggestedValue;
                            }
                            break;
                    }
                    phrase.UpdatedAt = DateTime.UtcNow;
                    phrase.UpdatedBy = request.AdminId;
                }
            }
        }

        _db.AuditLogs.Add(new Domain.Entities.AuditLog
        {
            ActorId = request.AdminId,
            Action = $"Suggestion.{request.Action}",
            EntityType = suggestion.EntryType,
            EntityId = suggestion.EntryId,
            DiffJson = request.AdminNote ?? $"Suggestion {request.Action.ToLower()}ed"
        });

        await _db.SaveChangesAsync(ct);
    }
}
