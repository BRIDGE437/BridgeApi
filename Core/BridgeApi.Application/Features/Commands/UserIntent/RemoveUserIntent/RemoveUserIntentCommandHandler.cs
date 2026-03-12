using BridgeApi.Application.Abstractions.Repositories.UserIntent;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Application.Features.Commands.UserIntent.RemoveUserIntent;

public class RemoveUserIntentCommandHandler : IRequestHandler<RemoveUserIntentCommandRequest, RemoveUserIntentCommandResponse>
{
    private readonly IUserIntentReadRepository _userIntentReadRepository;
    private readonly IUserIntentWriteRepository _userIntentWriteRepository;
    private readonly ILogger<RemoveUserIntentCommandHandler> _logger;

    public RemoveUserIntentCommandHandler(
        IUserIntentReadRepository userIntentReadRepository,
        IUserIntentWriteRepository userIntentWriteRepository,
        ILogger<RemoveUserIntentCommandHandler> logger)
    {
        _userIntentReadRepository = userIntentReadRepository;
        _userIntentWriteRepository = userIntentWriteRepository;
        _logger = logger;
    }

    public async Task<RemoveUserIntentCommandResponse> Handle(RemoveUserIntentCommandRequest request, CancellationToken cancellationToken)
    {
        var userIntent = await _userIntentReadRepository
            .GetWhere(ui => ui.UserId == request.UserId && ui.IntentId == request.IntentId, tracking: false)
            .FirstOrDefaultAsync(cancellationToken);

        if (userIntent != null)
        {
            await _userIntentWriteRepository.RemoveAsync(userIntent);
            await _userIntentWriteRepository.SaveAsync();

            _logger.LogInformation(
                "User {UserId} removed Intent {IntentId}",
                request.UserId,
                request.IntentId);
        }

        return new RemoveUserIntentCommandResponse();
    }
}
