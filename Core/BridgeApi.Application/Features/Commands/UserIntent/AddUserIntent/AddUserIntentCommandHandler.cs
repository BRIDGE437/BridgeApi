using BridgeApi.Application.Abstractions.Repositories.UserIntent;
using BridgeApi.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using UserIntentEntity = BridgeApi.Domain.Entities.UserIntent;

namespace BridgeApi.Application.Features.Commands.UserIntent.AddUserIntent;

public class AddUserIntentCommandHandler : IRequestHandler<AddUserIntentCommandRequest, AddUserIntentCommandResponse>
{
    private readonly IUserIntentWriteRepository _userIntentWriteRepository;
    private readonly ILogger<AddUserIntentCommandHandler> _logger;

    public AddUserIntentCommandHandler(
        IUserIntentWriteRepository userIntentWriteRepository,
        ILogger<AddUserIntentCommandHandler> logger)
    {
        _userIntentWriteRepository = userIntentWriteRepository;
        _logger = logger;
    }

    public async Task<AddUserIntentCommandResponse> Handle(AddUserIntentCommandRequest request, CancellationToken cancellationToken)
    {
        var userIntent = new UserIntentEntity
        {
            UserId = request.UserId,
            IntentId = request.IntentId
        };

        await _userIntentWriteRepository.AddAsync(userIntent);
        await _userIntentWriteRepository.SaveAsync();

        _logger.LogInformation(
            "User {UserId} added Intent {IntentId}",
            request.UserId,
            request.IntentId);

        return new AddUserIntentCommandResponse();
    }
}
