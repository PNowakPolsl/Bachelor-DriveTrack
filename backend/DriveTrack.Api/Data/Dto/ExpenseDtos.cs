namespace DriveTrack.Api.Data.Dto;

public record CreateExpenseRequest(
    Guid CategoryId,
    DateOnly Date,
    decimal Amount,
    string? Description,
    int? OdometerKm
);

 public record ElectricConsumptionReportItem(
    int Year,
    int Month,
    double AvgConsumptionKWhPer100km
);

public record VehicleExpensesReportItem(
    Guid VehicleId,
    string VehicleName,
    decimal TotalAmount
);

public record VehicleCostPer100KmReportItem(
    Guid VehicleId,
    string VehicleName,
    double CostPer100Km
);
