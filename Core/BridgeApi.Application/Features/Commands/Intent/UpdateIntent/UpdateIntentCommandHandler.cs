using BridgeApi.Application.Abstractions.Repositories.Intent;
using MediatR;

namespace BridgeApi.Application.Features.Commands.Intent.UpdateIntent;

public class UpdateIntentCommandHandler : IRequestHandler<UpdateIntentCommandRequest, UpdateIntentCommandResponse?>
{
    private readonly IIntentReadRepository _intentReadRepository;
    private readonly IIntentWriteRepository _intentWriteRepository;

    public UpdateIntentCommandHandler(
        IIntentReadRepository intentReadRepository,
        IIntentWriteRepository intentWriteRepository)
    {
        _intentReadRepository = intentReadRepository;
        _intentWriteRepository = intentWriteRepository;
    }

    public async Task<UpdateIntentCommandResponse?> Handle(UpdateIntentCommandRequest request, CancellationToken cancellationToken)
    {
        var intent = await _intentReadRepository.GetByIdAsync(request.Id, tracking: true);
        if (intent == null)
            return null;

        if (request.Title != null) intent.Title = request.Title;
        if (request.Description != null) intent.Description = request.Description;
        if (request.IsActive.HasValue) intent.IsActive = request.IsActive.Value;

        await _intentWriteRepository.UpdateAsync(intent);
        await _intentWriteRepository.SaveAsync();

        return new UpdateIntentCommandResponse(intent.Id, intent.Title, intent.Description, intent.IsActive, intent.CreatedAt);
    }
}
