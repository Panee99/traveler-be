using Data.Enums;
using Service.Models.Tour;

namespace Service.Models.TourVariant;

public record TourVariantViewModel
{
    public Guid Id;
    public Guid TourId;
    public string Code = null!;
    public int AdultPrice;
    public int ChildrenPrice;
    public int InfantPrice;
    public DateTime StartTime;
    public DateTime EndTime;
    public TourVariantStatus Status;
    public TourViewModel? Tour;
}