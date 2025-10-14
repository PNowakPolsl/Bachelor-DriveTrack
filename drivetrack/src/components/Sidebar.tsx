import { LayoutDashboard, Car, Wallet, BarChart3, Bell, User, LogOut } from "lucide-react";
import { useNavigate, useLocation } from "react-router-dom";

export default function Sidebar(){
    const navigate = useNavigate();
    const location = useLocation();

    return(
        <aside className="w-64 min-h-screen bg-white border-r border-gray-200 shadow-md flex flex-col justify-between">
            <div>
                <div className="p-6 flex items-center gap-3 border-b border-gray-100">
                    <div className="px-3 py-3 bg-gradient-to-r from-blue-600 to-sky-500 border-blue-100 shadow-md  rounded-xl">
                        <Car className="w-8 h-8 text-white " />
                    </div>
                    <h2 className="text-3xl font-bold bg-gradient-to-r from-blue-600 to-sky-500 bg-clip-text text-transparent">
                        DriveTrack
                    </h2>
                </div>

                <nav className="mt-4 text-lg flex flex-col gap-2 px-4 text-gray-700 font-semibold">
                    <button 
                        onClick={() => navigate("/dashboard")}
                        className={"flex items-center gap-4 px-4 py-3 rounded-lg transition " +
                            (location.pathname === "/dashboard"
                                ? "bg-blue-600 text-white"
                                : "hover:bg-blue-50 hover:text-blue-600"
                            )
                        }
                    >
                        <LayoutDashboard className="w-6 h-6" />
                        <span>Panel sterowania</span>
                    </button>

                    <button 
                        onClick={() => navigate("/vehicles")}
                        className={"flex items-center gap-4 px-4 py-3 rounded-lg transition " +
                            (location.pathname === "/vehicles"
                                ? "bg-blue-600 text-white"
                                : "hover:bg-blue-50 hover:text-blue-600"
                            )
                        }
                    >
                        <Car className="w-6 h-6" />
                        <span>Pojazdy</span>
                    </button>

                    <button className="flex items-center gap-4 px-4 py-3 rounded-lg hover:bg-blue-50 hover:text-blue-600 transition">
                        <Wallet className="w-6 h-6" />
                        <span>Wydatki</span>
                    </button>

                    <button className="flex items-center gap-4 px-4 py-3 rounded-lg hover:bg-blue-50 hover:text-blue-600 transition">
                        <BarChart3 className="w-6 h-6" />
                        <span>Raporty</span>
                    </button>

                    <button className="flex items-center gap-4 px-4 py-3 rounded-lg hover:bg-blue-50 hover:text-blue-600 transition">
                        <Bell className="w-6 h-6" />
                        <span>Przypomnienia</span>
                    </button>

                    <button className="flex items-center gap-4 px-4 py-3 rounded-lg hover:bg-blue-50 hover:text-blue-600 transition">
                        <User className="w-6 h-6" />
                        <span>Profil</span>
                    </button>
                </nav>
            </div>
            
            <div className="px-4 py-6 border-t border-gray-100">
                <button
                    onClick={() => navigate("/")}
                    className="flex items-center gap-3 px-4 py-2 w-full rounded-lg font-medium text-gray-700 hover:bg-red-50 hover:text-red-600 transition"
                >
                    <LogOut className="w-5 h-5" />
                    <span>Wyloguj się</span>    
                </button>
            </div>
        </aside>

    )
}