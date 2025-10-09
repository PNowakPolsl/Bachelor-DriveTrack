import {Car, TrendingUp, PieChart, Bell} from "lucide-react";

export default function MainMenu() {
  return (
    <div className="min-h-screen flex flex-col">
      <section className="bg-white">
      {/* Logo i nazwa aplikacji */ }
      <div className="w-full flex flex-col items-center py-20">
        <div className="flex items-center gap-3 bg-gradient-to-r from-blue-50 to-gray-100 px-6 py-3 rounded-3xl shadow-md border border-blue-100 hover:shadow-xl hover:scale-105 transition duration-300">
                  <Car className="w-14 h-14 text-blue-500" />
                  <h1 className="text-5xl font-bold bg-gradient-to-r from-blue-600 to-sky-500 bg-clip-text text-transparent">
                    DriveTrack
                  </h1>
        </div>
        {/* Hasło reklamowe i buttony */}
        <p className="mt-8 text-5xl text-center text-gray-800 font-bold">
          ZARZĄDZAJ WYDATKAMI SWOJEGO SAMOCHODU
          <span className="mt-2 block text-blue-500"> Z ŁATWOŚCIĄ</span>
        </p>
        <p className="mt-3 text-xl text-gray-500 text-center py-2 max-w-4xl">
          Prosty sposób na rejestrowanie kosztów paliwa, napraw i ubezpieczenia. Kontroluj wydatki na pojazd dzięki intuicyjnemu śledzeniu i wnikliwym analizom
        </p>
        <div className="mt-8 flex flex-row gap-6 justify-center">
          <button className="px-8 py-3 bg-blue-600 text-white text-xl font-semibold rounded-xl shadow-md hover:bg-blue-500 transition duration-300">
            Rejestracja
          </button>
          <button className="px-8 py-3 bg-white text-gray-800 text-xl font-semibold rounded-xl shadow-md hover:bg-blue-600 hover:text-white transition duration-300">
            Logowanie
          </button>
        </div>
      </div>
      </section>

      <section className=" w-full bg-gray-100 py-20">
        <div className="max-w-6xl mx-auto text-center">
          <h2 className="text-4xl font-bold text-gray-800 mb-4">
            Wszystko co musisz wiedzieć
          </h2>
          <p className="text-gray-500 font-medium text-lg mb-12">
            Funkcje, które pomogą Ci efektywnie zarządzać wydatkami na pojazd
          </p>
          { /*Kafelek 1 */ }
            <div className="grid grid-cols-1 md:grid-cols-3 gap-10 px-4">
              <div className="bg-white rounded-2xl shadow-md p-8 border border-gray hover:shadow-2xl hover:scale-105 transition duration-300"> 
                <div className="bg-blue-100 rounded-lg w-14 h-14 flex items-center justify-center mb-1 shadow-md">
                  <TrendingUp className="w-10 h-10 text-blue-500" />
                </div>
                <h3 className="mt-4 text-2xl font-semibold text-gray-800 text-left mb-2">
                  Monitorowanie kosztów
                </h3>
                <p className="text-gray-600 text-lg text-left">
                  Monitoruj wszystkie wydatki związane z pojazdem w jednym miejscu
                </p>
              </div>

          { /*Kafelek 2 */ }
              <div className="bg-white rounded-2xl shadow-md p-8 border border-gray hover:shadow-2xl hover:scale-105 transition duration-300"> 
                <div className="bg-blue-100 rounded-lg w-14 h-14 flex items-center justify-center mb-1 shadow-md">
                  <PieChart className="w-10 h-10 text-blue-500" />
                </div>
                <h3 className="mt-4 text-2xl font-semibold text-gray-800 text-left mb-2">
                  Wizualizacja raportów
                </h3>
                <p className="text-gray-600 text-lg text-left">
                  Uzyskaj wgląd dzięki wygenerowanym wykresom i analizom
                </p>
              </div>

              { /*Kafelek 3 */ }
              <div className="bg-white rounded-2xl shadow-md p-8 border border-gray hover:shadow-2xl hover:scale-105 transition duration-300"> 
                <div className="bg-blue-100 rounded-lg w-14 h-14 flex items-center justify-center mb-1 shadow-md">
                  <Bell className="w-10 h-10 text-blue-500" />
                </div>
                <h3 className="mt-4 text-2xl font-semibold text-gray-800 text-left mb-2">
                  Inteligentne przypomnienia
                </h3>
                <p className="text-gray-600 text-lg text-left">
                  Nigdy nie przegap ważnych przeglądów i opłat
                </p>
              </div>
            </div>
        </div>
      </section>
    </div>
  );
}
