using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Data.Entities.Activities;

public class AttendanceItem
{
    public Guid Id { get; set; }
    
    public Guid AttendanceActivityId { get; set; }

    public Guid UserId { get; set; }

    public bool Present { get; set; } = false;

    public string Reason { get; set; } = string.Empty;

    public DateTime? CreatedAt { get; set; }
}