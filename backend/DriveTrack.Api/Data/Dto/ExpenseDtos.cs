namespace DriveTrack.Api.Data.Dto;

public record CreateExpenseRequest(
    Guid CategoryId,
    DateOnly Date,
    decimal Amount,
    string? Description,
    int? OdometerKm
);
