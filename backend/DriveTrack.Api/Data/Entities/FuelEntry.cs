
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DriveTrack.Api.Data.Entities;

public class FuelEntry
{
    [Key] public Guid Id { get; set; }

    [Required] public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = default!;

    [Required] public Guid FuelTypeId { get; set; }
    public FuelType FuelType { get; set; } = default!;

    [Required] public DateOnly Date { get; set; }

    [Required] public decimal Volume { get; set; }
    [Required] public string Unit { get; set; } = "L";
    [Required] public decimal PricePerUnit { get; set; }
    [Required] public decimal TotalCost { get; set; }

    [Required] public int OdometerKm { get; set; }

    public bool IsFullTank { get; set; } = true;
    public string? Station { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // precision w DbContext
}
