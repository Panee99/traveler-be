using Service.Commons.QueryExtensions;

namespace Service.Models;

public record OrderByModel
{
    public string Property = null!;
    public Order Order;
}