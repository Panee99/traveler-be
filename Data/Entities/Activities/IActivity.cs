using Newtonsoft.Json;

namespace Data.Entities.Activities;

public interface IActivity
{
    public Guid? TourGroupId { get; set; }
    
    public string? Title { get; set; }

    public int? DayNo { get; set; }    
    
    public DateTime? CreatedAt { get; set; }

    public string? Note { get; set; }

    [JsonIgnore]
    public bool? IsDeleted { get; set; }
}