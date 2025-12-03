import { useParams, useNavigate } from "react-router-dom";
import Sidebar from "../components/Sidebar";
import Topbar from "../components/Topbar";
import VehicleUsersSection from "../components/VehicleUsersSection";
import type { Guid } from "../api/types";

export default function VehicleUsersPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  

  if (!id) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <p className="text-gray-700 text-lg">
          Nieprawidłowy adres (brak ID pojazdu w URL).
        </p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 flex">
      <Sidebar />
      <div className="flex flex-col flex-1">
        <Topbar />
        <main className="p-6">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h1 className="text-2xl font-bold text-gray-800">
                Użytkownicy pojazdu
              </h1>
              <p className="text-gray-500">
                Zarządzaj dostępem innych osób do tego pojazdu
              </p>
            </div>
            <button
              onClick={() => navigate("/vehicles")}
              className="px-4 py-2 rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-100 transition"
            >
              ← Wróć do pojazdów
            </button>
          </div>

          <VehicleUsersSection vehicleId={id as Guid} />
        </main>
      </div>
    </div>
  );
}
