namespace Service.Models.TourFlow;

public record TourFlowViewModel
{
    public Guid Id;
    public string Description = null!;
    public int Sequence;
    public DateTime? From;
    public DateTime? To;
}