namespace Data.Entities.Activities;

public class NextDestinationActivity : IActivity
{
    public Guid? Id { get; set; }

    public Guid? TourGroupId { get; set; }
    
    public TourGroup? TourGroup { get; set; }
    
    public string? Title { get; set; }

    public string? Note { get; set; }
    
    public DateTime? CreatedAt { get; set; }
}