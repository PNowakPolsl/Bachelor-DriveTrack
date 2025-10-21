import Sidebar from "../components/Sidebar";
import Topbar from "../components/Topbar";
import MonthlyExpensesChart from "../components/MonthlyExpensesChart";
import ExpensesCategoryChart from "../components/ExpensesCategoryChart";
import FuelConsumption from "../components/FuelConsumptionChart";

export default function Reports(){

     return(
        <div className="min-h-screen bg-gray-50 flex">
            <Sidebar />
            <div className="flex flex-col flex-1">
                <Topbar />
                <main className="p-6">
                    <div className="flex items-center justify-between">
                        <div>
                            <h1 className="text-3xl font-bold text-gray-800">
                                Raporty
                            </h1>
                            <p className="text-gray-500 text-lg mt-2">
                                Zapoznaj się z analizą swoich wydatków
                            </p>
                        </div>
                    </div>
                    <MonthlyExpensesChart />
                    <div className="mt-10 grid grid-cols-1 lg:grid-cols-2 gap-8 w-full">
                        <div className="w-full">
                            <ExpensesCategoryChart />
                        </div>
                        <div className="w-full">
                            <FuelConsumption />
                        </div>
                    </div>
                </main>
            </div>
        </div>
     )
}