using BridgeApi.Application.Abstractions.Repositories.Intent;
using MediatR;
using IntentEntity = BridgeApi.Domain.Entities.Intent;

namespace BridgeApi.Application.Features.Commands.Intent.CreateIntent;

public class CreateIntentCommandHandler : IRequestHandler<CreateIntentCommandRequest, CreateIntentCommandResponse>
{
    private readonly IIntentWriteRepository _intentWriteRepository;

    public CreateIntentCommandHandler(IIntentWriteRepository intentWriteRepository)
    {
        _intentWriteRepository = intentWriteRepository;
    }

    public async Task<CreateIntentCommandResponse> Handle(CreateIntentCommandRequest request, CancellationToken cancellationToken)
    {
        var intent = new IntentEntity
        {
            Title = request.Title,
            Description = request.Description,
            IsActive = request.IsActive
        };

        await _intentWriteRepository.AddAsync(intent);
        await _intentWriteRepository.SaveAsync();

        return new CreateIntentCommandResponse(intent.Id, intent.Title, intent.Description, intent.IsActive, intent.CreatedAt);
    }
}
