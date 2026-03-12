namespace BridgeApi.Application.Dtos;

public record LoginUserResult(string Id, string Username, string Email, TokenDto Token);
