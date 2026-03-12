using BridgeApi.Application.Features.Commands.Auth.Login;
using FluentValidation;

namespace BridgeApi.Application.Validators.Auth;

public class LoginCommandValidator : AbstractValidator<LoginCommandRequest>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty().WithMessage("Username or email is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
