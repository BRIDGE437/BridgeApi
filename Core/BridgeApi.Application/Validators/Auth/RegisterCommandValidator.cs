using BridgeApi.Application.Features.Commands.Auth.Register;
using FluentValidation;

namespace BridgeApi.Application.Validators.Auth;

public class RegisterCommandValidator : AbstractValidator<RegisterCommandRequest>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role.");
    }
}
