using BridgeApi.Application.Features.Commands.Auth.ResetPassword;
using FluentValidation;

namespace BridgeApi.Application.Validators.Auth;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommandRequest>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(254);

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.")
            .MaximumLength(4096);

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters.");
    }
}
