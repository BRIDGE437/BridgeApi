using BridgeApi.Application.Features.Commands.Auth.RefreshToken;
using FluentValidation;

namespace BridgeApi.Application.Validators.Auth;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommandRequest>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access token is required.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
