using System.ComponentModel.DataAnnotations;

namespace DriveTrack.Api.Data.Entities;

public class AppUser
{
    [Key] public Guid Id { get; set; }

    [Required] public string Email { get; set; } = string.Empty;
    [Required] public string PasswordHash { get; set; } = string.Empty;
    [Required] public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserVehicle> Vehicles { get; set; } = new List<UserVehicle>();
}
