using BridgeApi.Application.Features.Commands.Admin.ChangeUserRole;
using FluentValidation;

namespace BridgeApi.Application.Validators.Admin;

public class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommandRequest>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NewRole).IsInEnum();
    }
}
