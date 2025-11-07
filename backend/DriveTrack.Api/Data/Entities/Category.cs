using System.ComponentModel.DataAnnotations;

namespace DriveTrack.Api.Data.Entities;

public class Category
{
    [Key] public Guid Id { get; set; }

    [Required] public string Name { get; set; } = string.Empty;

    public Guid? OwnerUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser? OwnerUser { get; set; }
}
