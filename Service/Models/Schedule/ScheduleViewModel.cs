namespace Service.Models.Schedule;

public record ScheduleViewModel
{
    public Guid Id;
    public string Description = null!;
    public int Sequence;
}