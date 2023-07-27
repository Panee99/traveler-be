using Newtonsoft.Json;

namespace Data.Entities.Activities;

public class AttendanceActivity : IActivity
{
    public Guid? Id { get; set; }
    
    public Guid? TourGroupId { get; set; }
    
    public string? Title { get; set; }
    
    public int? DayNo { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    
    public string? Note { get; set; }
    
    public bool? IsDeleted { get; set; }
    
    public bool? IsOpen { get; set; }
    
    [JsonIgnore]
    public ICollection<AttendanceItem>? Items { get; set; }

    public TourGroup? TourGroup { get; set; }
}