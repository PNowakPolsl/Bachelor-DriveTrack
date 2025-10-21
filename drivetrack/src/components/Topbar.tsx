import { User } from "lucide-react";
import { useNavigate } from "react-router-dom";

export default function Topbar() {
  const navigate = useNavigate();

  return (
    <header className="w-full h-16 bg-white border-b border-gray-200 shadow-sm flex items-center justify-end px-8">
        <button 
          onClick={() => navigate("/profile")}
          className="flex items-center gap-4 px-4 py-3 rounded-lg hover:bg-blue-50 transition">
            <p className="text-gray-700 font-semibold text-lg hover:text-blue-600 ">
                Piotr Nowak
            </p>
            <div className="w-8 h-8 rounded-full bg-blue-100 flex items-center justify-center shadow-inner">
                <User className="w-8 h-8 text-blue-600" />
            </div>
        </button>
    </header>
  );
}