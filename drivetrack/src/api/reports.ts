import { http } from "./http";

export type MonthlyExpensesReportItem = {
  year: number;
  month: number;
  total: number;
};

export type CategoryExpensesReportItem = {
  categoryName: string;
  total: number;
};

export type FuelConsumptionReportItem = {
  year: number;
  month: number;
  avgConsumptionLPer100km: number;
};

export async function getMonthlyExpensesReport() {
  const { data } = await http.get<MonthlyExpensesReportItem[]>(
    "/reports/monthly-expenses"
  );
  return data;
}

export async function getExpensesByCategoryReport() {
  const { data } = await http.get<CategoryExpensesReportItem[]>(
    "/reports/expenses-by-category"
  );
  return data;
}

export async function getFuelConsumptionReport() {
  const { data } = await http.get<FuelConsumptionReportItem[]>(
    "/reports/fuel-consumption"
  );
  return data;
}
