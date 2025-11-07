using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DriveTrack.Api.Data.Entities;

public class Reminder
{
    [Key] public Guid Id { get; set; }

    [Required, ForeignKey(nameof(Vehicle))]
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = default!;

    [Required] public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    [Required] public DateOnly DueDate { get; set; }

    public bool IsCompleted { get; set; } = false;

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
