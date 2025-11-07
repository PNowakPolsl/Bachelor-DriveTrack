import { useState } from "react";
import { X } from "lucide-react";


interface VehicleFormPops{
    onClose: () => void;
    onAddVehicle: (vehicleData: any) => void;
    initialData?: any | null;
}

export default function VehicleForm({ onClose, onAddVehicle, initialData }: VehicleFormPops){

    const [formData, setFormData] = useState(
        initialData || {
        name: "",
        brand: "",
        model: "",
        registration: "",
        year: "",
        kilometers: "",
        fuelType: "benzyna",
    });

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();

        if(!formData.name || !formData.model || !formData.brand || !formData.registration || !formData.year || !formData.kilometers){
            alert("Wypełnij wszystkie pola, aby dodać pojazd");
            return;
        }
        
        onAddVehicle(formData);
    }
    
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
                    <select
                        value={formData.fuelType}
                        onChange={(e) => setFormData({ ...formData, fuelType: e.target.value})}
                        className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                        required
                    >
                            <option value="benzyna">Benzyna</option>
                            <option value="diesel">Diesel</option>
                            <option value="benzyna+LPG">Benzyna + LPG</option>
                            <option value="elektryk">Elektryk</option>
                            <option value="hybryda">Hybryda</option>
                    </select>

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