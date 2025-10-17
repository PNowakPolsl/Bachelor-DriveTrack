import Sidebar from "../components/Sidebar";
import Topbar from "../components/Topbar";
import { useState } from "react";
import ExpensesForm from "../components/ExpensesForm";

export default function Expenses(){

    const [isFormOpen, setIsFormOpen] = useState(false);
    const [expenses, setExpenses] = useState<any[]>([
        {
            date: "2025-10-17",
            category: "Paliwo",
            description: "Tankowanie na Orlenie",
            vehicle: "Volkswagen Golf 6",
            cost: "150",
        }
    ]);

    const vehicles = [
        { brand: "Volkswagen Golf 6"},
        { brand: "Skoda Fabia 4"},
    ];

    const handleAddExpense = (expenseData: any) => {
        setExpenses((prev) => [...prev, expenseData]);
        setIsFormOpen(false);
    };

    const [expenseToEdit, setExpenseToEdit] = useState<number | null>(null);

    const handleEditExpense = (index: number) => {
        setExpenseToEdit(index);
        setIsFormOpen(true);
    };

    const handleUpdateExpense = (updatedExpense: any) => {
        if(expenseToEdit === null) return;

        setExpenses((prev) => prev.map((v, i) => (i === expenseToEdit ? updatedExpense : v)));

        setExpenseToEdit(null);
        setIsFormOpen(false);
    };

    const handledDeleteExpense = (index: number) => {
        setExpenses(prev => prev.filter((_, i) => i !== index));
    };

    const categoryColors = (category: string) => {
        switch(category){
            case "Paliwo":
                return "bg-blue-50 text-blue-500 hover:bg-blue-200 transition";
            case "Mechanik":
                return "bg-purple-50 text-purple-500 hover:bg-purple-200 transition"
            case "Przegląd":
                return "bg-orange-50 text-orange-500 hover:bg-orange-200 transition"
            case "Ubezpieczenie":
                return "bg-green-50 text-green-500 hover:bg-green-200 transition"
            case "Inne":
                return "bg-yellow-50 text-yellow-500 hover:bg-yellow-200 transition"
            default:
                return "bg-gray-100 text-gray-800"
        }
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
                                Wydatki
                            </h1>
                            <p className="text-gray-500 text-lg mt-2">
                                Zarządzaj swoimi wydatkami
                            </p>
                        </div>
                        <button
                            onClick={() => {
                                setIsFormOpen(true)
                                setExpenseToEdit(null);
                            }}
                            className="bg-blue-600 text-white font-medium px-4 py-2 rounded-lg shadow-md hover:bg-blue-500 transition">
                            + Dodaj wydatek
                        </button>
                    </div>

                    {isFormOpen && 
                        <ExpensesForm 
                            onClose={() => {
                                setIsFormOpen(false);
                                setExpenseToEdit(null);
                            }}
                            onAddExpense={expenseToEdit === null ? handleAddExpense : handleUpdateExpense}
                            initialData={expenseToEdit !== null ? expenses[expenseToEdit] : null}
                            vehicles={vehicles}
                        />
                    }

                    <div className="w-full mx-auto mt-6 p-4 border border-gray-300 rounded-2xl bg-white">
                        <h2 className="text-xl font-bold text-gray-800">
                            Wszystkie wydatki
                        </h2>

                        <div className="mt-5 overflow-x-auto rounded-xl border border-gray-300">
                            <table className="w-full border-collapse">
                                <thead className="bg-gray-200">
                                    <tr>
                                        <th className="text-left p-3">Data</th>
                                        <th className="text-left p-3">Kategoria</th>
                                        <th className="text-left p-3">Opis</th>
                                        <th className="text-left p-3">Pojazd</th>
                                        <th className="text-right p-3">Koszt</th>
                                        <th className="text-center p-3">Akcja</th>
                                    </tr>
                                </thead>

                                <tbody>
                                    {expenses.map((expense, index) => (
                                        <tr
                                            key={index}
                                            className="border-t border-gray-300 hover:bg-gray-50 transition"
                                        >
                                            <td className="p-3">{expense.date}</td>
                                            <td className="p-3">
                                                <span className={`px-2 py-1 rounded-full font-semibold text-sm ${categoryColors(expense.category)}`}>
                                                    {expense.category}
                                                </span>
                                                </td>
                                            <td className="p-3">{expense.description}</td>
                                            <td className="p-3 text-gray-700">{expense.vehicle}</td>
                                            <td className="p-3 text-right font-semibold">{expense.cost} zł</td>
                                            <td className="p-3 flex justify-center gap-2">
                                                <button 
                                                    onClick={() => handleEditExpense(index)}
                                                    className="bg-blue-500 text-white px-2 py-1 rounded-lg hover:bg-blue-400 transition">
                                                        Edytuj
                                                </button>
                                                <button 
                                                    onClick={() => handledDeleteExpense(index)}
                                                    className="bg-red-500 text-white px-2 py-1 rounded-lg hover:bg-red-400 transition">
                                                        Usuń
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </div>


                </main>
            </div>
        </div>

    );

}