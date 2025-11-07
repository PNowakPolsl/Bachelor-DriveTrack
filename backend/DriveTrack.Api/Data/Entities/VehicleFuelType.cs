using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DriveTrack.Api.Data.Entities;

public class VehicleFuelType
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Vehicle))]
    public Guid VehicleId { get; set; }

    public Vehicle Vehicle { get; set; } = default!;

    [Required]
    [ForeignKey(nameof(FuelType))]
    public Guid FuelTypeId { get; set; }

    public FuelType FuelType { get; set; } = default!;
}
