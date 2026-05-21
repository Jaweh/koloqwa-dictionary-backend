using FluentValidation;
using Koloqwa.Application.DTOs;

namespace Koloqwa.Application.Validators;

public class CreatePhraseRequestValidator : AbstractValidator<CreatePhraseRequest>
{
    public CreatePhraseRequestValidator()
    {
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .Must(c => c == "Vernacular" || c == "Tribal")
            .WithMessage("Category must be 'Vernacular' or 'Tribal'.");

        RuleFor(x => x.LanguageId)
            .NotNull().WithMessage("LanguageId is required for Tribal entries.")
            .When(x => x.Category == "Tribal");

        RuleFor(x => x.LanguageId)
            .Null().WithMessage("LanguageId must be empty for Vernacular entries.")
            .When(x => x.Category == "Vernacular");

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
