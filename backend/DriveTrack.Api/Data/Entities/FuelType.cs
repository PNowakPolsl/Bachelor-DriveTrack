using System;
using System.ComponentModel.DataAnnotations;

namespace DriveTrack.Api.Data.Entities;

public class FuelType
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string DefaultUnit { get; set; } = "L"; // L, kWh, galUS, galUK

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
