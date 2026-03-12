namespace BridgeApi.Application.Abstractions.Pagination;

public record PaginatedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int Size,
    int TotalCount,
    int TotalPages,
    bool HasPrevious,
    bool HasNext);
