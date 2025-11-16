import { useState, useEffect } from "react";
import { X } from "lucide-react";
import type { Guid, Reminder } from "../api/types";
import { createReminder, updateReminder } from "../api/reminders";

interface ReminderFormProps {
  onClose: () => void;
  activeVehicleId: Guid;
  onSaved: (rem: Reminder) => void;
  initialData?: Reminder | null;
}

export default function ReminderForm({
  onClose,
  activeVehicleId,
  onSaved,
  initialData,
}: ReminderFormProps) {
  const [title, setTitle] = useState("");
  const [dueDate, setDueDate] = useState("");
  const [description, setDescription] = useState("");

  const [titleError, setTitleError] = useState("");
  const [dateError, setDateError] = useState("");

  useEffect(() => {
    if (!initialData) return;
    setTitle(initialData.title ?? "");
    setDueDate(initialData.dueDate ?? "");
    setDescription(initialData.description ?? "");
  }, [initialData]);

  const validate = () => {
    let ok = true;
    if (!title.trim()) {
      setTitleError("Podaj tytuł przypomnienia.");
      ok = false;
    } else {
      setTitleError("");
    }

    if (!dueDate) {
      setDateError("Podaj termin przypomnienia.");
      ok = false;
    } else {
      setDateError("");
    }

    return ok;
  };

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    try {
      if (!initialData) {
        const created = await createReminder(activeVehicleId, {
          title: title.trim(),
          description: description.trim() || null,
          dueDate,
        });
        onSaved(created);
      } else {
        await updateReminder(initialData.id, {
          title: title.trim(),
          description: description.trim() || null,
          dueDate,
        });

        const updated: Reminder = {
          ...initialData,
          title: title.trim(),
          description: description.trim() || null,
          dueDate,
        };

        onSaved(updated);
      }

      onClose();
    } catch (err: any) {
      console.error("Błąd zapisu przypomnienia", err);
      alert(err?.response?.data ?? "Nie udało się zapisać przypomnienia");
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
      <div className="bg-white rounded-2xl shadow-xl p-8 w-full max-w-md relative">
        <h2 className="text-2xl font-bold text-gray-800 mb-6">
          {initialData ? "Edytuj przypomnienie" : "Dodaj przypomnienie"}
        </h2>

        <button
          onClick={onClose}
          className="text-gray-400 hover:text-gray-600 transition absolute top-4 right-4"
        >
          <X className="w-8 h-8" />
        </button>

        <form onSubmit={submit} className="flex flex-col gap-4">
          <div>
            <input
              type="text"
              placeholder="Tytuł (np. Przegląd, Ubezpieczenie)"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none w-full"
            />
            {titleError && (
              <p className="text-red-600 text-sm mt-1">{titleError}</p>
            )}
          </div>

          <div>
            <input
              type="date"
              value={dueDate}
              onChange={(e) => setDueDate(e.target.value)}
              className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none w-full"
            />
            {dateError && (
              <p className="text-red-600 text-sm mt-1">{dateError}</p>
            )}
          </div>

          <textarea
            placeholder="Opis (opcjonalnie)"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none w-full min-h-[80px]"
          />

          <button
            type="submit"
            className="text-lg font-semibold px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-500 transition"
          >
            {initialData ? "Zapisz zmiany" : "Zapisz"}
          </button>
        </form>
      </div>
    </div>
  );
}
