import {Car} from "lucide-react";

export default function MainMenu() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-50 via-white to-gray-100 flex flex-row items-center justify-center">
      {/*Logo i nazwa aplikacji */ }
        <div className="flex items-center gap-3">
            <div className="w-12 h-15 bg-blue-200 rounded-full flex items-center justify-center">
                <Car className="w-10 h-10 text-blue-500" />
            </div>
        </div>
        <h1 className="text-4xl font-bold text-gray-800">DriveTrack</h1>
    </div>
  );
}
