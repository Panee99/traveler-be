namespace Service.Models.TourVariant;

public record TourVariantUpdateModel
{
    public int? AdultPrice;
    public int? ChildrenPrice;
    public int? InfantPrice;
    public DateTime? StartTime;
    public DateTime? EndTime;
}