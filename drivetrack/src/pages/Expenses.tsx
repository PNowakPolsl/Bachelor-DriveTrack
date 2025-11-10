// src/pages/Expenses.tsx
import Sidebar from "../components/Sidebar";
import Topbar from "../components/Topbar";
import { useEffect, useMemo, useState } from "react";
import ExpensesForm from "../components/ExpensesForm";

import { listVehicles } from "../api/vehicles";
import { listCategories } from "../api/categories";
import { listExpenses } from "../api/expenses";

import type { Vehicle, Category, Expense, Guid } from "../api/types";

export default function Expenses() {
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [activeVehicleId, setActiveVehicleId] = useState<Guid | null>(null);
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [isFormOpen, setIsFormOpen] = useState(false);

    useEffect(() => {
        (async () => {
        const [v, c] = await Promise.all([
            listVehicles(),
            listCategories(),
        ]);
        setVehicles(v);
        setCategories(c);
        if (v.length && !activeVehicleId) setActiveVehicleId(v[0].id);
        })();
    }, []);

    useEffect(() => {
        if (!activeVehicleId) return;
            (async () => {
            const data = await listExpenses(activeVehicleId);
            setExpenses(data);
        })();
    }, [activeVehicleId]);

    useEffect(() => {
        if (!isFormOpen) return;
            (async () => {
                const c = await listCategories();
                setCategories(c);
            })();
    }, [isFormOpen]);

    const activeVehicle = useMemo(
        () => vehicles.find(v => v.id === activeVehicleId) ?? null,
        [vehicles, activeVehicleId]
    );

    const categoryBadge = (name: string) => {
    switch (name.toLowerCase()) {
        case "paliwo":
        return "bg-blue-50 text-blue-600 hover:bg-blue-200 transition";
        case "mechanik":
        return "bg-purple-50 text-purple-600 hover:bg-purple-200 transition";
        case "przegląd":
        case "przeglad":
        return "bg-orange-50 text-orange-600 hover:bg-orange-200 transition";
        case "ubezpieczenie":
        return "bg-green-50 text-green-600 hover:bg-green-200 transition";
        case "inne":
        return "bg-yellow-50 text-yellow-600 hover:bg-yellow-200 transition";
        default:
        return "bg-gray-100 text-gray-800";
    }
    };


  return (
    <div className="min-h-screen bg-gray-50 flex">
      <Sidebar />
      <div className="flex flex-col flex-1">
        <Topbar />
        <main className="p-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold text-gray-800">Wydatki</h1>
              <p className="text-gray-500 text-lg mt-2">Zarządzaj swoimi wydatkami</p>
            </div>

            <div className="flex items-center gap-3">
              {/* wybór pojazdu */}
              <select
                className="border rounded-lg p-2"
                value={activeVehicleId ?? ""}
                onChange={(e) => setActiveVehicleId(e.target.value)}
              >
                {vehicles.map(v => (
                  <option key={v.id} value={v.id}>
                    {v.make} {v.model} — {v.name}
                  </option>
                ))}
              </select>

              <button
                onClick={() => setIsFormOpen(true)}
                className="bg-blue-600 text-white font-medium px-4 py-2 rounded-lg shadow-md hover:bg-blue-500 transition"
                disabled={!activeVehicleId}
              >
                + Dodaj wydatek
              </button>
            </div>
          </div>

          {/* Formularz */}
          {isFormOpen && activeVehicleId && (
            <ExpensesForm
              onClose={() => setIsFormOpen(false)}
              activeVehicleId={activeVehicleId}
              categories={categories}
              onCreated={(created) => {
                setExpenses(prev => [created, ...prev]);
                setIsFormOpen(false);
              }}
            />
          )}

          {/* Tabela wydatków */}
          <div className="w-full mx-auto mt-6 p-4 shadow-md rounded-2xl bg-white">
            <div className="flex items-center justify-between">
              <h2 className="text-xl font-bold text-gray-800">
                Wszystkie wydatki {activeVehicle ? `— ${activeVehicle.make} ${activeVehicle.model} (${activeVehicle.name})` : ""}
              </h2>
            </div>

            <div className="mt-5 overflow-x-auto rounded-xl border border-gray-300">
              <table className="w-full border-collapse">
                <thead className="bg-gray-200">
                  <tr>
                    <th className="text-left p-3">Data</th>
                    <th className="text-left p-3">Kategoria</th>
                    <th className="text-left p-3">Opis</th>
                    <th className="text-right p-3">Kwota</th>
                    <th className="text-right p-3">Przebieg</th>
                  </tr>
                </thead>
                <tbody>
                  {expenses.map((e) => (
                    <tr key={e.id} className="border-t border-gray-300 hover:bg-gray-50 transition">
                      <td className="p-3">{e.date}</td>
                      <td className="p-3">
                        <span className={`px-2 py-1 rounded-full font-semibold text-sm ${categoryBadge(e.category.name)}`}>
                            {e.category.name}
                        </span>
                      </td>


                      <td className="p-3">{e.description ?? "—"}</td>
                      <td className="p-3 text-right font-semibold">{e.amount.toFixed(2)} zł</td>
                      <td className="p-3 text-right">{e.odometerKm ?? "—"}</td>
                    </tr>
                  ))}
                  {expenses.length === 0 && (
                    <tr>
                      <td className="p-4 text-gray-500" colSpan={5}>
                        Brak wydatków dla tego pojazdu.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </main>
      </div>
    </div>
  );
}
