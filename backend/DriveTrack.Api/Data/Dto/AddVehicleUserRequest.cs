using DriveTrack.Api.Data.Entities;

namespace DriveTrack.Api.Data.Dto
{
    public class AddVehicleUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public VehicleRole Role { get; set; } = VehicleRole.Driver;
    }
}
