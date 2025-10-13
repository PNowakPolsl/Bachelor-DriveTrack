import { User } from "lucide-react";

export default function Topbar() {
  return (
    <header className="w-full h-16 bg-white border-b border-gray-200 shadow-sm flex items-center justify-end px-8">
        <div className="flex items-center gap-3">
            <p className="text-gray-700 font-semibold text-lg">
                Piotr Nowak
            </p>
            <div className="w-10-h-10 rounded-full bg-blue-100 flex items-center justify-center shadow-inner">
                <User className="w-8 h-8 text-blue-600" />
            </div>
        </div>
    </header>
  );
}
