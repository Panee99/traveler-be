namespace Service.Pagination;

public class PaginationModel<T>
{
    public int Page { get; set; }

    public int Size { get; set; }

    public int Max { get; set; }

    public List<T> Values { get; set; } = new();

    public PaginationModel<TResult> Map<TResult>(Func<T, TResult> selector)
    {
        return new PaginationModel<TResult>
        {
            Page = Page,
            Size = Size,
            Max = Max,
            Values = Values.Select(selector).ToList()
        };
    }

    public PaginationModel<TResult> UseMapper<TResult>(Func<List<T>, List<TResult>> mapper)
    {
        return new PaginationModel<TResult>
        {
            Page = Page,
            Size = Size,
            Max = Max,
            Values = mapper(Values)
        };
    }
}