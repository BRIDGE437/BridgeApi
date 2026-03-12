using BridgeApi.Application.Features.Commands.Auth.GoogleLogin;
using FluentValidation;

namespace BridgeApi.Application.Validators.Auth;

public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommandRequest>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Google ID Token is required.")
            .MinimumLength(10).WithMessage("Invalid Google ID Token format.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role.");
    }
}
