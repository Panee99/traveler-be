using Data.Enums;

namespace Service.Models.Tour;

public record TourFilterModel : PagingFilterModel
{
    public string? Title;
    public TourType? Type;
}