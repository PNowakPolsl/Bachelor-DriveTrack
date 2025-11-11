import { useState, useEffect, useMemo } from "react";
import { X } from "lucide-react";
import type {
  Category,
  CreateExpenseRequest,
  Expense,
  Guid,
  FuelType,
} from "../api/types";
import { createExpense } from "../api/expenses";
import { createFuelEntry } from "../api/fuel";
import { getVehicle } from "../api/vehicles";

export default function ExpensesForm({
  onClose,
  activeVehicleId,
  categories,
  onCreated,
}: {
  onClose: () => void;
  activeVehicleId: Guid;
  categories: Category[];
  onCreated: (exp: Expense) => void;
}) {
  const [form, setForm] = useState({
    date: "",
    categoryId: "",
    amount: "",
    description: "",
    odometerKm: "",
  });

  const [fuelTypes, setFuelTypes] = useState<FuelType[]>([]);
  const [fuelTypeId, setFuelTypeId] = useState<string>("");
  const [unit, setUnit] = useState<string>("");
  const [volume, setVolume] = useState<string>("");
  const [pricePerUnit, setPricePerUnit] = useState<string>("");
  const [isFullTank, setIsFullTank] = useState(true);
  const [station, setStation] = useState("");

  const isFuel = useMemo(() => {
    const c = categories.find((x) => x.id === form.categoryId);
    return c ? c.name.toLowerCase() === "paliwo" : false;
  }, [form.categoryId, categories]);

  useEffect(() => {
    if (!isFuel) return;
    (async () => {
      const vehicle = await getVehicle(activeVehicleId);
      const list = vehicle.fuelTypes ?? [];
      setFuelTypes(list);
      if (list.length === 1) {
        setFuelTypeId(list[0].id);
        setUnit(list[0].defaultUnit);
      }
    })();
  }, [isFuel, activeVehicleId]);

  useEffect(() => {
    if (!fuelTypeId || fuelTypes.length === 0) return;
    const ft = fuelTypes.find((f) => f.id === fuelTypeId);
    if (ft) setUnit(ft.defaultUnit);
  }, [fuelTypeId, fuelTypes]);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!form.date) {
      alert("Podaj datę wydatku.");
      return;
    }

    if (!isFuel) {
      const amountNum = Number(form.amount);
      if (isNaN(amountNum) || amountNum <= 0) {
        alert("Kwota musi być liczbą dodatnią.");
        return;
      }

      const odometerNum = form.odometerKm ? Number(form.odometerKm) : null;
      if (odometerNum !== null && (isNaN(odometerNum) || odometerNum < 0)) {
        alert("Przebieg musi być >= 0.");
        return;
      }

      const payload: CreateExpenseRequest = {
        date: form.date,
        categoryId: form.categoryId as Guid,
        amount: amountNum,
        description: form.description?.trim() || null,
        odometerKm: odometerNum,
      };

      const created = await createExpense(activeVehicleId, payload);
      onCreated(created);
      onClose();
      return;
    }

    if (!fuelTypeId) {
      alert("Wybierz typ paliwa.");
      return;
    }

    const volumeNum = Number(volume);
    const priceNum = Number(pricePerUnit);
    if (isNaN(volumeNum) || volumeNum <= 0) {
      alert("Ilość paliwa musi być liczbą dodatnią.");
      return;
    }

    const odometerNum = form.odometerKm ? Number(form.odometerKm) : 0;

    const created = await createFuelEntry(activeVehicleId, {
      fuelTypeId: fuelTypeId as Guid,
      unit: unit || null,
      date: form.date,
      volume: volumeNum,
      pricePerUnit: isNaN(priceNum) ? 0 : priceNum,
      odometerKm: odometerNum,
      isFullTank,
      station: station?.trim() || null,
    });

    onCreated(created as Expense);
    onClose();
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
      <div className="bg-white rounded-2xl shadow-xl p-8 w-full max-w-md relative">
        <h2 className="text-2xl font-bold text-gray-800 mb-6">Dodaj wydatek</h2>

        <button
          onClick={onClose}
          className="text-gray-400 hover:text-gray-600 transition absolute top-4 right-4"
        >
          <X className="w-8 h-8" />
        </button>

        <form onSubmit={submit} className="flex flex-col gap-4">
          <input
            type="date"
            value={form.date}
            onChange={(e) => setForm({ ...form, date: e.target.value })}
            className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
            required
          />

          <select
            value={form.categoryId}
            onChange={(e) => setForm({ ...form, categoryId: e.target.value })}
            className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
            required
          >
            <option value="">Kategoria</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>

          {!isFuel && (
            <>
              <input
                type="number"
                step="0.01"
                placeholder="Kwota (PLN)"
                value={form.amount}
                onChange={(e) =>
                  setForm({ ...form, amount: e.target.value })
                }
                className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                required
              />

              <input
                type="number"
                placeholder="Przebieg (opcjonalnie)"
                value={form.odometerKm}
                onChange={(e) =>
                  setForm({ ...form, odometerKm: e.target.value })
                }
                className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
              />

              <input
                type="text"
                placeholder="Opis (opcjonalnie)"
                value={form.description}
                onChange={(e) =>
                  setForm({ ...form, description: e.target.value })
                }
                className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
              />
            </>
          )}

          {isFuel && (
            <>
              <select
                value={fuelTypeId}
                onChange={(e) => setFuelTypeId(e.target.value)}
                className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                required
              >
                <option value="">Typ paliwa</option>
                {fuelTypes.map((f) => (
                  <option key={f.id} value={f.id}>
                    {f.name} {f.defaultUnit ? `(${f.defaultUnit})` : ""}
                  </option>
                ))}
              </select>

              <div className="grid grid-cols-2 gap-3">
                <input
                  type="number"
                  step="0.001"
                  placeholder="Ilość (L/kWh)"
                  value={volume}
                  onChange={(e) => setVolume(e.target.value)}
                  className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                  required
                />
                <input
                  type="number"
                  step="0.001"
                  placeholder="Cena / jednostkę"
                  value={pricePerUnit}
                  onChange={(e) => setPricePerUnit(e.target.value)}
                  className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <input
                  type="number"
                  placeholder="Przebieg (km)"
                  value={form.odometerKm}
                  onChange={(e) =>
                    setForm({ ...form, odometerKm: e.target.value })
                  }
                  className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                />
                <input
                  type="text"
                  placeholder="Stacja (opcjonalnie)"
                  value={station}
                  onChange={(e) => setStation(e.target.value)}
                  className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                />
              </div>

              <label className="inline-flex items-center gap-2">
                <input
                  type="checkbox"
                  checked={isFullTank}
                  onChange={(e) => setIsFullTank(e.target.checked)}
                />
                <span>Pełny bak</span>
              </label>
            </>
          )}

          <button
            type="submit"
            className="text-lg font-semibold px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-500 transition"
          >
            Zapisz
          </button>
        </form>
      </div>
    </div>
  );
}
