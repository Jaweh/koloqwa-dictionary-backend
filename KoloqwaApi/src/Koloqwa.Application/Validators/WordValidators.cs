using FluentValidation;
using Koloqwa.Application.DTOs;

namespace Koloqwa.Application.Validators;

public class CreateWordRequestValidator : AbstractValidator<CreateWordRequest>
{
    public CreateWordRequestValidator()
    {
        RuleFor(x => x.LanguageId).NotEmpty().WithMessage("Language is required.");
        RuleFor(x => x.Headword).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Definitions)
            .NotEmpty().WithMessage("At least one definition is required.")
            .Must(d => d.Count <= 10).WithMessage("A word may have at most 10 definitions.");
        RuleForEach(x => x.Definitions).SetValidator(new CreateDefinitionRequestValidator());
        RuleFor(x => x.Tags)
            .Must(t => t == null || t.Count <= 20).WithMessage("At most 20 tags allowed.");
    }
}

public class CreateDefinitionRequestValidator : AbstractValidator<CreateDefinitionRequest>
{
    public CreateDefinitionRequestValidator()
    {
        RuleFor(x => x.Definition)
            .NotEmpty().WithMessage("Definition text is required.")
            .MinimumLength(5).WithMessage("Definition must be at least 5 characters.")
            .MaximumLength(1000);
        RuleFor(x => x.Register).MaximumLength(50);
    }
}
