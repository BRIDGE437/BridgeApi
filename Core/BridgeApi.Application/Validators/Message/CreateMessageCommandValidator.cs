using BridgeApi.Application.Features.Commands.Message.CreateMessage;
using FluentValidation;

namespace BridgeApi.Application.Validators.Message;

public class CreateMessageCommandValidator : AbstractValidator<CreateMessageCommandRequest>
{
    public CreateMessageCommandValidator()
    {
        RuleFor(x => x.ConnectionId)
            .NotEmpty().WithMessage("Connection ID is required.");

        RuleFor(x => x.SenderId)
            .NotEmpty().WithMessage("Sender ID is required.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(2000).WithMessage("Content must not exceed 2000 characters.");
    }
}
