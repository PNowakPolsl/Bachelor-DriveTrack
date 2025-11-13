import { useEffect, useState } from "react";
import { X } from "lucide-react";
import { listFuelTypes } from "../api/vehicles";

interface VehicleFormPops{
  onClose: () => void;
  onAddVehicle: (vehicleData: any) => void;
  initialData?: any | null;
}

type FuelTypeDict = { id: string; name: string; defaultUnit: string };

export default function VehicleForm({ onClose, onAddVehicle, initialData }: VehicleFormPops){
  const [formData, setFormData] = useState(() => {
  if (initialData) {
    const v: any = initialData;
    return {
      name: v.name ?? "",
      brand: v.make ?? "",
      model: v.model ?? "",
      registration: v.plate ?? "",
      year: v.year ? String(v.year) : "",
      kilometers: v.odometerKm != null ? String(v.odometerKm) : "",
      fuelType: "benzyna",
    };
  }
  return {
    name: "",
    brand: "",
    model: "",
    registration: "",
    year: "",
    kilometers: "",
    fuelType: "benzyna",
  };
});


  const [fuelTypes, setFuelTypes] = useState<FuelTypeDict[]>([]);
  const [selectedFuelTypeIds, setSelectedFuelTypeIds] = useState<string[]>([]);

    useEffect(() => {
    if (initialData && (initialData as any).fuelTypes) {
      const selected = (initialData as any).fuelTypes.map((ft: any) => ft.id);
      setSelectedFuelTypeIds(selected);
    }
  }, [initialData]);


  useEffect(() => {
    (async () => {
      const dict = await listFuelTypes();
      setFuelTypes(dict);
    })();
  }, []);

  const toggleFuel = (id: string) => {
    setSelectedFuelTypeIds(prev =>
      prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]
    );
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.name || !formData.model || !formData.brand || !formData.registration || !formData.year || !formData.kilometers) {
      alert("Wypełnij wszystkie pola, aby dodać pojazd");
      return;
    }

    onAddVehicle({
      ...formData,
      selectedFuelTypeIds,
    });
  };

  return(
    <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
      <div className="bg-white rounded-2xl shadow-xl p-8 w-full max-w-md relative">
        <h2 className="text-2xl font-bold text-gray-800 mb-6">
          {initialData ? "Edytuj pojazd" : "Dodaj Pojazd"}
        </h2>
        <button
          onClick={onClose}
          className="text-gray-400 hover:text-gray-600 transition absolute top-4 right-4">
          <X className="w-8 h-8" />
        </button>

        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <input
            type="text"
            placeholder="Nazwa własna pojazdu"
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value})}
            className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
            required
          />
          <input
            type="text"
            placeholder="Marka pojazdu"
            value={formData.brand}
            onChange={(e) => setFormData({ ...formData, brand: e.target.value})}
            className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
            required
          />
          <input
            type="text"
            placeholder="Model pojazdu"
            value={formData.model}
            onChange={(e) => setFormData({ ...formData, model: e.target.value})}
            className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
            required
          />
          <input
            type="text"
            placeholder="Numer rejestracyjny"
            value={formData.registration}
            onChange={(e) => setFormData({ ...formData, registration: e.target.value})}
            className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
            required
          />
          <input
            type="number"
            placeholder="Rok pojazdu"
            value={formData.year}
            onChange={(e) => setFormData({ ...formData, year: e.target.value})}
            className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
            required
          />
          <input
            type="number"
            placeholder="Przebieg"
            value={formData.kilometers}
            onChange={(e) => setFormData({ ...formData, kilometers: e.target.value})}
            className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
            required
          />

          <div className="mt-2">
            <p className="mb-2 font-semibold">Typy paliwa / energii:</p>
            <div className="flex flex-wrap gap-3">
              {fuelTypes.map(ft => (
                <label key={ft.id} className="inline-flex items-center gap-2">
                  <input
                    type="checkbox"
                    checked={selectedFuelTypeIds.includes(ft.id)}
                    onChange={() => toggleFuel(ft.id)}
                  />
                  <span>
                    {ft.name} <span className="text-gray-500 text-sm">({ft.defaultUnit})</span>
                  </span>
                </label>
              ))}
            </div>
          </div>

          <button 
            type="submit"
            className="text-lg font-semibold px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-500 transition">
              {initialData? "Zapisz zmiany" : "Zapisz"}
          </button>
        </form>
      </div>
    </div>
  );
}
