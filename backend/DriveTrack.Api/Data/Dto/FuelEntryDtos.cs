namespace DriveTrack.Api.Data.Dto;

public record CreateFuelEntryRequest(
    Guid FuelTypeId,
    string? Unit,
    DateOnly Date,
    decimal Volume,
    decimal PricePerUnit,
    int OdometerKm,
    bool IsFullTank = true,
    string? Station = null
);
