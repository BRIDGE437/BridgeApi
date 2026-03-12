using BridgeApi.Application.Features.Commands.User.UpdateUser;
using FluentValidation;

namespace BridgeApi.Application.Validators.User;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommandRequest>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Username)
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.Username));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role.")
            .When(x => x.Role.HasValue);
    }
}
