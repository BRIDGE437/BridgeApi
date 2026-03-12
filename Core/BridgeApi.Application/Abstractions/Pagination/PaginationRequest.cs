namespace BridgeApi.Application.Abstractions.Pagination;

public record PaginationRequest(int Page = 1, int Size = 10);
