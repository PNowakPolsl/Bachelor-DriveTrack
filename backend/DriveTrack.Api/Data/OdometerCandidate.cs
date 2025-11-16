public record OdometerCandidate(
    string Source,
    int? OdometerKm,
    DateOnly? Date
);
public record MonthlyExpenseReportItem(
    int Year,
    int Month,
    decimal Total
);

public record CategoryExpenseReportItem(
    string CategoryName,
    decimal Total
);

public record FuelConsumptionReportItem(
    int Year,
    int Month,
    double AvgConsumptionLPer100km
);
