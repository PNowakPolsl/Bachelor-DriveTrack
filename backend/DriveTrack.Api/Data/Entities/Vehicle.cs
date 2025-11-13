using System;
using System.ComponentModel.DataAnnotations;

namespace DriveTrack.Api.Data.Entities;

public class Vehicle
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Make { get; set; } = string.Empty;

    [Required]
    public string Model { get; set; } = string.Empty;

    public string? Plate { get; set; }
    public int? Year { get; set; }
    public string? Vin { get; set; }
    public int? InitialOdometerKm { get; set; }
    public int? BaseOdometerKm { get; set; }
    
    public Guid? CreatedBy { get; set; }
    public AppUser? CreatedByUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<UserVehicle> Users { get; set; } = new List<UserVehicle>();
}
