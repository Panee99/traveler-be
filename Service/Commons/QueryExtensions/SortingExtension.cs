using System.Linq.Expressions;

namespace Service.Commons.QueryExtensions;

public static class SortingExtension
{
    public static IQueryable<T> ApplyOrderBy<T>(this IQueryable<T> query, string propertyName, Order order)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, propertyName);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = order is Order.Desc ? "OrderByDescending" : "OrderBy";

        var orderByExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(T), property.Type },
            query.Expression,
            Expression.Quote(lambda)
        );

        return query.Provider.CreateQuery<T>(orderByExpression);
    }
}

public enum Order
{
    Asc,
    Desc
}