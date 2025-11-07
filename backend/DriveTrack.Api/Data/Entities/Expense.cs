using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DriveTrack.Api.Data.Entities;

public class Expense
{
    [Key] public Guid Id { get; set; }

    [Required, ForeignKey(nameof(Vehicle))]
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = default!;

    [Required, ForeignKey(nameof(Category))]
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    [Required] public DateOnly Date { get; set; }
    [Required] public decimal Amount { get; set; }
    public string? Description { get; set; }
    public int? OdometerKm { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
