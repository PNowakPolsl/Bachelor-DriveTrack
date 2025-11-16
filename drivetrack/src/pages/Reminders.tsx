import Sidebar from "../components/Sidebar";
import Topbar from "../components/Topbar";
import { useEffect, useMemo, useState } from "react";
import ReminderForm from "../components/ReminderForm";
import { CalendarDays, Pencil, Trash2 } from "lucide-react";

import { listVehicles } from "../api/vehicles";
import { listReminders, deleteReminder } from "../api/reminders";
import type { Vehicle, Reminder, Guid } from "../api/types";

export default function Reminders() {
  const [isFormOpen, setIsFormOpen] = useState(false);

  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [activeVehicleId, setActiveVehicleId] = useState<Guid | null>(null);

  const [reminders, setReminders] = useState<Reminder[]>([]);

  const [reminderToDelete, setReminderToDelete] = useState<Reminder | null>(null);
  const [reminderToEdit, setReminderToEdit] = useState<Reminder | null>(null);

  useEffect(() => {
    (async () => {
      try {
        const v = await listVehicles();
        setVehicles(v);
        if (v.length && !activeVehicleId) {
          setActiveVehicleId(v[0].id);
        }
      } catch (e) {
        console.error("Błąd pobierania pojazdów", e);
      }
    })();
  }, []);

  useEffect(() => {
    if (!activeVehicleId) return;
    (async () => {
      try {
        const data = await listReminders(activeVehicleId);
        setReminders(data);
      } catch (e) {
        console.error("Błąd pobierania przypomnień", e);
      }
    })();
  }, [activeVehicleId]);

  const activeVehicle = useMemo(
    () => vehicles.find((v) => v.id === activeVehicleId) ?? null,
    [vehicles, activeVehicleId]
  );

  const handleAddOrUpdateReminder = (reminder: Reminder) => {
    setReminders((prev) => {
      const exists = prev.find((r) => r.id === reminder.id);
      if (!exists) {
        return [...prev, reminder];
      }
      return prev.map((r) => (r.id === reminder.id ? reminder : r));
    });
  };

  const handleEditReminder = (rem: Reminder) => {
    setReminderToEdit(rem);
    setIsFormOpen(true);
  };

  const handleOpenCreate = () => {
    setReminderToEdit(null);
    setIsFormOpen(true);
  };

  const handleDeleteClick = (rem: Reminder) => {
    setReminderToDelete(rem);
  };

  const confirmDelete = async () => {
    if (!reminderToDelete) return;
    try {
      await deleteReminder(reminderToDelete.id);
      setReminders((prev) => prev.filter((r) => r.id !== reminderToDelete.id));
    } catch (e: any) {
      console.error("Błąd usuwania przypomnienia", e);
      alert(e?.response?.data ?? "Nie udało się usunąć przypomnienia");
    } finally {
      setReminderToDelete(null);
    }
  };

  const cancelDelete = () => setReminderToDelete(null);

  const daysLeftBadge = (dueDate: string) => {
    const today = new Date();
    const reminderDate = new Date(dueDate);
    const diffTime = reminderDate.getTime() - today.getTime();
    const daysLeft = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    let color =
      "text-green-500 bg-green-50 rounded-full px-3 py-1 hover:bg-green-200 transition";
    if (daysLeft <= 2) {
      color =
        "text-red-600 bg-red-50 rounded-full px-3 py-1 hover:bg-red-200 transition";
    } else if (daysLeft <= 7) {
      color =
        "text-yellow-500 bg-orange-50 rounded-full px-3 py-1 hover:bg-yellow-200 transition";
    }

    return <span className={color}>{daysLeft} dni</span>;
  };

  return (
    <div className="min-h-screen bg-gray-50 flex">
      <Sidebar />
      <div className="flex flex-col flex-1">
        <Topbar />
        <main className="p-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold text-gray-800">Przypomnienia</h1>
              <p className="text-gray-500 text-lg mt-2">
                Pamiętaj o najważniejszych wydarzeniach związanych z twoim
                pojazdem
              </p>
            </div>

            <div className="flex items-center gap-3">
              {/* wybór pojazdu */}
              <select
                className="border rounded-lg p-2"
                value={activeVehicleId ?? ""}
                onChange={(e) => setActiveVehicleId(e.target.value)}
              >
                {vehicles.map((v) => (
                  <option key={v.id} value={v.id}>
                    {v.make} {v.model} — {v.name}
                  </option>
                ))}
              </select>

              <button
                onClick={handleOpenCreate}
                className="bg-blue-600 text-white font-medium px-4 py-2 rounded-lg shadow-md hover:bg-blue-500 transition"
                disabled={!activeVehicleId}
              >
                + Dodaj przypomnienie
              </button>
            </div>
          </div>

          {/* FORMULARZ */}
          {isFormOpen && activeVehicleId && (
            <ReminderForm
              onClose={() => setIsFormOpen(false)}
              activeVehicleId={activeVehicleId}
              initialData={reminderToEdit}
              onSaved={handleAddOrUpdateReminder}
            />
          )}

          {/* LISTA PRZYPOMNIEŃ W FORMIE KART */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mt-8">
            {reminders.map((reminder) => (
              <div
                key={reminder.id}
                className="bg-white rounded-2xl shadow-md p-6 border border-gray-100 relative"
              >
                <div className="absolute top-4 right-4 text-sm font-bold">
                  {daysLeftBadge(reminder.dueDate)}
                </div>

                <div className="mb-4">
                  <h3 className="text-xl font-bold text-gray-800 capitalize">
                    {reminder.title}
                  </h3>
                  <p className="text-gray-500 text-sm">
                    {activeVehicle
                      ? `${activeVehicle.make} ${activeVehicle.model} — ${activeVehicle.name}`
                      : ""}
                  </p>
                </div>

                <p className="text-gray-700 mb-4">
                  {reminder.description ?? "—"}
                </p>
                <div className="flex items-center gap-2 text-gray-500 text-base mt-2">
                  <CalendarDays className="w-4 h-4 text-blue-500" />
                  <span>{reminder.dueDate}</span>
                </div>

                <div className="flex mt-4 gap-2">
                  <button
                    onClick={() => handleEditReminder(reminder)}
                    className="flex-1 flex items-center justify-center gap-2 bg-gray-100 text-gray-800 font-semibold border border-gray-200 shadow-md px-4 py-2 rounded-lg hover:bg-blue-500 hover:text-white transition"
                  >
                    <Pencil className="w-4 h-4" />
                    Edytuj
                  </button>
                  <button
                    onClick={() => handleDeleteClick(reminder)}
                    className="flex items-center justify-center bg-red-600 text-white p-2 rounded-lg hover:bg-red-500 transition"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>
            ))}

            {reminders.length === 0 && (
              <p className="text-gray-500 mt-4 col-span-full">
                Brak przypomnień dla tego pojazdu.
              </p>
            )}
          </div>

          {/* MODAL POTWIERDZENIA USUNIĘCIA */}
          {reminderToDelete && (
            <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
              <div className="bg-white p-6 rounded-2xl shadow-xl w-full max-w-sm text-center">
                <h2 className="text-xl font-bold text-gray-800 mb-2">
                  Usuń przypomnienie
                </h2>
                <p className="text-gray-600 mb-6">
                  Czy na pewno chcesz usunąć przypomnienie "{reminderToDelete.title}"?
                </p>
                <div className="flex justify-center gap-4">
                  <button
                    onClick={cancelDelete}
                    className="px-5 py-2 rounded-lg bg-gray-200 text-gray-800 hover:bg-gray-300 transition"
                  >
                    Anuluj
                  </button>
                  <button
                    onClick={confirmDelete}
                    className="px-5 py-2 rounded-lg bg-red-600 text-white hover:bg-red-500 transition "
                  >
                    Tak, usuń
                  </button>
                </div>
              </div>
            </div>
          )}
        </main>
      </div>
    </div>
  );
}
