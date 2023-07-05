namespace Data.Entities.Activities;

public interface IActivity
{
    public Guid? TourGroupId { get; set; }
    
    public string? Title { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Note { get; set; }
}