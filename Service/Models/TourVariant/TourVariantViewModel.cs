using Data.Enums;
using Service.Models.Tour;

namespace Service.Models.TourVariant;

public record TourVariantViewModel
{
    public Guid Id;
    public string Code = null!;
    public int AdultPrice;
    public int ChildrenPrice;
    public int InfantPrice;
    public DateTime StartTime;
    public DateTime EndTime;
    public TourVariantStatus Status;
    public Guid TourId;
    public TourViewModel? Tour;
}