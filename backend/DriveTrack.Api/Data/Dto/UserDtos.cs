namespace DriveTrack.Api.Data.Dto;

public record CreateUserRequest(string Email, string Password, string Name);
public record UserResponse(Guid Id, string Email, string Name, DateTime CreatedAt);
