import { http } from "./http";

export type MonthlyExpensesReportItem = {
  year: number;
  month: number;
  totalAmount: number;
};

export type CategoryExpensesReportItem = {
  categoryName: string;
  total: number;
};

export type FuelConsumptionReportItem = {
  year: number;
  month: number;
  averageConsumption: number;
};

export type ElectricConsumptionReportItem = {
  year: number;
  month: number;
  averageConsumption: number;
};

export type VehicleExpensesReportItem = {
  vehicleId: string;
  vehicleName: string;
  totalAmount: number;
};

export type VehicleCostPer100KmReportItem = {
  vehicleId: string;
  vehicleName: string;
  costPer100Km: number;
};


export async function getMonthlyExpensesReport(from?: string, to?: string, vehicleId?: string) {
  const { data } = await http.get<MonthlyExpensesReportItem[]>(
    "/reports/monthly-expenses",
    { params: { from, to, vehicleId } }
  );
  return data;
}

export async function getExpensesByCategoryReport(from?: string, to?: string, vehicleId?: string) {
  const { data } = await http.get<CategoryExpensesReportItem[]>(
    "/reports/expenses-by-category",
    { params: { from, to, vehicleId } }
  );
  return data;
}

export async function getFuelConsumptionReport(from?: string, to?: string, vehicleId?: string) {
  const { data } = await http.get<FuelConsumptionReportItem[]>(
    "/reports/fuel-consumption",
    { params: { from, to, vehicleId } }
  );
  return data;
}

export async function getElectricConsumptionReport(from?: string, to?: string, vehicleId?: string) {
  const { data } = await http.get<ElectricConsumptionReportItem[]>(
    "/reports/ev-consumption",
    { params: { from, to, vehicleId } }
  );
  return data;
}

export async function getVehicleExpensesReport(from?: string, to?: string) {
  const { data } = await http.get<VehicleExpensesReportItem[]>(
    "/reports/vehicle-expenses",
    { params: { from, to } }
  );
  return data;
}

export async function getVehicleCostPer100KmReport(from?: string, to?: string) {
  const { data } = await http.get<VehicleCostPer100KmReportItem[]>(
    "/reports/vehicle-cost-per-100km",
    { params: { from, to } }
  );
  return data;
}


