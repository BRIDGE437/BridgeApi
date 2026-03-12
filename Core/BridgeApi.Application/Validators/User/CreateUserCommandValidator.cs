using BridgeApi.Application.Features.Commands.User.CreateUser;
using BridgeApi.Domain.Enums;
using FluentValidation;

namespace BridgeApi.Application.Validators.User;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommandRequest>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role.");
    }
}
