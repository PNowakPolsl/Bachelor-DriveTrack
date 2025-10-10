import { LayoutDashboard, Car, Wallet, BarChart3, Bell, User} from "lucide-react";

export default function Sidebar(){
    return(
        <aside className="w-64 min-h bg-white border-r border-gray-200 shadow-md">
            <div className="p-6 flex items-center gap-3">
                <div className="px-2 py-2 bg-gradient-to-r from-blue-600 to-sky-500 border-blue-100 shadow-md  rounded-xl">
                    <Car className="w-8 h-8 text-white " />
                </div>
                <h2 className="text-2xl font-bold bg-gradient-to-r from-blue-600 to-sky-500 bg-clip-text text-transparent">
                    DriveTrack
                </h2>
            </div>

            <nav className="mt-6 flex flex-col gap-2 px-4 text-gray-700 font-medium">
                <button className="flex items-center gap-3 px-4 py-2 rounded-lg hover:bg-blue-50 hover:text-blue-600 transition">
                    <LayoutDashboard className="w-5 h-5" />
                    <span>Panel sterowania</span>
                </button>

                <button className="flex items-center gap-3 px-4 py-2 rounded-lg hover:bg-blue-50 hover:text-blue-600 transition">
                    <Car className="w-5 h-5" />
                    <span>Pojazdy</span>
                </button>

                <button className="flex items-center gap-3 px-4 py-2 rounded-lg hover:bg-blue-50 hover:text-blue-600 transition">
                    <Wallet className="w-5 h-5" />
                    <span>Wydatki</span>
                </button>

                <button className="flex items-center gap-3 px-4 py-2 rounded-lg hover:bg-blue-50 hover:text-blue-600 transition">
                    <BarChart3 className="w-5 h-5" />
                    <span>Raporty</span>
                </button>

                <button className="flex items-center gap-3 px-4 py-2 rounded-lg hover:bg-blue-50 hover:text-blue-600 transition">
                    <Bell className="w-5 h-5" />
                    <span>Przypomnienia</span>
                </button>

                <button className="flex items-center gap-3 px-4 py-2 rounded-lg hover:bg-blue-50 hover:text-blue-600 transition">
                    <User className="w-5 h-5" />
                    <span>Profil</span>
                </button>
            </nav>
        </aside>


    )

}