using BridgeApi.Application.Features.Commands.Intent.UpdateIntent;
using FluentValidation;

namespace BridgeApi.Application.Validators.Intent;

public class UpdateIntentCommandValidator : AbstractValidator<UpdateIntentCommandRequest>
{
    public UpdateIntentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Intent ID is required.");

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
