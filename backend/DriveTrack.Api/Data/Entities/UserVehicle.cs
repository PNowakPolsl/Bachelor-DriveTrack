using System.ComponentModel.DataAnnotations.Schema;

namespace DriveTrack.Api.Data.Entities;

public class UserVehicle
{
    // Composite PK w OnModelCreating
    [ForeignKey(nameof(User))]   public Guid UserId { get; set; }
    public AppUser User { get; set; } = default!;

    [ForeignKey(nameof(Vehicle))] public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = default!;

    public VehicleRole Role { get; set; } = VehicleRole.Owner;
}
