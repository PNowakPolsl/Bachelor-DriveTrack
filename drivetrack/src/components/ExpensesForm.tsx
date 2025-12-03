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
import { createFuelEntry, listStations } from "../api/fuel";
import { getVehicle, getVehicleOdometer } from "../api/vehicles";


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

  const [lastOdometer, setLastOdometer] = useState<number | null>(null);
  const [odometerError, setOdometerError] = useState<string>("");

  const [fuelTypes, setFuelTypes] = useState<FuelType[]>([]);
  const [fuelTypeId, setFuelTypeId] = useState<string>("");

  const [unit, setUnit] = useState("");
  const [volume, setVolume] = useState("");
  const [pricePerUnit, setPricePerUnit] = useState("");
  const [station, setStation] = useState("");
  const [isFullTank, setIsFullTank] = useState(true);

  // 🔔 Czy pokazywać okienko z potwierdzeniem przyszłej daty
  const [showFutureConfirm, setShowFutureConfirm] = useState(false);

  const isFuel = useMemo(() => {
    const c = categories.find((x) => x.id === form.categoryId);
    return c ? c.name.toLowerCase() === "paliwo" : false;
  }, [form.categoryId, categories]);

  const [knownStations, setKnownStations] = useState<string[]>([]);

  useEffect(() => {
    (async () => {
      const odo = await getVehicleOdometer(activeVehicleId);
      setLastOdometer(odo.odometerKm);
    })();
  }, [activeVehicleId]);

  useEffect(() => {
  if (!isFuel) return;

  (async () => {
    // 1) paliwa
    const vehicle = await getVehicle(activeVehicleId);
    setFuelTypes(vehicle.fuelTypes);

    if (vehicle.fuelTypes.length === 1) {
      setFuelTypeId(vehicle.fuelTypes[0].id);
      setUnit(vehicle.fuelTypes[0].defaultUnit);
    }

    // 2) STACJE dla tego pojazdu
    try {
      const stations = await listStations(activeVehicleId);
      setKnownStations(stations);
    } catch (e) {
      console.error("Nie udało się pobrać stacji", e);
    }
  })();
}, [isFuel, activeVehicleId]);


  useEffect(() => {
    const ft = fuelTypes.find((f) => f.id === fuelTypeId);
    if (ft) setUnit(ft.defaultUnit);
  }, [fuelTypeId, fuelTypes]);

  const validateOdometer = (raw: string) => {
    const num = raw ? Number(raw) : null;

    if (num === null) {
      setOdometerError("");
      return true;
    }

    if (isNaN(num) || num < 0) {
      setOdometerError("Przebieg musi być liczbą dodatnią.");
      return false;
    }

    if (lastOdometer !== null && num < lastOdometer) {
      setOdometerError(
        `Przebieg nie może być mniejszy niż ${lastOdometer} km`
      );
      return false;
    }

    setOdometerError("");
    return true;
  };

  // 🔍 Sprawdzenie, czy data jest w przyszłości ( > dzisiaj )
  const isDateInFuture = () => {
    if (!form.date) return false;

    // form.date to "YYYY-MM-DD"
    const selected = new Date(form.date + "T00:00:00");
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return selected.getTime() > today.getTime();
  };

  // 🧠 właściwy zapis (to co wcześniej było w środku submit)
  const performSubmit = async () => {
    if (!validateOdometer(form.odometerKm)) {
      return;
    }

    const odometerNum = form.odometerKm ? Number(form.odometerKm) : null;

    try {
      if (!isFuel) {
        const payload: CreateExpenseRequest = {
          date: form.date,
          categoryId: form.categoryId as Guid,
          amount: Number(form.amount),
          description: form.description?.trim() || null,
          odometerKm: odometerNum,
        };

        const created = await createExpense(activeVehicleId, payload);
        onCreated(created);
        onClose();
        return;
      }

      const created = await createFuelEntry(activeVehicleId, {
        fuelTypeId,
        date: form.date,
        volume: Number(volume),
        unit,
        pricePerUnit: Number(pricePerUnit) || 0,
        odometerKm: odometerNum!,
        station: station.trim(),
        isFullTank,
      });


      onCreated(created as Expense);
      onClose();
    } catch (e: any) {
      console.error(e);
      alert(e?.response?.data ?? "Błąd zapisu wydatku");
    }
  };

  // 🔘 obsługa kliknięcia Zapisz
  const submit = async (e: React.FormEvent) => {
    e.preventDefault();

    // jeśli data w przyszłości → pokaż popup i dopiero TAM zdecydujemy
    if (isDateInFuture()) {
      setShowFutureConfirm(true);
      return;
    }

    await performSubmit();
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
      <div className="bg-white rounded-2xl shadow-xl p-8 w-full max-w-md relative">
        <h2 className="text-2xl font-bold mb-6">Dodaj wydatek</h2>

        <button
          onClick={onClose}
          className="absolute top-4 right-4 text-gray-400 hover:text-gray-600"
        >
          <X className="w-8 h-8" />
        </button>

        <form onSubmit={submit} className="flex flex-col gap-4">
          {/* DATA */}
          <input
            type="date"
            required
            value={form.date}
            onChange={(e) => setForm({ ...form, date: e.target.value })}
            className="border rounded-lg p-3"
          />

          {/* KATEGORIA */}
          <select
            required
            value={form.categoryId}
            onChange={(e) =>
              setForm({ ...form, categoryId: e.target.value })
            }
            className="border rounded-lg p-3"
          >
            <option value="">Kategoria</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>

          {/* JEŚLI NIE PALIWO */}
          {!isFuel && (
            <>
              <input
                type="number"
                step="0.01"
                required
                placeholder="Kwota"
                value={form.amount}
                onChange={(e) =>
                  setForm({ ...form, amount: e.target.value })
                }
                className="border rounded-lg p-3"
              />

              <div>
                <input
                  type="number"
                  placeholder="Przebieg (opcjonalnie)"
                  value={form.odometerKm}
                  onChange={(e) => {
                    setForm({ ...form, odometerKm: e.target.value });
                    validateOdometer(e.target.value);
                  }}
                  className="border rounded-lg p-3 w-full"
                />
                {odometerError && (
                  <p className="text-red-600 text-sm mt-1">
                    {odometerError}
                  </p>
                )}
              </div>

              <input
                type="text"
                placeholder="Opis (opcjonalnie)"
                value={form.description}
                onChange={(e) =>
                  setForm({ ...form, description: e.target.value })
                }
                className="border rounded-lg p-3"
              />
            </>
          )}

          {/* JEŚLI PALIWO */}
          {isFuel && (
            <>
              <select
                required
                value={fuelTypeId}
                onChange={(e) => setFuelTypeId(e.target.value)}
                className="border rounded-lg p-3"
              >
                <option value="">Typ paliwa</option>
                {fuelTypes.map((f) => (
                  <option key={f.id} value={f.id}>
                    {f.name}
                  </option>
                ))}
              </select>

              <input
                type="number"
                required
                step="0.001"
                placeholder="Ilość"
                value={volume}
                onChange={(e) => setVolume(e.target.value)}
                className="border rounded-lg p-3"
              />

              <input
                type="number"
                placeholder="Cena / jednostkę"
                value={pricePerUnit}
                onChange={(e) => setPricePerUnit(e.target.value)}
                className="border rounded-lg p-3"
              />

              {/* przebieg + stacja */}
              <div>
                <div className="grid grid-cols-2 gap-3">
                  <input
                    type="number"
                    required
                    placeholder="Przebieg (km)"
                    value={form.odometerKm}
                    onChange={(e) => {
                      setForm({ ...form, odometerKm: e.target.value });
                      validateOdometer(e.target.value);
                    }}
                    className="border rounded-lg p-3 w-full"
                  />
                  <input
                    list="stationSuggestions"
                    type="text"
                    required
                    placeholder="Stacja"
                    value={station}
                    onChange={(e) => setStation(e.target.value)}
                    className="border rounded-lg p-3 w-full"
                  />

                  <datalist id="stationSuggestions">
                    {knownStations.map((s) => (
                      <option key={s} value={s} />
                    ))}
                  </datalist>

                </div>
                {odometerError && (
                  <p className="text-red-600 text-sm mt-1">
                    {odometerError}
                  </p>
                )}
              </div>

              {/* PEŁNY BAK */}
              <label className="inline-flex items-center gap-2 mt-1">
                <input
                  type="checkbox"
                  checked={isFullTank}
                  onChange={(e) => setIsFullTank(e.target.checked)}
                  className="h-4 w-4"
                />
                <span>Pełny bak</span>
              </label>
            </>
          )}

          <button
            type="submit"
            className="bg-blue-600 hover:bg-blue-500 text-white p-3 rounded-lg font-semibold"
          >
            Zapisz
          </button>
        </form>
      </div>

      {/* 🔔 MODAL POTWIERDZENIA PRZYSZŁEJ DATY */}
      {showFutureConfirm && (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50 z-60">
          <div className="bg-white rounded-2xl shadow-xl p-6 w-full max-w-sm text-center">
            <h3 className="text-lg font-semibold text-gray-800 mb-2">
              Data w przyszłości
            </h3>
            <p className="text-gray-600 mb-6">
              Czy na pewno chcesz dodać wydatek na przyszłość?
            </p>
            <div className="flex justify-center gap-4">
              <button
                onClick={() => setShowFutureConfirm(false)}
                className="px-5 py-2 rounded-lg bg-gray-200 text-gray-800 hover:bg-gray-300 transition"
              >
                Nie
              </button>
              <button
                onClick={async () => {
                  setShowFutureConfirm(false);
                  await performSubmit();
                }}
                className="px-5 py-2 rounded-lg bg-blue-600 text-white hover:bg-blue-500 transition"
              >
                Tak
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
