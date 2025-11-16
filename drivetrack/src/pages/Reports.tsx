import Sidebar from "../components/Sidebar";
import Topbar from "../components/Topbar";
import MonthlyExpensesChart from "../components/MonthlyExpensesChart";
import ExpensesCategoryChart from "../components/ExpensesCategoryChart";
import FuelConsumption from "../components/FuelConsumptionChart";

import { useEffect, useState } from "react";
import {
  getMonthlyExpensesReport,
  getExpensesByCategoryReport,
  getFuelConsumptionReport,
} from "../api/reports";

function monthLabelPl(month: number): string {
  const names = [
    "Styczeń",
    "Luty",
    "Marzec",
    "Kwiecień",
    "Maj",
    "Czerwiec",
    "Lipiec",
    "Sierpień",
    "Wrzesień",
    "Październik",
    "Listopad",
    "Grudzień",
  ];
  return names[month - 1] ?? `Miesiąc ${month}`;
}

export default function Reports() {
  const [monthlyData, setMonthlyData] = useState<
    { month: string; amount: number }[]
  >([]);
  const [categoryData, setCategoryData] = useState<
    { name: string; value: number }[]
  >([]);
  const [fuelData, setFuelData] = useState<
    { month: string; consumption: number }[]
  >([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        const [m, c, f] = await Promise.all([
          getMonthlyExpensesReport(),
          getExpensesByCategoryReport(),
          getFuelConsumptionReport(),
        ]);

        setMonthlyData(
          m.map((item) => ({
            month: monthLabelPl(item.month),
            amount: Number(item.total),
          }))
        );

        setCategoryData(
          c.map((item) => ({
            name: item.categoryName,
            value: Number(item.total),
          }))
        );

        setFuelData(
          f.map((item) => ({
            month: monthLabelPl(item.month),
            consumption: Number(
              Math.round(item.avgConsumptionLPer100km * 10) / 10
            ),
          }))
        );
      } catch (e) {
        console.error("Błąd pobierania raportów", e);
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  return (
    <div className="min-h-screen bg-gray-50 flex">
      <Sidebar />
      <div className="flex flex-col flex-1">
        <Topbar />
        <main className="p-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold text-gray-800">Raporty</h1>
              <p className="text-gray-500 text-lg mt-2">
                Zapoznaj się z analizą swoich wydatków
              </p>
            </div>
          </div>

          {loading ? (
            <p className="mt-10 text-gray-500">Wczytywanie danych raportów…</p>
          ) : (
            <>
              <MonthlyExpensesChart data={monthlyData} />

              <div className="mt-10 grid grid-cols-1 lg:grid-cols-2 gap-8 w-full">
                <div className="w-full">
                  <ExpensesCategoryChart data={categoryData} />
                </div>
                <div className="w-full">
                  <FuelConsumption data={fuelData} />
                </div>
              </div>
            </>
          )}
        </main>
      </div>
    </div>
  );
}
