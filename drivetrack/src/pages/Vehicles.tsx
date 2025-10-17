import Sidebar from "../components/Sidebar";
import Topbar from "../components/Topbar";
import { useState } from "react";
import VehicleForm from "../components/VehicleForm";
import { Fuel, Pencil, Trash2 } from "lucide-react";


export default function Vehicles(){
    const [isFormOpen, setIsFormOpen] = useState(false);

    const [vehicles, setVehicles] = useState<any[]>([
        {
            brand: "Volkswagen Golf 6",
            registration: "SW 123987",
            year: "2008",
            kilometers: "250497",
            fuelType: "Benzyna + LPG",
        }
    ]); //zmienic podejscie jak bedzie dodawany back

    const handleAddVehicle = (vehicleData: any) => {
        setVehicles((prev) => [...prev, vehicleData]);
        setIsFormOpen(false);
    };

    const [vehicleToDelete, setVehicleToDelete] = useState<number | null>(null);

    const handledDeleteVehicle = (index: number) => {
        setVehicleToDelete(index);
    };

    const confirmDelete= () => {
        if(vehicleToDelete === null) return;
        setVehicles((prev) => prev.filter((_, i) => i !== vehicleToDelete));
        setVehicleToDelete(null);
    };

    const cancelDelete = () => setVehicleToDelete(null);

    const [vehicleToEdit, setVehicleToEdit] = useState<number | null>(null);

    const handleEditVehicle = (index: number) => {
        setVehicleToEdit(index);
        setIsFormOpen(true);
    };

    const handleUpdateVehicle = (updatedVehicle: any) => {
        if(vehicleToEdit === null) return;

        setVehicles((prev) => prev.map((v, i) => (i === vehicleToEdit ? updatedVehicle : v)));

        setVehicleToEdit(null);
        setIsFormOpen(false);
    };

    return(
        <div className="min-h-screen bg-gray-50 flex">
            <Sidebar />
            <div className="flex flex-col flex-1">
                <Topbar />
                <main className="p-6">
                    <div className="flex items-center justify-between">
                        <div>
                            <h1 className="text-3xl font-bold text-gray-800">
                                Pojazdy
                            </h1>
                            <p className="text-gray-500 text-lg mt-2">
                                Zarządzaj swoimi pojazdami
                            </p>
                        </div>
                        <button
                            onClick={() => {
                                setIsFormOpen(true)
                                setVehicleToEdit(null);
                            }}
                            className="bg-blue-600 text-white font-medium px-4 py-2 rounded-lg shadow-md hover:bg-blue-500 transition">
                            + Dodaj pojazd
                        </button>
                    </div>

                {isFormOpen && 
                    <VehicleForm 
                        onClose={() => {
                            setIsFormOpen(false);
                            setVehicleToEdit(null);
                        }}
                        onAddVehicle={vehicleToEdit === null ? handleAddVehicle : handleUpdateVehicle}
                        initialData={vehicleToEdit !== null ? vehicles[vehicleToEdit] : null}
                />}

                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mt-8">
                    {vehicles.map((vehicle, index) => (
                        <div
                            key={index}
                            className="bg-white rounded-2xl shadow-md p-6 border border-gray-100 relative"
                        >
                            <div className="flex justify-between items-start mb-4">
                                <div>
                                    <h3 className="text-xl font-bold text-gray-800 capitalize">{vehicle.brand}</h3>
                                    <p className="text-gray-500 text-sm">{vehicle.year}</p>
                                </div>

                                <div className="flex items-center gap-2 text-sm font-medium border bg-gray-200 border-gray-300 rounded-full px-3 py-1 text-gray-700 capitalize">
                                    <Fuel className="w-4 h-4 text-gray-700"/> 
                                    <span className="text-gray-700 font-bold capitalize text-sm">
                                        {vehicle.fuelType}
                                    </span>
                                </div>
                            </div>

                            <div className="mt-4 border-t border-gray-100 pt-4">
                                <div className="flex justify-between text-gray-800 font-bold">
                                    <span className="font-medium text-gray-500">Numer rejestracyjny</span>
                                    <span className="uppercase">{vehicle.registration}</span>
                                </div>
                                <div className="flex justify-between text-gray-800 font-bold">
                                    <span className="font-medium text-gray-500">Przebieg</span>
                                    <span>{vehicle.kilometers} km</span>
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
                                <h2 className="text-xl font-bold text-gray-800 mb-2">
                                    Usuń pojazd
                                </h2>
                                <p className="text-gray-600 mb-6">
                                    Czy na pewno chcesz usunąć pojazd?
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