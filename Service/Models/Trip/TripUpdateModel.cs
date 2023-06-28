namespace Service.Models.Trip;

public record TripUpdateModel
{
    public int? AdultPrice;
    public int? ChildrenPrice;
    public int? InfantPrice;
    public DateTime? StartTime;
    public DateTime? EndTime;
}