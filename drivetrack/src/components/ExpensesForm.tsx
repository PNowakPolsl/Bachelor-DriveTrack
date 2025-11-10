// src/components/ExpensesForm.tsx
import { useState, useEffect } from "react";
import { X } from "lucide-react";
import type { Category, CreateExpenseRequest, Expense, Guid } from "../api/types";
import { createExpense } from "../api/expenses";

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

    const submit = async (e: React.FormEvent) => {
        e.preventDefault();

    const amountNum = Number(form.amount);
    if (isNaN(amountNum) || amountNum <= 0) {
        alert("Kwota musi być liczbą dodatnią.");
        return;
    }

    const odometerNum = form.odometerKm ? Number(form.odometerKm) : null;
    if (odometerNum !== null && (isNaN(odometerNum) || odometerNum < 0)) {
        alert("Przebieg musi być liczbą większą lub równą 0.");
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
    setForm({ date: "", categoryId: "", amount: "", description: "", odometerKm: "" });
    onClose();
 };


  useEffect(() => {
    if (!form.categoryId && categories.length > 0) {
        setForm((f) => ({ ...f, categoryId: categories[0].id }));
    }
  }, [categories]);

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

          <input
            type="number"
            step="0.01"
            placeholder="Kwota (PLN)"
            value={form.amount}
            onChange={(e) => setForm({ ...form, amount: e.target.value })}
            className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
            required
          />

          <input
            type="number"
            placeholder="Przebieg (opcjonalnie)"
            value={form.odometerKm}
            onChange={(e) => setForm({ ...form, odometerKm: e.target.value })}
            className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
          />

          <input
            type="text"
            placeholder="Opis (opcjonalnie)"
            value={form.description}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
            className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
          />

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
