import { Car } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useState } from "react";

export default function Login() {
  const navigate = useNavigate();

  // 🔹 Dane formularza i stan błędu
  const [formData, setFormData] = useState({
    email: "",
    password: "",
  });

  const [error, setError] = useState("");

  // 🔹 Obsługa zmiany danych w inputach
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  // 🔹 Obsługa kliknięcia przycisku „Zaloguj”
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.email || !formData.password) {
      setError("Wszystkie pola są wymagane.");
      return;
    }

    // Tu możesz dodać backendowe logowanie (np. API)
    setError("");
    navigate("/"); // po zalogowaniu przekierowanie np. na główną stronę
  };

  return (
    <div className="min-h-screen flex flex-col items-center py-24 bg-gray-50">
      {/* Logo + Nagłówek */}
      <div className="w-full flex flex-col items-center py-4">
        <button
          onClick={() => navigate("/")}
          className="flex items-center gap-3 bg-gradient-to-r from-blue-600 to-sky-500 px-6 py-3 rounded-3xl shadow-md border border-blue-100 hover:shadow-xl hover:scale-105 transition duration-300"
        >
          <Car className="w-10 h-10 text-white" />
          <h1 className="text-3xl font-bold bg-white from-blue-600 to-sky-500 bg-clip-text text-transparent">
            DriveTrack
          </h1>
        </button>
      </div>

      <p className="text-3xl text-center text-gray-800 font-bold">
        Witamy w DriveTrack
        <span className="mt-2 block text-gray-600 text-base font-normal">
          Zarządzaj wydatkami swojego samochodu z łatwością
        </span>
      </p>

      {/* Pole logowania */}
      <div className="mt-10 w-full max-w-md bg-white rounded-2xl shadow-lg p-8 border border-gray">
        <h3 className="text-2xl font-semibold text-gray-800 text-left">
          Logowanie
        </h3>
        <p className="text-gray-600 text-md text-left">
          Wpisz dane, aby przejść do konta
        </p>

        <form className="flex flex-col gap-4" onSubmit={handleSubmit}>
          <div className="flex flex-col gap-2 py-6">
            <h2 className="text-md font-semibold text-gray-800 text-left mt-2">
              Email
            </h2>
            <input
              type="email"
              name="email"
              placeholder="jankowalski@email.com"
              value={formData.email}
              onChange={handleChange}
              className="px-4 py-3 bg-gray-50 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />

            <h2 className="text-md font-semibold text-gray-800 text-left mt-2">
              Hasło
            </h2>
            <input
              type="password"
              name="password"
              placeholder="••••••••"
              value={formData.password}
              onChange={handleChange}
              className="px-4 py-3 bg-gray-50 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {error && (
            <p className="text-red-600 text-center font-medium -mt-2 mb-2">
              {error}
            </p>
          )}

          <button
            type="submit"
            className="px-6 py-3 bg-blue-600 text-white text-xl font-semibold rounded-xl shadow-md hover:bg-blue-500 transition duration-300"
          >
            Zaloguj
          </button>

          <p className="text-center text-gray-600 text-sm">
            Nie posiadasz konta?{" "}
            <button
              type="button"
              onClick={() => navigate("/register")}
              className="text-blue-600 font-semibold hover:underline hover:text-blue-700 transition"
            >
              Zarejestruj się
            </button>
          </p>
        </form>
      </div>
    </div>
  );
}
