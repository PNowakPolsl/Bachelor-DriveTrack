import Sidebar from "../components/Sidebar";
import Topbar from "../components/Topbar";
import { useState } from "react";

export default function Profile(){

    const [formData, setFormData] = useState({
        currentPassword: "",
        newPassword: "",
        confirmPassword: "",

    });

    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();

        if(formData.newPassword !== formData.confirmPassword){
            setError("Nowe hasło i potwierdzenie muszą być takie same");
            setTimeout(() => setError(""), 4000);
            return;
        }

        if(formData.newPassword === formData.currentPassword){
            setError("Stare i nowe hasło nie mogą być takie same");
            setTimeout(() => setError(""), 4000);
            return;
        }

        setSuccess("Hasło zostało pomyślnie zmienione")
        setFormData({currentPassword: "", newPassword: "", confirmPassword: ""});

        setTimeout(() => setSuccess(""), 4000);

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
                                Profil
                            </h1>
                            <p className="text-gray-500 text-lg mt-2">
                                Edytuj swój profil
                            </p>
                        </div>
                    </div>
                    <div className="bg-white rounded-2xl shadow-md p-8 mt-10 max-w-2xl">
                        <h2 className="text-2xl font-bold text-gray-800 mb-2">
                            Zmiana hasła
                        </h2>
                        <p className="text-gray-500 mb-6">
                            Zmień swoje hasło, aby zapewnić bezpieczeństwo swoim danym
                        </p>

                        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                            <input
                                type="password"
                                placeholder="Obecne hasło"
                                value={formData.currentPassword}
                                onChange={(e) => setFormData({ ...formData, currentPassword: e.target.value})}
                                className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                                required
                            />

                            <input
                                type="password"
                                placeholder="Nowe hasło"
                                value={formData.newPassword}
                                onChange={(e) => setFormData({ ...formData, newPassword: e.target.value})}
                                className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                                required
                            />
                            <input
                                type="password"
                                placeholder="Potwierdź nowe hasło"
                                value={formData.confirmPassword}
                                onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value})}
                                className="border border-gray-300 rounded-lg p-3 focus:ring-2 focus:ring-blue-500 outline-none"
                                required
                            />

                            <button
                                type="submit"
                                className="mt-2 bg-blue-600 text-white font-semibold text-lg py-3 rounded-lg hover:bg-blue-500 transition"
                            >
                                Aktualizuj hasło
                            </button>
                            {error && (
                                <p className="text-red-600 text-center font-medium -mt-2 mb-2">
                                    {error}
                                </p>
                            )}
                            {success && (
                                <p className="text-green-600 text-center font-medium mt-3">
                                    {success}
                                </p>
                            )}
                        </form>
                    </div>
                </main>
            </div>
        </div>
    );
};