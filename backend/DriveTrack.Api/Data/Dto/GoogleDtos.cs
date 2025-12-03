using System;

namespace DriveTrack.Api.Data.Dto;

public record RegisterRequest(
    string Name,
    string Email,
    string Password
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthResponse(
    Guid UserId,
    string Email,
    string Name,
    string Token
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

