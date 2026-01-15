import { useEffect, useState } from "react";
import {
  listVehicleUsers,
  addVehicleUser,
  removeVehicleUser,
  type VehicleUser,
  type AddVehicleUserRequest,
} from "../api/vehicles";
import type { Guid } from "../api/types";
import { X } from "lucide-react";

type Props = {
  vehicleId: Guid;
};

export default function VehicleUsersSection({ vehicleId }: Props) {
  const [users, setUsers] = useState<VehicleUser[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [form, setForm] = useState<AddVehicleUserRequest>({
    email: "",
    role: "Driver",
  });

  const [userToRemove, setUserToRemove] = useState<VehicleUser | null>(null);

  async function load() {
    try {
      setLoading(true);
      const data = await listVehicleUsers(vehicleId);
      setUsers(data);
    } catch (e: any) {
      setError(e?.response?.data ?? "Nie udało się pobrać użytkowników.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, [vehicleId]);

  async function handleAdd(e: React.FormEvent) {
    e.preventDefault();
    setError("");

    try {
      await addVehicleUser(vehicleId, form);
      setForm({ email: "", role: "Driver" });
      await load();
    } catch (e: any) {
      setError(e?.response?.data ?? "Nie udało się dodać użytkownika.");
    }
  }

  function askRemove(user: VehicleUser) {
    setUserToRemove(user);
    setError("");
  }

  async function confirmRemove() {
    if (!userToRemove) return;

    try {
      await removeVehicleUser(vehicleId, userToRemove.id);
      setUserToRemove(null);
      await load();
    } catch (e: any) {
      setError(e?.response?.data ?? "Nie udało się usunąć użytkownika.");
    }
  }

  function cancelRemove() {
    setUserToRemove(null);
  }

  return (
    <>
      <div className="mt-10 bg-white rounded-2xl shadow-md p-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-xl font-semibold text-gray-800">
            Użytkownicy pojazdu
          </h2>
        </div>

        {error && (
          <p className="mb-4 text-sm text-red-600 font-medium">{error}</p>
        )}

        <form onSubmit={handleAdd} className="flex flex-wrap gap-3 mb-6">
          <input
            type="email"
            placeholder="E-mail użytkownika"
            value={form.email}
            onChange={(e) => setForm({ ...form, email: e.target.value })}
            className="flex-1 min-w-[220px] border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
          <select
            value={form.role}
            onChange={(e) =>
              setForm({
                ...form,
                role: e.target.value as AddVehicleUserRequest["role"],
              })
            }
            className="border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="Driver">Kierowca</option>
            <option value="Viewer">Podgląd</option>
            <option value="Owner">Właściciel</option>
          </select>
          <button
            type="submit"
            className="bg-blue-600 text-white font-semibold px-4 py-2 rounded-lg hover:bg-blue-500 transition"
          >
            Dodaj użytkownika
          </button>
        </form>

        {loading ? (
          <p className="text-gray-500">Ładowanie...</p>
        ) : users.length === 0 ? (
          <p className="text-gray-500">
            Ten pojazd nie ma jeszcze przypisanych użytkowników.
          </p>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b">
                <th className="py-2">Imię i nazwisko</th>
                <th className="py-2">E-mail</th>
                <th className="py-2">Rola</th>
                <th className="py-2 w-12" />
              </tr>
            </thead>
            <tbody>
              {users.map((u) => (
                <tr key={u.id} className="border-b last:border-0">
                  <td className="py-2">{u.name}</td>
                  <td className="py-2 text-gray-600">{u.email}</td>
                  <td className="py-2">
                    <span className="inline-flex px-2 py-1 rounded-full text-xs font-semibold bg-gray-100 text-gray-700">
                      {u.role === "Owner"
                        ? "Właściciel"
                        : u.role === "Driver"
                        ? "Kierowca"
                        : "Podgląd"}
                    </span>
                  </td>
                  <td className="py-2 text-right">
                    {u.role !== "Owner" && (
                      <button
                        type="button"
                        onClick={() => askRemove(u)}
                        className="p-1 rounded-full hover:bg-red-50"
                        title="Usuń dostęp"
                      >
                        <X className="w-4 h-4 text-red-600" />
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {userToRemove && (
        <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
          <div className="bg-white p-6 rounded-2xl shadow-xl w-full max-w-sm text-center">
            <h2 className="text-xl font-bold text-gray-800 mb-2">
              Usuń użytkownika pojazdu
            </h2>
            <p className="text-gray-600 mb-4">
              Czy na pewno chcesz usunąć dostęp użytkownika
            </p>
            <p className="font-semibold text-gray-800 mb-6">
              {userToRemove.name || userToRemove.email}
            </p>
            <div className="flex justify-center gap-4">
              <button
                onClick={cancelRemove}
                className="px-5 py-2 rounded-lg bg-gray-200 text-gray-800 hover:bg-gray-300 transition"
              >
                Anuluj
              </button>
              <button
                onClick={confirmRemove}
                className="px-5 py-2 rounded-lg bg-red-600 text-white hover:bg-red-500 transition"
              >
                Tak, usuń
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
