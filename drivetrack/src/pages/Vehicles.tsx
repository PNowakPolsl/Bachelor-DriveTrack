import Sidebar from "../components/Sidebar";
import Topbar from "../components/Topbar";
import { useState } from "react";
import { X } from "lucide-react";

export default function Vehicles(){
    const [isFormOpen, setIsFormOpen] = useState(false);

    const [formData, setFormData] = useState({
        brand: "",
        registration: "",
        year: "",
        kilometers: "",
        fuelType: "benzyna",
    });

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
                            onClick={() => setIsFormOpen(true)}
                            className="bg-blue-600 text-white font-medium px-4 py-2 rounded-lg shadow-md hover:bg-blue-500 transition">
                            + Dodaj pojazd
                        </button>
                    </div>

                    {isFormOpen && (
                        <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
                            <div className="bg-white rounded-2xl shadow-xl p-8 w-full max-w-md relative">
                                <h2 className="text-2xl font-bold text-gray-800 mb-6">
                                    Dodaj Pojazd
                                </h2>
                                <button
                                    onClick={() => setIsFormOpen(false)}
                                    className="text-gray-400 hover:text-gray-600 transition absolute top-4 right-4">
                                        <X className="w-8 h-8"></X>
                                </button>
                                <form className="flex flex-col gap-4">
                                    <input
                                        type="text"
                                        placeholder="Marka pojazdu"
                                        value={formData.brand}
                                        onChange={(e) => setFormData({ ...formData, brand: e.target.value})}
                                        className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                                    />
                                    <input
                                        type="text"
                                        placeholder="Numer rejestracyjny"
                                        value={formData.registration}
                                        onChange={(e) => setFormData({ ...formData, registration: e.target.value})}
                                        className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                                    />
                                    <input
                                        type="number"
                                        placeholder="Rok pojazdu"
                                        value={formData.year}
                                        onChange={(e) => setFormData({ ...formData, year: e.target.value})}
                                        className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                                    />
                                    <input
                                        type="number"
                                        placeholder="Przebieg"
                                        value={formData.kilometers}
                                        onChange={(e) => setFormData({ ...formData, kilometers: e.target.value})}
                                        className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                                    />
                                    <select
                                        value={formData.fuelType}
                                        onChange={(e) => setFormData({ ...formData, fuelType: e.target.value})}
                                        className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none">
                                            <option value="benzyna">Benzyna</option>
                                            <option value="diesel">Diesel</option>
                                            <option value="benzyna+lpg">Benzyna + LPG</option>
                                            <option value="elektryk">Elektryk</option>
                                    </select>

                                    <button 
                                        type="submit"
                                        className="text-lg font-semibold px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-500 transition">
                                            Zapisz
                                    </button>
                                </form>
                            </div>
                        </div>
                    )}
                </main>


            </div>
        </div>


    );

}