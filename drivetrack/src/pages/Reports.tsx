import Sidebar from "../components/Sidebar";
import Topbar from "../components/Topbar";
import MonthlyExpensesChart from "../components/MonthlyExpensesChart";
import ExpensesCategoryChart from "../components/ExpensesCategoryChart";
import FuelConsumption from "../components/FuelConsumptionChart";
import ElectricConsumptionChart from "../components/ElectricConsumptionChart";
import VehicleExpensesChart from "../components/VehicleExpensesChart";
import VehicleCostPerKmChart from "../components/VehicleCostPerKmChart";
import { http } from "../api/http";

import { useEffect, useMemo, useState } from "react";
import {
  getMonthlyExpensesReport,
  getExpensesByCategoryReport,
  getFuelConsumptionReport,
  getElectricConsumptionReport,
  getVehicleExpensesReport,
  getVehicleCostPer100KmReport,
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

type PeriodKey = "7d" | "31d" | "3m" | "6m" | "365d";

function periodLabelPl(period: PeriodKey): string {
  switch (period) {
    case "7d":
      return "ostatnich 7 dni";
    case "31d":
      return "ostatnich 31 dni";
    case "3m":
      return "ostatnich 3 miesięcy";
    case "6m":
      return "ostatnich 6 miesięcy";
    case "365d":
      return "ostatnich 365 dni";
  }
}

type VehicleOption = { id: string; name: string; fuelUnits: string[] };

export default function Reports() {
  const [period, setPeriod] = useState<PeriodKey>("6m");

  const [vehicles, setVehicles] = useState<VehicleOption[]>([]);

  const [vehicleForCategory, setVehicleForCategory] = useState<string>("all");
  const [vehicleForFuelConsumption, setVehicleForFuelConsumption] = useState<string>("all");
  const [vehicleForEvConsumption, setVehicleForEvConsumption] = useState<string>("all");
  const [vehicleForMonthly, setVehicleForMonthly] = useState<string>("all");

  const [monthlyData, setMonthlyData] = useState<{ month: string; amount: number }[]>([]);
  const [categoryData, setCategoryData] = useState<{ name: string; value: number }[]>([]);
  const [fuelData, setFuelData] = useState<{ month: string; consumption: number }[]>([]);
  const [evData, setEvData] = useState<{ month: string; consumption: number }[]>([]);
  const [vehicleExpensesData, setVehicleExpensesData] = useState<{ vehicle: string; amount: number }[]>([]);
  const [vehicleCostData, setVehicleCostData] = useState<{ vehicle: string; costPer100Km: number }[]>([]);

  const [loading, setLoading] = useState(true);

  function calcRange(p: PeriodKey): { from: string; to: string } {
    const end = new Date();
    const start = new Date(end);

    switch (p) {
      case "7d":
        start.setDate(start.getDate() - 6);
        break;
      case "31d":
        start.setDate(start.getDate() - 30);
        break;
      case "3m":
        start.setMonth(start.getMonth() - 3);
        break;
      case "6m":
        start.setMonth(start.getMonth() - 6);
        break;
      case "365d":
        start.setDate(start.getDate() - 364);
        break;
    }

    return {
      from: start.toISOString().slice(0, 10),
      to: end.toISOString().slice(0, 10),
    };
  }

  const fixedMonthlyRange = useMemo(() => {
    const today = new Date();
    const sixMonthsAgo = new Date();
    sixMonthsAgo.setMonth(today.getMonth() - 6);

    return {
      fixedFrom: sixMonthsAgo.toISOString().slice(0, 10),
      fixedTo: today.toISOString().slice(0, 10),
    };
  }, []);

  // --- LISTY AUT DO SELECTÓW (z fallbackiem gdy fuelUnits nie ma) ---
  const fuelVehiclesStrict = useMemo(() => {
    return vehicles.filter((v) => v.fuelUnits.includes("l"));
  }, [vehicles]);

  const evVehiclesStrict = useMemo(() => {
    return vehicles.filter((v) => v.fuelUnits.includes("kwh"));
  }, [vehicles]);

  // Jeśli backend nie zwróci fuelUnits -> strict listy będą puste.
  // Wtedy pokazujemy wszystkie auta, żeby UI działał.
  const fuelVehicles = fuelVehiclesStrict.length > 0 ? fuelVehiclesStrict : vehicles;
  const evVehicles = evVehiclesStrict.length > 0 ? evVehiclesStrict : vehicles;

  // Autoreset jeśli aktualny wybór nie istnieje w danej liście
  useEffect(() => {
    if (vehicleForFuelConsumption !== "all") {
      const ok = fuelVehicles.some((v) => v.id === vehicleForFuelConsumption);
      if (!ok) setVehicleForFuelConsumption("all");
    }
  }, [fuelVehicles, vehicleForFuelConsumption]);

  useEffect(() => {
    if (vehicleForEvConsumption !== "all") {
      const ok = evVehicles.some((v) => v.id === vehicleForEvConsumption);
      if (!ok) setVehicleForEvConsumption("all");
    }
  }, [evVehicles, vehicleForEvConsumption]);

  // pobierz listę aut
  useEffect(() => {
    async function fetchVehicles(): Promise<void> {
      try {
        const { data } = await http.get<any[]>("/me/vehicles");
        console.log("GET /me/vehicles =>", data);

        const mapped: VehicleOption[] = (data ?? []).map((v: any) => ({
          id: String(v.id ?? v.Id),
          name: String(v.name ?? v.Name),
          fuelUnits: (v.fuelUnits ?? v.FuelUnits ?? []).map((x: any) => String(x).toLowerCase()),
        }));

        console.log("Mapped vehicles =>", mapped);
        setVehicles(mapped);
      } catch (e) {
        console.error("Błąd pobierania listy pojazdów", e);
        setVehicles([]);
      }
    }

    fetchVehicles();
  }, []);

  // pobierz raporty
  useEffect(() => {
    async function fetchReports(): Promise<void> {
      setLoading(true);
      try {
        const { from, to } = calcRange(period);

        const vehicleIdMonthly = vehicleForMonthly === "all" ? undefined : vehicleForMonthly;
        const vehicleIdCategory = vehicleForCategory === "all" ? undefined : vehicleForCategory;
        const vehicleIdFuel = vehicleForFuelConsumption === "all" ? undefined : vehicleForFuelConsumption;
        const vehicleIdEv = vehicleForEvConsumption === "all" ? undefined : vehicleForEvConsumption;

        const [m, c, f, ev, ve, vc] = await Promise.all([
          getMonthlyExpensesReport(fixedMonthlyRange.fixedFrom, fixedMonthlyRange.fixedTo, vehicleIdMonthly),
          getExpensesByCategoryReport(from, to, vehicleIdCategory),
          getFuelConsumptionReport(from, to, vehicleIdFuel),
          getElectricConsumptionReport(from, to, vehicleIdEv),
          getVehicleExpensesReport(from, to),
          getVehicleCostPer100KmReport(from, to),
        ]);

        setMonthlyData(
          m.map((item) => ({
            month: monthLabelPl(item.month),
            amount: Number(item.totalAmount),
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
            consumption: Number(Math.round(item.averageConsumption * 10) / 10),
          }))
        );

        setEvData(
          ev.map((item) => ({
            month: monthLabelPl(item.month),
            consumption: Number(Math.round(item.averageConsumption * 10) / 10),
          }))
        );

        setVehicleExpensesData(
          ve.map((item) => ({
            vehicle: item.vehicleName,
            amount: Number(item.totalAmount),
          }))
        );

        setVehicleCostData(
          vc.map((item) => ({
            vehicle: item.vehicleName,
            costPer100Km: Number(Math.round(item.costPer100Km * 100) / 100),
          }))
        );
      } catch (e) {
        console.error("Błąd pobierania raportów", e);
      } finally {
        setLoading(false);
      }
    }

    fetchReports();
  }, [
    period,
    vehicleForMonthly,
    vehicleForCategory,
    vehicleForFuelConsumption,
    vehicleForEvConsumption,
    fixedMonthlyRange,
  ]);

  const rangeLabel = periodLabelPl(period);

  const monthlyKey = `monthly-${vehicleForMonthly}`;
  const categoryKey = `cat-${period}-${vehicleForCategory}`;
  const fuelKey = `fuel-${period}-${vehicleForFuelConsumption}`;
  const evKey = `ev-${period}-${vehicleForEvConsumption}`;

  return (
    <div className="min-h-screen bg-gray-50 flex">
      <Sidebar />
      <div className="flex flex-col flex-1">
        <Topbar />

        <main className="p-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold text-gray-800">Raporty</h1>
              <p className="text-gray-500 text-lg mt-2">Zapoznaj się z analizą swoich wydatków</p>
            </div>
          </div>

          {loading && <p className="mt-6 text-gray-500">Wczytywanie danych raportów…</p>}

          {/* Filtr miesięcznych (6 miesięcy) */}
          <div className="flex items-center justify-end gap-2 mt-4">
            <span className="text-sm text-gray-600">Pojazd (6 miesięcy):</span>
            <select
              className="border rounded-lg p-2 text-sm bg-white"
              value={vehicleForMonthly}
              onChange={(e) => setVehicleForMonthly(e.target.value)}
            >
              <option value="all">Wszystkie auta</option>
              {vehicles.map((v) => (
                <option key={v.id} value={v.id}>
                  {v.name}
                </option>
              ))}
            </select>
          </div>

          <div className="mt-4">
            <MonthlyExpensesChart key={monthlyKey} data={monthlyData} rangeLabel={rangeLabel} />
          </div>

          {/* Zakres czasu */}
          <div className="flex items-center gap-2 justify-end mt-8">
            <span className="text-sm text-gray-600">Zakres czasu:</span>
            <select
              className="border rounded-lg p-2 text-sm bg-white"
              value={period}
              onChange={(e) => setPeriod(e.target.value as PeriodKey)}
            >
              <option value="7d">Ostatnie 7 dni</option>
              <option value="31d">Ostatnie 31 dni</option>
              <option value="3m">Ostatnie 3 miesiące</option>
              <option value="6m">Ostatnie 6 miesięcy</option>
              <option value="365d">Ostatnie 365 dni</option>
            </select>
          </div>

          {/* Kategoria + spalanie */}
          <div className="mt-8 grid grid-cols-1 lg:grid-cols-2 gap-8 w-full items-start">
            <div className="w-full">
              <div className="flex items-center justify-end gap-2 mb-2">
                <span className="text-sm text-gray-600">Pojazd:</span>
                <select
                  className="border rounded-lg p-2 text-sm bg-white"
                  value={vehicleForCategory}
                  onChange={(e) => setVehicleForCategory(e.target.value)}
                >
                  <option value="all">Wszystkie auta</option>
                  {vehicles.map((v) => (
                    <option key={v.id} value={v.id}>
                      {v.name}
                    </option>
                  ))}
                </select>
              </div>

              <ExpensesCategoryChart key={categoryKey} data={categoryData} rangeLabel={rangeLabel} />
            </div>

            <div className="w-full">
              <div className="flex items-center justify-end gap-2 mb-2">
                <span className="text-sm text-gray-600">Pojazd (L/100km):</span>
                <select
                  className="border rounded-lg p-2 text-sm bg-white"
                  value={vehicleForFuelConsumption}
                  onChange={(e) => setVehicleForFuelConsumption(e.target.value)}
                >
                  <option value="all">Wszystkie auta</option>
                  {fuelVehicles.map((v) => (
                    <option key={v.id} value={v.id}>
                      {v.name}
                    </option>
                  ))}
                </select>
              </div>

              <FuelConsumption key={fuelKey} data={fuelData} rangeLabel={rangeLabel} />
            </div>
          </div>

          {/* Koszty wg pojazdu */}
          <div className="mt-8 grid grid-cols-1 lg:grid-cols-2 gap-8 w-full items-start">
            <div className="w-full">
              <VehicleCostPerKmChart data={vehicleCostData} rangeLabel={rangeLabel} />
            </div>
            <div className="w-full">
              <VehicleExpensesChart data={vehicleExpensesData} rangeLabel={rangeLabel} />
            </div>
          </div>

          {/* EV */}
          {evData.length > 0 && (
            <div className="mt-8">
              <div className="flex items-center justify-end gap-2 mb-2">
                <span className="text-sm text-gray-600">Pojazd (EV):</span>
                <select
                  className="border rounded-lg p-2 text-sm bg-white"
                  value={vehicleForEvConsumption}
                  onChange={(e) => setVehicleForEvConsumption(e.target.value)}
                >
                  <option value="all">Wszystkie auta</option>
                  {evVehicles.map((v) => (
                    <option key={v.id} value={v.id}>
                      {v.name}
                    </option>
                  ))}
                </select>
              </div>

              <ElectricConsumptionChart key={evKey} data={evData} rangeLabel={rangeLabel} />
            </div>
          )}
        </main>
      </div>
    </div>
  );
}
