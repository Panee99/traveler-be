using Microsoft.EntityFrameworkCore;

namespace Service.Pagination;

public static class PaginationExtensions
{
    public static async Task<PaginationModel<T>> Paging<T>(this IQueryable<T> query, int page, int size)
    {
        if (page < 1 || size < 1)
        {
            return new PaginationModel<T>()
            {
                Page = page,
                Size = size,
                Max = 0,
                Values = new List<T>(),
            };
        }

        var countMax = await query.CountAsync();
        var values = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return new PaginationModel<T>()
        {
            Page = page,
            Size = size,
            Max = countMax,
            Values = values
        };
    }

    // public static async Task<PaginationModel<T>> Paging<T>(this IQueryable<T> query,
    //     int page,
    //     int size,
    //     string orderBy,
    //     Order order)
    // {
    //     if (page < 1 || size < 1)
    //     {
    //         return new PaginationModel<T>()
    //         {
    //             Page = page,
    //             Size = size,
    //             Max = 0,
    //             Values = new List<T>(),
    //         };
    //     }
    //
    //     var countMax = await query.CountAsync();
    //
    //     query = order is Order.ASC ? query.OrderByAscending(orderBy) : query.OrderByDescending(orderBy);
    //
    //     var values = await query.Skip((page - 1) * size).Take(size).ToListAsync();
    //
    //     return new PaginationModel<T>()
    //     {
    //         Page = page,
    //         Size = size,
    //         Max = countMax,
    //         Values = values
    //     };
    // }
    //
    // private static IOrderedQueryable<T> OrderingHelper<T>(IQueryable<T> source,
    //     string propertyName,
    //     bool descending,
    //     bool anotherLevel)
    // {
    //     var param = Expression.Parameter(typeof(T), string.Empty);
    //     var property = Expression.PropertyOrField(param, propertyName);
    //     var sort = Expression.Lambda(property, param);
    //
    //     var call = Expression.Call(
    //         typeof(Queryable),
    //         (!anotherLevel ? "OrderBy" : "ThenBy") + (descending ? "Descending" : string.Empty),
    //         new[] { typeof(T), property.Type },
    //         source.Expression,
    //         Expression.Quote(sort));
    //
    //     return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
    // }
    //
    // private static IOrderedQueryable<T> OrderByAscending<T>(this IQueryable<T> source, string propertyName)
    // {
    //     return OrderingHelper(source, propertyName, false, false);
    // }
    //
    // private static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName)
    // {
    //     return OrderingHelper(source, propertyName, true, false);
    // }
    //
    // public enum Order
    // {
    //     ASC,
    //     DESC
    // }
}