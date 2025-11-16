import Sidebar from "../components/Sidebar";
import Topbar from "../components/Topbar";
import { Wallet, Fuel, Car } from "lucide-react";
import MonthlyExpensesChart from "../components/MonthlyExpensesChart";

export default function Dashboard() {

  return (
    <div className="min-h-screen bg-gray-50 flex">
      <Sidebar />
      <div className="flex flex-col flex-1">
        <Topbar />

        <main className="p-6">
          <h1 className="text-3xl font-bold text-gray-800">
            Panel sterowania
          </h1>
          <p className="text-gray-500 text-lg mt-2">
            Podsumowanie twoich wydatków
          </p>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mt-8">

            <div className="bg-white rounded-2xl shadow-md p-6 relative">
              <div className="absolute top-6 right-6 text-blue-500">
                <Wallet className="w-6 h-6" />
              </div>
              <h3 className="text-gray-600 font-medium">
                Łączne wydatkiw tym miesiącu
              </h3>
              <p className="text-3xl font-bold text-gray-800 mt-2">
                2 300 zł
              </p>
            </div>

            <div className="bg-white rounded-2xl shadow-md p-6 relative">
              <div className="absolute top-6 right-6 text-blue-500">
                <Fuel className="w-6 h-6" />
              </div>
              <h3 className="text-gray-600 font-medium">
                Średnie spalanie
              </h3>
              <p className="text-3xl font-bold text-gray-800 mt-2">
                7.4/100km
              </p>
            </div>

            <div className="bg-white rounded-2xl shadow-md p-6 relative">
              <div className="absolute top-6 right-6 text-blue-500">
                <Car className="w-6 h-6" />
              </div>
              <h3 className="text-gray-600 font-medium">
                Aktywne pojazdy
              </h3>
              <p className="text-3xl font-bold text-gray-800 mt-2">
                2
              </p>
            </div>
          </div>

           {/* <MonthlyExpensesChart /> */ }
        

        </main>
      </div>
    </div>
  );
}
