import { useEffect, useState } from "react";
import Sidebar from "../components/Sidebar";
import Topbar from "../components/Topbar";
import VehicleForm from "../components/VehicleForm";
import { Fuel, Pencil, Trash2 } from "lucide-react";

import {
  listVehicles,
  createVehicle,
  getVehicle,
  assignFuelType,
  getVehicleOdometer,
  deleteVehicle,
  updateVehicle,
  unassignFuelType,
  
} from "../api/vehicles";
import type { Vehicle, VehicleDetails } from "../api/types";

export default function Vehicles() {
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [vehicles, setVehicles] = useState<(Vehicle | VehicleDetails | any)[]>([]);
  const [vehicleToDelete, setVehicleToDelete] = useState<number | null>(null);
  const [vehicleToEdit, setVehicleToEdit] = useState<number | null>(null);

  useEffect(() => {
    (async () => {
      try {
        const base = await listVehicles();

        const detailed = await Promise.all(
          base.map(async (v) => {
            try {
              const det = await getVehicle(v.id);
              return det;
            } catch {
              return v;
            }
          })
        );

        const withOdo = await Promise.all(
          detailed.map(async (v: any) => {
            try {
              const odo = await getVehicleOdometer(v.id);
              return { ...v, odometerKm: odo.odometerKm };
            } catch {
              return { ...v, odometerKm: null };
            }
          })
        );

        setVehicles(withOdo);
      } catch (e) {
        console.error("Błąd listVehicles()", e);
      }
    })();
  }, []);

  const handleAddVehicle = async (vehicleData: any) => {
  try {
    const payload = {
      name: vehicleData.name.trim(),
      make: vehicleData.brand.trim(),
      model: vehicleData.model.trim(),
      plate: vehicleData.registration.trim(),
      year: vehicleData.year ? Number(vehicleData.year) : null,
      baseOdometerKm: vehicleData.kilometers
        ? Number(vehicleData.kilometers)
        : null,
};


    const created = await createVehicle(payload);

      const selectedFuelTypeIds: string[] = vehicleData.selectedFuelTypeIds ?? [];
      for (const ftId of selectedFuelTypeIds) {
        await assignFuelType(created.id, ftId);
      }

      const detailed = await getVehicle(created.id);
      const odo = await getVehicleOdometer(created.id);

      setVehicles((prev) => [{ ...detailed, odometerKm: odo.odometerKm }, ...prev]);
      setIsFormOpen(false);
    } catch (e: any) {
      alert(e?.response?.data ?? "Błąd zapisu pojazdu");
    }
  };

  const handledDeleteVehicle = (index: number) => setVehicleToDelete(index);

  const confirmDelete = async () => {
  if (vehicleToDelete === null) return;

  const toDelete = vehicles[vehicleToDelete];
  const id = toDelete?.id;

  if (!id) {
    setVehicles((prev) => prev.filter((_, i) => i !== vehicleToDelete));
    setVehicleToDelete(null);
    return;
  }

  try {
    await deleteVehicle(id);
    setVehicles((prev) => prev.filter((_, i) => i !== vehicleToDelete));
  } catch (e: any) {
    console.error("Błąd usuwania pojazdu", e);
    alert(e?.response?.data ?? "Nie udało się usunąć pojazdu");
  } finally {
    setVehicleToDelete(null);
  }
};


  const cancelDelete = () => setVehicleToDelete(null);

  const handleEditVehicle = (index: number) => {
    setVehicleToEdit(index);
    setIsFormOpen(true);
  };

    const handleUpdateVehicle = async (vehicleData: any) => {
    if (vehicleToEdit === null) return;

    const existing: any = vehicles[vehicleToEdit];
    const id = existing.id;
    if (!id) return;

    const payload = {
      name: vehicleData.name.trim(),
      make: vehicleData.brand.trim(),
      model: vehicleData.model.trim(),
      plate: vehicleData.registration.trim(),
      year: vehicleData.year ? Number(vehicleData.year) : null,
      baseOdometerKm: vehicleData.kilometers
        ? Number(vehicleData.kilometers)
        : null,
    };


        try {
      await updateVehicle(id, payload);

      const currentFuelIds: string[] = (existing.fuelTypes ?? []).map((f: any) => f.id);
      const newFuelIds: string[] = vehicleData.selectedFuelTypeIds ?? [];

      const toAdd = newFuelIds.filter((x) => !currentFuelIds.includes(x));
      const toRemove = currentFuelIds.filter((x) => !newFuelIds.includes(x));

      for (const ftId of toAdd) {
        await assignFuelType(id, ftId);
      }
      for (const ftId of toRemove) {
        await unassignFuelType(id, ftId);
      }

      // ⬇ pobieramy świeże dane pojazdu + świeży przebieg
      const [fresh, odo] = await Promise.all([
        getVehicle(id),
        getVehicleOdometer(id),
      ]);

      setVehicles((prev) =>
        prev.map((v, i) =>
          i === vehicleToEdit ? { ...fresh, odometerKm: odo.odometerKm } : v
        )
      );

      setVehicleToEdit(null);
      setIsFormOpen(false);
    } catch (e: any) {
      console.error("Błąd edycji pojazdu", e);
      alert(e?.response?.data ?? "Nie udało się zaktualizować pojazdu");
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
              <h1 className="text-3xl font-bold text-gray-800">Pojazdy</h1>
              <p className="text-gray-500 text-lg mt-2">Zarządzaj swoimi pojazdami</p>
            </div>
            <button
              onClick={() => {
                setIsFormOpen(true);
                setVehicleToEdit(null);
              }}
              className="bg-blue-600 text-white font-medium px-4 py-2 rounded-lg shadow-md hover:bg-blue-500 transition"
            >
              + Dodaj pojazd
            </button>
          </div>

          {isFormOpen && (
            <VehicleForm
              onClose={() => {
                setIsFormOpen(false);
                setVehicleToEdit(null);
              }}
              onAddVehicle={vehicleToEdit === null ? handleAddVehicle : handleUpdateVehicle}
              initialData={vehicleToEdit !== null ? vehicles[vehicleToEdit] : null}
            />
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mt-8">
            {vehicles.map((vehicle: any, index) => (
              <div
                key={vehicle.id ?? index}
                className="bg-white rounded-2xl shadow-md p-6 border border-gray-100 relative"
              >
                <div className="flex justify-between items-start mb-4">
                  <div>
                    <h3 className="text-xl font-bold text-gray-800 capitalize">
                      {vehicle.make} {vehicle.model}
                    </h3>
                    <h2 className="text-sm font-bold text-gray-600">{vehicle.name}</h2>
                    <p className="text-gray-500 text-sm">{vehicle.year ?? "—"}</p>
                  </div>

                  <div className="flex items-center gap-2 text-sm font-medium border bg-gray-200 border-gray-300 rounded-full px-3 py-1 text-gray-700">
                    <Fuel className="w-4 h-4 text-gray-700" />
                    <span className="text-gray-700 font-bold text-sm">
                      {vehicle.fuelTypes && vehicle.fuelTypes.length > 0
                        ? vehicle.fuelTypes.map((ft: any) => ft.name).join(" / ")
                        : "Brak"}
                    </span>
                  </div>
                </div>

                <div className="mt-4 border-t border-gray-100 pt-4">
                  <div className="flex justify-between text-gray-800 font-bold">
                    <span className="font-medium text-gray-500">Numer rejestracyjny</span>
                    <span className="uppercase">{vehicle.plate ?? "—"}</span>
                  </div>
                  <div className="flex justify-between text-gray-800 font-bold">
                    <span className="font-medium text-gray-500">Przebieg</span>
                    <span>
                      {vehicle.odometerKm != null ? `${vehicle.odometerKm} km` : "— km"}
                    </span>
                  </div>
                </div>

                <div className="flex mt-4 gap-2">
                  <button
                    onClick={() => handleEditVehicle(index)}
                    className="flex-1 flex items-center justify-center gap-2 bg-gray-100 text-gray-800 font-semibold border border-gray-200 shadow-md px-4 py-2 rounded-lg hover:bg-blue-500 hover:text-white transition"
                  >
                    <Pencil className="w-4 h-4" />
                    Edytuj
                  </button>
                  <button
                    onClick={() => handledDeleteVehicle(index)}
                    className="flex items-center justify-center bg-red-600 text-white p-2 rounded-lg hover:bg-red-500 transition"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>
            ))}
          </div>

          {vehicleToDelete !== null && (
            <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
              <div className="bg-white p-6 rounded-2xl shadow-xl w-full max-w-sm text-center">
                <h2 className="text-xl font-bold text-gray-800 mb-2">Usuń pojazd</h2>
                <p className="text-gray-600 mb-6">Czy na pewno chcesz usunąć pojazd?</p>
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
