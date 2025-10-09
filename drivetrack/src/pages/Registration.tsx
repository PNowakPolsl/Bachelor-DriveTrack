import { Car } from "lucide-react";

export default function Registration() {
  return (
    <div className="min-h-screen flex flex-col items-center py-24 bg-gray-50">
        { /* Logo + Nagłówek */ }
        <div className="w-full flex flex-col items-center py-4">
            <div className="flex items-center gap-3 bg-gradient-to-r from-blue-600 to-sky-500 px-6 py-3 rounded-3xl shadow-md border border-blue-100">
                    <Car className="w-10 h-10 text-white" />
                    <h1 className="text-3xl font-bold bg-white from-blue-600 to-sky-500 bg-clip-text text-transparent">
                        DriveTrack
                    </h1>
            </div>
        </div>
        <p className="text-3xl text-center text-gray-800 font-bold">
          Utwórz konto
          <span className="mt-2 block text-gray-600 text-base font-normal"> Zacznij kontrolować wydatki już dziś</span>
        </p>
        { /* Pole rejestracji */ }
        <div className="mt-10 w-full max-w-md bg-white rounded-2xl shadow-lg p-8 border border-gray">
            <h3 className="text-2xl font-semibold text-gray-800 text-left">
                  Rejestracja
            </h3>
            <p className="text-gray-600 text-md text-left">
                  Utwórz nowe konto aby rozpocząć
            </p>
            <form className="flex flex-col gap-4">
                <div className="flex flex-col gap-2 py-6">
                    <h2 className="text-md font-semibold text-gray-800 text-left">
                        Imię i nazwisko
                    </h2>
                    <input
                        type="text"
                        placeholder="Jan Kowalski"
                        className="px-4 py-3 bg-gray-50 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
        
                    <h2 className="text-md font-semibold text-gray-800 text-left mt-2">
                        Email
                    </h2>
                    <input
                        type="text"
                        placeholder="jankowalski@email.com"
                        className="px-4 py-3 bg-gray-50 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />

                    <h2 className="text-md font-semibold text-gray-800 text-left mt-2">
                        Hasło
                    </h2>
                    <input
                        type="password"
                        placeholder="••••••••"
                        className="px-4 py-3 bg-gray-50 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />

                    <h2 className="text-md font-semibold text-gray-800 text-left mt-2">
                        Powtórz hasło
                    </h2>
                    <input
                        type="password"
                        placeholder="••••••••"
                        className="px-4 py-3 bg-gray-50 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                </div>
                <button className="px-6 py-3 bg-blue-600 text-white text-xl font-semibold rounded-xl shadow-md hover:bg-blue-500 transition duration-300">
                    Utwórz konto
                </button>
            </form>
        </div>



    </div>
  );
}
