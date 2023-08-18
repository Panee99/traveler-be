namespace Data.Entities;

public class BaseEntity
{
    public Guid CreatedById { get; set; }

    public User CreatedBy { get; set; } = null!;

    public Guid? DeletedById { get; set; }

    public User? DeletedBy { get; set; }
}