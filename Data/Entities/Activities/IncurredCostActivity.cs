using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Data.Entities.Activities;

public class IncurredCostActivity : IActivity
{
    public Guid? Id { get; set; }

    public Guid? TourGroupId { get; set; }

    public Guid? ImageId { get; set; }

    public double Cost { get; set; }

    public string Currency { get; set; } = null!;

    public string? Title { get; set; }

    public int? DayNo { get; set; }

    public string? Note { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    [JsonIgnore] public TourGroup? TourGroup { get; set; }

    [JsonIgnore] public Attachment? Image { get; set; }

    [NotMapped]
    public String? ImageUrl { get; set; }
}