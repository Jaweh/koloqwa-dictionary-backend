using FluentValidation;
using Koloqwa.Application.DTOs;

namespace Koloqwa.Application.Validators;

public class CreatePhraseRequestValidator : AbstractValidator<CreatePhraseRequest>
{
    public CreatePhraseRequestValidator()
    {
        RuleFor(x => x.LanguageId).NotEmpty().WithMessage("Language is required.");
        RuleFor(x => x.PhraseText)
            .NotEmpty().MinimumLength(2).MaximumLength(500);
        RuleFor(x => x.Meanings)
            .NotEmpty().WithMessage("At least one meaning is required.");
        RuleForEach(x => x.Meanings).ChildRules(m =>
        {
            m.RuleFor(x => x.Meaning).NotEmpty().MinimumLength(5).MaximumLength(1000);
        });
    }
}
