import Sidebar from "../components/Sidebar";
import Topbar from "../components/Topbar";
import { useState } from "react";
import ReminderForm from "../components/ReminderForm";
import { CalendarDays, Pencil, Trash2 } from "lucide-react";

export default function Reminders(){

    const [isFormOpen, setIsFormOpen] = useState(false);

    const [reminders, setReminders] = useState<any[]>([
        {
            title: "Przegląd",
            vehicle: "Volkswagen Golf 6",
            description: "Stacja diagnostyczna Chorzów",
            date: "2025-10-25"
        }
    ]); //zmienic podejscie jak bedzie dodawany back

    const [reminderToDelete, setReminderToDelete] = useState<number | null>(null);

    const [reminderToEdit, setReminderToEdit] = useState<number | null>(null);

    const vehicles = [
        { brand: "Volkswagen Golf 6"},
        { brand: "Skoda Fabia 4"},
    ];

    const handleAddReminder = (reminderData: any) => {
        setReminders((prev) => [...prev, reminderData]);
        setIsFormOpen(false);
    };

    const handleUpdateReminder = (updatedReminder: any) => {
        if(reminderToEdit === null) return;

        setReminders((prev) => prev.map((v, i) => (i === reminderToEdit ? updatedReminder : v)));

        setReminderToEdit(null);
        setIsFormOpen(false);
    };

    const handleEditReminder = (index: number) => {
        setReminderToEdit(index);
        setIsFormOpen(true);
    };
    

    const handledDeleteReminder = (index: number) => {
        setReminderToDelete(index);
    };

    const confirmDelete= () => {
        if(reminderToDelete === null) return;
        setReminders((prev) => prev.filter((_, i) => i !== reminderToDelete));
        setReminderToDelete(null);
    };

    const cancelDelete = () => setReminderToDelete(null);


    return(
        <div className="min-h-screen bg-gray-50 flex">
            <Sidebar />
            <div className="flex flex-col flex-1">
                <Topbar />
                <main className="p-6">
                    <div className="flex items-center justify-between">
                        <div>
                            <h1 className="text-3xl font-bold text-gray-800">
                                Przypomnienia
                            </h1>
                            <p className="text-gray-500 text-lg mt-2">
                                Pamiętaj o najważniejszych wydarzeniach związanych z twoim pojazdem 
                            </p>
                        </div>
                        <button
                            onClick={() => {
                                setIsFormOpen(true)
                                setReminderToEdit(null);
                            }}
                            className="bg-blue-600 text-white font-medium px-4 py-2 rounded-lg shadow-md hover:bg-blue-500 transition">
                            + Dodaj przypomnienie
                        </button>
                    </div>
                        
                    {isFormOpen && 
                        <ReminderForm 
                            onClose={() => {
                                setIsFormOpen(false);
                                setReminderToEdit(null);
                            }}
                            onAddReminder={reminderToEdit === null ? handleAddReminder : handleUpdateReminder}
                            initialData={reminderToEdit !== null ? reminders[reminderToEdit] : null}
                            vehicles={vehicles}
                        />}

                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mt-8">
                    {reminders.map((reminder, index) => (
                        <div
                            key={index}
                            className="bg-white rounded-2xl shadow-md p-6 border border-gray-100 relative"
                        >
                            <div className="absolute top-4 right-4 text-sm font-bold">
                                {(() => {
                                    const today = new Date();
                                    const reminderDate = new Date(reminder.date);
                                    const diffTime = reminderDate.getTime() - today.getTime();
                                    const daysLeft = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
                                    let color = "text-green-500 bg-green-50 rounded-full px-3 py-1 hover:bg-green-200 transition";
                                    if (daysLeft <= 2) color = "text-red-600 bg-red-50 rounded-full px-3 py-1 hover:bg-red-200 transition";
                                    else if (daysLeft <= 7) color = "text-yellow-500 bg-orange-50 rounded-full px-3 py-1 hover:bg-yellow-200 transition";
                                    return <span className={color}>{daysLeft} dni</span>;
                                })()}
                            </div>
                            <div className="mb-4">
                                <h3 className="text-xl font-bold text-gray-800 capitalize">{reminder.title}</h3>
                                <p className="text-gray-500 text-sm">{reminder.vehicle}</p>
                            </div>                              
                            

                            <p className="text-gray-700 mb-4">{reminder.description}</p>
                            <div className="flex items-center gap-2 text-gray-500 text-base mt-2">
                                <CalendarDays className="w-4 h-4 text-blue-500" />
                                <span>{reminder.date}</span>
                            </div>
                            <div className="flex mt-4 gap-2">
                                <button
                                    onClick={() => handleEditReminder(index)}
                                    className="flex-1 flex items-center justify-center gap-2 bg-gray-100 text-gray-800 font-semibold border border-gray-200 shadow-md px-4 py-2 rounded-lg hover:bg-blue-500 hover:text-white transition"
                                >
                                    <Pencil className="w-4 h-4" />
                                    Edytuj
                                </button>
                                <button
                                    onClick={() => handledDeleteReminder(index)}
                                    className="flex items-center justify-center bg-red-600 text-white p-2 rounded-lg hover:bg-red-500 transition"
                                >
                                    <Trash2 className="w-4 h-4" />
                                </button>
                            </div>
                        </div>
                    ))}
                </div>
                    
                    {reminderToDelete !== null && (
                        <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
                            <div className="bg-white p-6 rounded-2xl shadow-xl w-full max-w-sm text-center">
                                <h2 className="text-xl font-bold text-gray-800 mb-2">
                                    Usuń przypomnienie
                                </h2>
                                <p className="text-gray-600 mb-6">
                                    Czy na pewno chcesz usunąć przypomnienie?
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


};