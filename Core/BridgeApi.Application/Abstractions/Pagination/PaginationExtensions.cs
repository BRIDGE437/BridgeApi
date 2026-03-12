using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Abstractions.Pagination;

public static class PaginationExtensions
{
    public static async Task<PaginatedResponse<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> source,
        int page,
        int size,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await source.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)size);

        var items = await source
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<T>(
            Items: items,
            Page: page,
            Size: size,
            TotalCount: totalCount,
            TotalPages: totalPages,
            HasPrevious: page > 1,
            HasNext: page < totalPages);
    }
}
