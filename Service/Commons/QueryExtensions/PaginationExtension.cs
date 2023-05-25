using Microsoft.EntityFrameworkCore;

namespace Service.Commons.QueryExtensions;

public static class PaginationExtension
{
    public static async Task<PaginationModel<T>> Paging<T>(this IQueryable<T> query, int page, int size)
    {
        if (page < 1 || size < 1)
            return new PaginationModel<T>
            {
                Page = page,
                Size = size,
                Max = 0,
                Values = new List<T>()
            };

        var countMax = await query.CountAsync();
        var values = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return new PaginationModel<T>
        {
            Page = page,
            Size = size,
            Max = countMax,
            Values = values
        };
    }
}