using MediatR;

namespace BridgeApi.Application.Features.Commands.Auth.ChangePassword;

public record ChangePasswordCommandRequest(string UserId, string CurrentPassword, string NewPassword) : IRequest<ChangePasswordCommandResponse>;
