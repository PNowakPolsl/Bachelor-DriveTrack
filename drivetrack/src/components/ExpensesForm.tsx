import { useState } from "react";
import { X } from "lucide-react";


interface ExpensesFormPops{
    onClose: () => void;
    onAddExpense: (expenseData: any) => void;
    vehicles: any[];
    initialData?: any | null;
}

export default function ExpensesForm({ onClose, onAddExpense, vehicles, initialData }: ExpensesFormPops){

    const [formData, setFormData] = useState(
        initialData || {
        date: "",
        category: "Paliwo",
        description: "",
        vehicle: vehicles.length > 0 ? vehicles[0].brand : "",
        cost: "",
    });

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();

        if(!formData.date || !formData.category || !formData.description || !formData.vehicle || !formData.cost){
            alert("Wypełnij wszystkie pola, aby dodać wydatek");
            return;
        }
        
        onAddExpense(formData);
    }
    
    return(
        <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
            <div className="bg-white rounded-2xl shadow-xl p-8 w-full max-w-md relative">
                <h2 className="text-2xl font-bold text-gray-800 mb-6">
                    {initialData ? "Edytuj wydatek" : "Dodaj wydatek"}
                </h2>
                <button
                    onClick={onClose}
                    className="text-gray-400 hover:text-gray-600 transition absolute top-4 right-4">
                        <X className="w-8 h-8" />
                </button>
                <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                    <input
                        type="date"
                        placeholder="Data wydatku"
                        value={formData.date}
                        onChange={(e) => setFormData({ ...formData, date: e.target.value})}
                        className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                        required
                    />
                    <select
                        value={formData.category}
                        onChange={(e) => setFormData({ ...formData, category: e.target.value})}
                        className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                        required
                    >
                            <option value="Paliwo">Paliwo</option>
                            <option value="Mechanik">Mechanik</option>
                            <option value="Przegląd">Przegląd</option>
                            <option value="Ubezpieczenie">Ubezpieczenie</option>
                            <option value="Inne">Inne</option>
                    </select>
                    <input
                        type="text"
                        placeholder="Opis"
                        value={formData.description}
                        onChange={(e) => setFormData({ ...formData, description: e.target.value})}
                        className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                        required
                    />
                    <select
                        value={formData.vehicle}
                        onChange={(e) => setFormData({ ...formData, vehicle: e.target.value})}
                        className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                        required
                    >
                            {vehicles.map((v, i) => (
                                <option key={i} value={v.brand}>{v.brand}</option>
                            ))}
                    </select>
                    <input
                        type="number"
                        placeholder="Koszt"
                        value={formData.cost}
                        onChange={(e) => setFormData({ ...formData, cost: e.target.value})}
                        className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                        required
                    />
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