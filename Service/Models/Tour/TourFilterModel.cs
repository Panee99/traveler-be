using Data.Enums;

namespace Service.Models.Tour;

public record TourFilterModel : PagingFilterModel
{
    public string? Title;
    public int? MinPrice;
    public int? MaxPrice;
    public DateTime? StartAfter;
    public DateTime? EndBefore;
    public TourType? Type;
}