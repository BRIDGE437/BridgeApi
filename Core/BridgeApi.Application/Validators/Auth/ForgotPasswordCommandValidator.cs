using BridgeApi.Application.Features.Commands.Auth.ForgotPassword;
using FluentValidation;

namespace BridgeApi.Application.Validators.Auth;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommandRequest>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(254).WithMessage("Email is too long.");
    }
}
