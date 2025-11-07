namespace DriveTrack.Api.Data.Dto;

public record CreateReminderRequest(
    string Title,
    string? Description,
    DateOnly DueDate
);

public record UpdateReminderRequest(
    string? Title,
    string? Description,
    DateOnly? DueDate,
    bool? IsCompleted
);
