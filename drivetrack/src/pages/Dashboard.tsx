import { useEffect, useMemo, useState } from "react";
import Sidebar from "../components/Sidebar";
import Topbar from "../components/Topbar";
import { Bell, Fuel, Wallet } from "lucide-react";
import { http } from "../api/http";

type MonthlyExpenseReportItemApi = {
  Year?: number;
  Month?: number;
  TotalAmount?: number;
  year?: number;
  month?: number;
  totalAmount?: number;
};

type FuelConsumptionReportItemApi = {
  Year?: number;
  Month?: number;
  AverageConsumption?: number;
  year?: number;
  month?: number;
  averageConsumption?: number;
};

type MonthlyExpenseItem = {
  year: number;
  month: number;
  totalAmount: number;
};

type ConsumptionItem = {
  year: number;
  month: number;
  averageConsumption: number;
};

type UpcomingReminderApi = {
  Id?: string;
  Title?: string;
  DueDate?: string;
  VehicleName?: string;
  DaysLeft?: number;
  id?: string;
  title?: string;
  dueDate?: string;
  vehicleName?: string;
  daysLeft?: number;
};

type UpcomingReminder = {
  id: string;
  title: string;
  dueDate: string;
  vehicleName: string;
  daysLeft: number;
};

type MeVehicleApi = {
  id?: string;
  Id?: string;
  name?: string;
  Name?: string;
  make?: string;
  Make?: string;
  model?: string;
  Model?: string;
  fuelUnits?: string[];
  FuelUnits?: string[];
};

type VehicleFuelTypeApi = { id?: string; name?: string; defaultUnit?: string; Id?: string; Name?: string; DefaultUnit?: string };

type VehicleDetailsApi = {
  id?: string;
  Id?: string;
  name?: string;
  Name?: string;
  make?: string;
  Make?: string;
  model?: string;
  Model?: string;
  fuelTypes?: VehicleFuelTypeApi[];
  FuelTypes?: VehicleFuelTypeApi[];
};

type VehicleDashboardCard = {
  vehicleId: string;
  displayName: string;
  makeModel: string;
  fuelLabel: string;
  consumptionValue: number | null;
  consumptionUnit: "L/100km" | "kWh/100km" | null;
  expensesThisMonth: number;
};

function formatCurrency(value: number | null | undefined) {
  if (value == null) return "—";
  return new Intl.NumberFormat("pl-PL", {
    style: "currency",
    currency: "PLN",
    maximumFractionDigits: 0,
  }).format(value);
}

function formatDate(dateStr: string) {
  const d = new Date(dateStr);
  if (Number.isNaN(d.getTime())) return dateStr;
  return d.toLocaleDateString("pl-PL", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  });
}

function pad2(n: number) {
  return String(n).padStart(2, "0");
}

function toIsoDateOnly(d: Date) {
  return `${d.getFullYear()}-${pad2(d.getMonth() + 1)}-${pad2(d.getDate())}`;
}

function monthStartIso(d: Date) {
  return `${d.getFullYear()}-${pad2(d.getMonth() + 1)}-01`;
}

function mapMonthly(items: MonthlyExpenseReportItemApi[]): MonthlyExpenseItem[] {
  return (items ?? []).map((x) => ({
    year: x.year ?? x.Year ?? 0,
    month: x.month ?? x.Month ?? 0,
    totalAmount: Number(x.totalAmount ?? x.TotalAmount ?? 0),
  }));
}

function mapConsumption(items: FuelConsumptionReportItemApi[]): ConsumptionItem[] {
  return (items ?? []).map((x) => ({
    year: x.year ?? x.Year ?? 0,
    month: x.month ?? x.Month ?? 0,
    averageConsumption: Number(x.averageConsumption ?? x.AverageConsumption ?? 0),
  }));
}

function pickLatestConsumption(list: ConsumptionItem[]): number | null {
  if (!list || list.length === 0) return null;
  const latest = list.reduce((acc, cur) => {
    if (cur.year > acc.year || (cur.year === acc.year && cur.month > acc.month)) return cur;
    return acc;
  }, list[0]);
  return latest?.averageConsumption ?? null;
}

function normalizeFuelTypes(raw?: VehicleFuelTypeApi[]) {
  return (raw ?? []).map((ft) => ({
    name: String(ft.name ?? ft.Name ?? "").trim(),
    unit: String(ft.defaultUnit ?? ft.DefaultUnit ?? "").trim().toLowerCase(),
  })).filter(x => x.name.length > 0);
}

function buildFuelLabel(fuelTypes: { name: string; unit: string }[], fallbackUnits: string[]): { label: string; isEv: boolean } {
  const isEv = fuelTypes.some(ft => ft.unit === "kwh") || fallbackUnits.includes("kwh");

  if (fuelTypes.length > 0) {
    const names = Array.from(new Set(fuelTypes.map(ft => ft.name)));
    const label = names.join("/");
    return { label: label || (isEv ? "EV" : "Spalinowe"), isEv };
  }

  if (isEv) return { label: "EV", isEv: true };
  return { label: "Benzyna/LPG/Diesel", isEv: false };
}

export default function Dashboard() {
  const [cards, setCards] = useState<VehicleDashboardCard[]>([]);
  const [reminders, setReminders] = useState<UpcomingReminder[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const now = useMemo(() => new Date(), []);
  const fromMonth = useMemo(() => monthStartIso(now), [now]);
  const toToday = useMemo(() => toIsoDateOnly(now), [now]);

  useEffect(() => {
    async function load() {
      try {
        setLoading(true);
        setError(null);

        const remindersReq = http.get<UpcomingReminderApi[]>("/dashboard/upcoming-reminders");

        const vehiclesReq = http.get<MeVehicleApi[]>("/me/vehicles");

        const [remindersRes, vehiclesRes] = await Promise.all([remindersReq, vehiclesReq]);

        // --- przypomnienia ---
        const remRaw = remindersRes.data ?? [];
        const mappedRem: UpcomingReminder[] = remRaw.map((r) => ({
          id: r.id ?? r.Id ?? crypto.randomUUID(),
          title: r.title ?? r.Title ?? "(bez tytułu)",
          dueDate: r.dueDate ?? r.DueDate ?? "",
          vehicleName: r.vehicleName ?? r.VehicleName ?? "(bez nazwy pojazdu)",
          daysLeft: r.daysLeft ?? r.DaysLeft ?? 0,
        }));
        setReminders(mappedRem);

        // --- pojazdy ---
        const meVehicles = (vehiclesRes.data ?? []).map((v) => ({
          id: String(v.id ?? v.Id ?? ""),
          name: String(v.name ?? v.Name ?? "(bez nazwy)"),
          make: String(v.make ?? v.Make ?? ""),
          model: String(v.model ?? v.Model ?? ""),
          fuelUnits: ((v.fuelUnits ?? v.FuelUnits ?? []) as any[]).map((x) => String(x).toLowerCase()),
        })).filter(v => v.id);

        if (meVehicles.length === 0) {
          setCards([]);
          return;
        }

        const perVehicle = await Promise.all(
          meVehicles.map(async (v) => {
            const [detailsRes, monthlyRes, fuelRes, evRes] = await Promise.all([
              http.get<VehicleDetailsApi>(`/vehicles/${v.id}`),
              http.get<MonthlyExpenseReportItemApi[]>("/reports/monthly-expenses", {
                params: { from: fromMonth, to: toToday, vehicleId: v.id },
              }),
              http.get<FuelConsumptionReportItemApi[]>("/reports/fuel-consumption", {
                params: { vehicleId: v.id },
              }),
              http.get<FuelConsumptionReportItemApi[]>("/reports/ev-consumption", {
                params: { vehicleId: v.id },
              }),
            ]);

            const d = detailsRes.data ?? {};
            const fuelTypes = normalizeFuelTypes((d.fuelTypes ?? d.FuelTypes) as any);

            const { label: fuelLabel, isEv } = buildFuelLabel(fuelTypes, v.fuelUnits);

            const monthly = mapMonthly(monthlyRes.data ?? []);
            const expensesThisMonth = monthly.reduce((sum, x) => sum + (x.totalAmount || 0), 0);

            const fuel = mapConsumption(fuelRes.data ?? []);
            const ev = mapConsumption(evRes.data ?? []);

            const consumptionValue = isEv ? pickLatestConsumption(ev) : pickLatestConsumption(fuel);
            const consumptionUnit: VehicleDashboardCard["consumptionUnit"] = isEv ? "kWh/100km" : "L/100km";

            const makeModel = `${v.make} ${v.model}`.trim() || "(brak marki/modelu)";

            const card: VehicleDashboardCard = {
              vehicleId: v.id,
              displayName: v.name,
              makeModel,
              fuelLabel,
              consumptionValue,
              consumptionUnit,
              expensesThisMonth,
            };

            return card;
          })
        );

        perVehicle.sort((a, b) => b.expensesThisMonth - a.expensesThisMonth);

        setCards(perVehicle);
      } catch (err) {
        console.error(err);
        setError("Nie udało się załadować danych dashboardu.");
      } finally {
        setLoading(false);
      }
    }

    load();
  }, [fromMonth, toToday]);

  return (
    <div className="min-h-screen bg-gray-50 flex">
      <Sidebar />
      <div className="flex flex-col flex-1">
        <Topbar />

        <main className="p-6">
          <h1 className="text-3xl font-bold text-gray-800">Panel sterowania</h1>
          <p className="text-gray-500 text-lg mt-2">Podsumowanie twoich wydatków i pojazdów</p>

          {error && (
            <div className="mt-4 rounded-xl bg-red-50 text-red-700 px-4 py-3 text-sm">
              {error}
            </div>
          )}

          {/* --- WYDATKI W TYM MIESIĄCU (KAFELKI PER AUTO) --- */}
          <div className="mt-8">
            <div className="flex items-center gap-2 text-gray-800 font-semibold">
              <Wallet className="w-5 h-5 text-blue-500" />
              <span>Wydatki w tym miesiącu (per pojazd)</span>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mt-4">
              {loading ? (
                [1, 2, 3].map((i) => (
                  <div key={i} className="bg-white rounded-2xl shadow-md p-6">
                    <div className="h-4 w-40 bg-gray-200 rounded animate-pulse" />
                    <div className="h-3 w-52 bg-gray-100 rounded mt-2 animate-pulse" />
                    <div className="h-8 w-32 bg-gray-200 rounded mt-4 animate-pulse" />
                  </div>
                ))
              ) : cards.length === 0 ? (
                <div className="text-sm text-gray-500">Brak pojazdów lub brak danych.</div>
              ) : (
                cards.map((c) => (
                  <div key={c.vehicleId} className="bg-white rounded-2xl shadow-md p-6">
                    <div className="flex items-start justify-between">
                      <div>
                        <h3 className="text-gray-800 font-semibold text-lg">{c.displayName}</h3>
                        <p className="text-gray-500 text-sm mt-1">
                          {c.makeModel} • {c.fuelLabel}
                        </p>
                      </div>
                      <div className="text-blue-500">
                        <Wallet className="w-6 h-6" />
                      </div>
                    </div>

                    <div className="mt-4 text-3xl font-bold text-gray-800">
                      {formatCurrency(c.expensesThisMonth)}
                    </div>
                    <p className="text-gray-500 text-sm mt-1">Od {fromMonth} do {toToday}</p>
                  </div>
                ))
              )}
            </div>
          </div>

          {/* --- SPALANIE / ZUŻYCIE ENERGII (KAFELKI PER AUTO) --- */}
          <div className="mt-10">
            <div className="flex items-center gap-2 text-gray-800 font-semibold">
              <Fuel className="w-5 h-5 text-blue-500" />
              <span>Średnie spalanie / zużycie (ostatni dostępny miesiąc)</span>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mt-4">
              {loading ? (
                [1, 2, 3].map((i) => (
                  <div key={i} className="bg-white rounded-2xl shadow-md p-6">
                    <div className="h-4 w-40 bg-gray-200 rounded animate-pulse" />
                    <div className="h-3 w-52 bg-gray-100 rounded mt-2 animate-pulse" />
                    <div className="h-8 w-24 bg-gray-200 rounded mt-4 animate-pulse" />
                  </div>
                ))
              ) : cards.length === 0 ? (
                <div className="text-sm text-gray-500">Brak pojazdów lub brak danych.</div>
              ) : (
                cards.map((c) => (
                  <div key={c.vehicleId} className="bg-white rounded-2xl shadow-md p-6">
                    <div className="flex items-start justify-between">
                      <div>
                        <h3 className="text-gray-800 font-semibold text-lg">{c.displayName}</h3>
                        <p className="text-gray-500 text-sm mt-1">
                          {c.makeModel} • {c.fuelLabel}
                        </p>
                      </div>
                      <div className="text-blue-500">
                        <Fuel className="w-6 h-6" />
                      </div>
                    </div>

                    <div className="mt-4 text-3xl font-bold text-gray-800">
                      {c.consumptionValue == null ? (
                        <span className="text-gray-500 text-base font-medium">Brak danych</span>
                      ) : (
                        <>
                          {c.consumptionValue.toFixed(1)}
                          <span className="text-base text-gray-500 ml-2">
                            {c.consumptionUnit}
                          </span>
                        </>
                      )}
                    </div>

                    <p className="text-gray-500 text-sm mt-1">Na podstawie tankowań/ładowań.</p>
                  </div>
                ))
              )}
            </div>
          </div>

          {/* Nadchodzące przypomnienia (jak było) */}
          <section className="mt-10 grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div className="lg:col-span-2 bg-white rounded-2xl shadow-md p-6">
              <div className="flex items-center justify-between mb-4">
                <h2 className="text-xl font-semibold text-gray-800 flex items-center gap-2">
                  <Bell className="w-5 h-5 text-blue-500" />
                  Nadchodzące przypomnienia
                </h2>
              </div>

              {loading ? (
                <div className="space-y-3">
                  {[1, 2, 3].map((i) => (
                    <div
                      key={i}
                      className="flex items-center justify-between border-b last:border-0 pb-3"
                    >
                      <div className="space-y-2">
                        <div className="h-4 w-48 bg-gray-200 rounded animate-pulse" />
                        <div className="h-3 w-32 bg-gray-100 rounded animate-pulse" />
                      </div>
                      <div className="h-4 w-16 bg-gray-100 rounded animate-pulse" />
                    </div>
                  ))}
                </div>
              ) : reminders.length === 0 ? (
                <p className="text-gray-500 text-sm">
                  Brak nadchodzących przypomnień. Dodaj przypomnienie przy konkretnym pojeździe.
                </p>
              ) : (
                <ul className="divide-y divide-gray-100">
                  {reminders.map((r) => (
                    <li key={r.id} className="py-3 flex items-center justify-between">
                      <div>
                        <p className="font-medium text-gray-800">{r.title}</p>
                        <p className="text-sm text-gray-500">
                          Pojazd: <span className="font-medium">{r.vehicleName}</span> • termin: {formatDate(r.dueDate)}
                        </p>
                      </div>
                      <span
                        className={`text-sm px-3 py-1 rounded-full ${
                          r.daysLeft < 0
                            ? "bg-red-50 text-red-700"
                            : r.daysLeft === 0
                            ? "bg-orange-50 text-orange-700"
                            : "bg-blue-50 text-blue-700"
                        }`}
                      >
                        {r.daysLeft < 0
                          ? `Spóźnione o ${Math.abs(r.daysLeft)} dni`
                          : r.daysLeft === 0
                          ? "Dzisiaj"
                          : `Za ${r.daysLeft} dni`}
                      </span>
                    </li>
                  ))}
                </ul>
              )}
            </div>

            <div className="bg-white rounded-2xl shadow-md p-6">
              <h2 className="text-xl font-semibold text-gray-800 mb-4">Podpowiedzi</h2>
              <ul className="space-y-2 text-sm text-gray-600">
                <li>• Dodaj wydatki przy pojazdach, aby zobaczyć pełne statystyki.</li>
                <li>• Tankowania automatycznie tworzą wydatek „Paliwo”.</li>
                <li>• Udostępnij pojazd innemu użytkownikowi w zakładce „Użytkownicy”.</li>
              </ul>
            </div>
          </section>
        </main>
      </div>
    </div>
  );
}
