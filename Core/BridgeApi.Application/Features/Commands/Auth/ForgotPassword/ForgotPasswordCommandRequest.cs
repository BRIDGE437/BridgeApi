using MediatR;

namespace BridgeApi.Application.Features.Commands.Auth.ForgotPassword;

public record ForgotPasswordCommandRequest(string Email) : IRequest<ForgotPasswordCommandResponse>;
