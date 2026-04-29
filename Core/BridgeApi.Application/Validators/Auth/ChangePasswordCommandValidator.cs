using BridgeApi.Application.Features.Commands.Auth.ChangePassword;
using FluentValidation;

namespace BridgeApi.Application.Validators.Auth;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommandRequest>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8).WithMessage("New password must be at least 8 characters.");
        RuleFor(x => x)
            .Must(x => x.CurrentPassword != x.NewPassword)
            .WithMessage("New password must differ from current password.")
            .WithName("NewPassword");
    }
}
