import Sidebar from "../components/Sidebar";
import Topbar from "../components/Topbar";

export default function Vehicles(){
    return(
        <div className="min-h-screen bg-gray-50 flex">
            <Sidebar />
            <div className="flex flex-col flex-1">
                    <Topbar />


                    
            </div>
        </div>


    );

}