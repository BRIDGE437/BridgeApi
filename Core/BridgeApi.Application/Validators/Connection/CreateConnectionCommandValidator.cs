using BridgeApi.Application.Features.Commands.Connection.CreateConnection;
using FluentValidation;

namespace BridgeApi.Application.Validators.Connection;

public class CreateConnectionCommandValidator : AbstractValidator<CreateConnectionCommandRequest>
{
    public CreateConnectionCommandValidator()
    {
        RuleFor(x => x.SenderId)
            .NotEmpty().WithMessage("Sender ID is required.");

        RuleFor(x => x.ReceiverId)
            .NotEmpty().WithMessage("Receiver ID is required.");

        RuleFor(x => x.IntentId)
            .NotEmpty().WithMessage("Intent ID is required.");
    }
}
