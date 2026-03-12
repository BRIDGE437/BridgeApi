using BridgeApi.Application.Features.Commands.Connection.UpdateConnection;
using FluentValidation;

namespace BridgeApi.Application.Validators.Connection;

public class UpdateConnectionCommandValidator : AbstractValidator<UpdateConnectionCommandRequest>
{
    public UpdateConnectionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Connection ID is required.");
    }
}
