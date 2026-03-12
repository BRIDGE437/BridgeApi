using MediatR;

namespace BridgeApi.Application.Features.Commands.UserProfile.DeleteUserProfile;

public record DeleteUserProfileCommandRequest(Guid Id) : IRequest<DeleteUserProfileCommandResponse?>;
