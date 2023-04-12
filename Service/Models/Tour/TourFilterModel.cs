using Data.Enums;

namespace Service.Models.Tour;

public record TourFilterModel : PagingFilterModel
{
    public DateTime? EndBefore;
    public int? MaxPrice;
    public int? MinPrice;
    public DateTime? StartAfter;
    public string? Title;
    public TourType? Type;
}